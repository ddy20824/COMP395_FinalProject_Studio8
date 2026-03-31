using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CookController : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private SFXTypeEventChannel onSFXRequest;
    [Header("References")]
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private Transform panDropPoint;
    [SerializeField] private Transform trashPoint;
    [SerializeField] private float dishOffsetY = 0.5f; // Dish spawn height offset from the panDropPoint

    [Header("Animation Settings (New)")]
    [SerializeField] private Transform panMesh;
    [SerializeField] private float panShakeSpeed = 40f;
    [SerializeField] private float panShakeAmount = 0.015f;

    [Header("Outline Settings")]
    [SerializeField] private Color readyOutlineColor = Color.red;
    [SerializeField] private float outlineWidth = 10f;
    [SerializeField] private float readyPulseMinOutlineWidth = 8f;
    [SerializeField] private float readyPulseMaxOutlineWidth = 12f;
    [SerializeField] private float readyPulseSpeed = 4f;

    [Header("Fire Settings")]
    [SerializeField] private GameObject fireObject;
    [SerializeField] private float readyFirePulseMinScale = 0.92f;
    [SerializeField] private float readyFirePulseMaxScale = 1.08f;

    [Header("UI References")]
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private Transform ingredientIconContainer;
    [SerializeField] private GameObject ingredientIconPrefab;
    [SerializeField] private float ingredientIconsYOffset = 1.2f;
    [SerializeField] private float ingredientIconMaxRowWidth = 0.35f;
    [SerializeField] private float ingredientIconIdealSpacing = 0.25f;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private Transform progressBarRoot;
    [SerializeField] private float progressBarYOffset = 1.6f;

    [Header("Settings")]
    public int maxIngredients = 3;

    [Header("UI Animation Settings")]
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float globalIconScale = 5.0f;

    private List<IngredientMapping> currentIngredients = new List<IngredientMapping>();
    private List<GameObject> spawnedIngredientIcons = new List<GameObject>();
    private bool isCooking = false;
    private GameObject currentFinishedDish;
    private float bobTimer;

    // Hover state management
    private bool isHoveredByDraggingIngredient; // when dragging an ingredient over the stove
    private bool isMouseHovering;               // when mouse is simply hovering (without dragging)
    private bool IsHovered => isHoveredByDraggingIngredient || isMouseHovering;

    private Outline stoveOutline;
    private Color defaultOutlineColor = Color.white;
    private Vector3 fireBaseScale = Vector3.one;

    void Start()
    {
        stoveOutline = GetComponentInChildren<Outline>();
        if (stoveOutline != null)
        {
            defaultOutlineColor = stoveOutline.OutlineColor;
            stoveOutline.enabled = false;
        }

        if (progressBarRoot == null && progressBarFill != null && progressBarFill.transform.parent != null)
            progressBarRoot = progressBarFill.transform.parent;

        if (fireObject != null)
        {
            fireBaseScale = fireObject.transform.localScale;
            fireObject.SetActive(false);
        }

        ResetUI();
        RefreshStoveVisualState();
    }

    void Update()
    {
        if (PauseMenu.Instance.IsPause) return; // Temporary UI fix for icon disappearing during game pause.

        var mouse = Mouse.current;
        if (mouse == null) return;

        // Hover detection using raycast from mouse position
        Vector2 mousePosition = mouse.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        bool currentFrameHover = false;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                currentFrameHover = true;

                // Start cooking on left-click
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    if (!isCooking && currentIngredients.Count > 0 && currentFinishedDish == null)
                    {
                        StartCooking();
                    }
                }
            }
        }

        // When hover state changes, refresh visual
        if (isMouseHovering != currentFrameHover)
        {
            isMouseHovering = currentFrameHover;
            RefreshStoveVisualState();
        }

        UpdateReadyPulseAnimation();
    }

    void LateUpdate()
    {
        UpdateFloatingUITransforms();
    }

    void OnDisable()
    {
        ClearSpawnedIngredientIcons();
        if (stoveOutline != null)
            stoveOutline.enabled = false;

        if (fireObject != null)
        {
            fireObject.transform.localScale = fireBaseScale;
            fireObject.SetActive(false);
        }
    }

    public bool TryAddIngredient(GameObject obj)
    {
        var ingredient = obj.GetComponentInParent<IngredientController>();
        if (ingredient == null)
            return false;

        if (ingredient.IsRotted())
        {
            return false;
        }

        if (!CanAcceptIngredient())
            return false;

        IngredientType type = ingredient.GetIngredientType();
        IngredientMapping mapping = orderManager.gameConfig.ingredientMappings.Find(x => x.type == type);

        if (mapping != null)
        {
            currentIngredients.Add(mapping);
            UpdateIngredientUI();
            RefreshStoveVisualState();
        }

        Destroy(ingredient.gameObject);
        return true;
    }

    private void UpdateIngredientUI()
    {
        if (uiRoot != null)
            uiRoot.SetActive(true);

        if (ingredientIconContainer != null)
            ingredientIconContainer.gameObject.SetActive(currentIngredients.Count > 0);

        RebuildIngredientIcons();
    }

    private void RebuildIngredientIcons()
    {
        ClearSpawnedIngredientIcons();

        if (ingredientIconContainer == null || ingredientIconPrefab == null || currentIngredients.Count == 0)
            return;

        float currentMaxRowWidth = ingredientIconMaxRowWidth;
        float spacing = 0f;
        if (currentIngredients.Count > 1)
        {
            spacing = Mathf.Min(ingredientIconIdealSpacing, currentMaxRowWidth / (currentIngredients.Count - 1));
        }

        for (int i = 0; i < currentIngredients.Count; i++)
        {
            GameObject iconRoot = Instantiate(ingredientIconPrefab, ingredientIconContainer);

            SpriteRenderer[] renderers = iconRoot.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in renderers)
            {
                if (sr.gameObject.name == "Icon")
                {
                    sr.sprite = currentIngredients[i].sprite;
                    break;
                }
            }

            float xOffset = (i - (currentIngredients.Count - 1) / 2f) * spacing;
            iconRoot.transform.localPosition = new Vector3(xOffset, 0f, 0f);
            iconRoot.transform.localRotation = Quaternion.identity;
            iconRoot.transform.localScale = Vector3.one * (currentIngredients[i].iconScale * globalIconScale);

            spawnedIngredientIcons.Add(iconRoot);
        }
    }

    private void UpdateFloatingUITransforms()
    {
        if (panDropPoint == null || Camera.main == null)
            return;

        bobTimer += Time.deltaTime;
        float bobOffset = Mathf.Sin(bobTimer * bobFrequency) * bobAmplitude;

        if (ingredientIconContainer != null && ingredientIconContainer.gameObject.activeSelf)
        {
            ingredientIconContainer.position = panDropPoint.position + Vector3.up * (ingredientIconsYOffset + bobOffset);
            FaceCamera(ingredientIconContainer, Camera.main.transform);
        }

        if (progressBarRoot != null && progressBarRoot.gameObject.activeSelf)
        {
            progressBarRoot.position = panDropPoint.position + Vector3.up * progressBarYOffset;
            FaceCamera(progressBarRoot, Camera.main.transform);
        }
    }

    public void StartCooking()
    {
        if (isCooking || currentFinishedDish != null || currentIngredients.Count == 0) return;

        onSFXRequest.Raise(GameplaySFXType.COOKING_START);
        HideIngredientIconsAtCookStart();
        StartCoroutine(CookRoutine());
    }

    private IEnumerator CookRoutine()
    {
        GameConfig config = orderManager != null ? orderManager.gameConfig : null;
        if (config == null)
        {
            Debug.LogWarning("CookController: Missing GameConfig reference from OrderManager.");
            RefreshStoveVisualState();
            yield break;
        }

        isCooking = true;
        RefreshStoveVisualState();
        onSFXRequest.Raise(GameplaySFXType.IS_COOKING);

        if (progressBarFill != null)
            progressBarFill.fillAmount = 0f;

        if (progressBarRoot != null)
            progressBarRoot.gameObject.SetActive(true);

        if (uiRoot != null)
            uiRoot.SetActive(true);

        bool isMatch = orderManager.CheckMatchAndReserve(currentIngredients, out DishTypeMapping matchedDish, out int targetOrderIndex);
        float resolvedCookTime = isMatch && matchedDish != null
            ? Mathf.Max(0.1f, matchedDish.cookingTime)
            : Mathf.Max(0.1f, config.failedDishCookTime);

        // Start cooking animation and wait for it to finish
        Vector3 originalPanLocalPos = Vector3.zero;
        if (panMesh != null)
            originalPanLocalPos = panMesh.localPosition;

        float timer = 0f;
        while (timer < resolvedCookTime)
        {
            timer += Time.deltaTime;
            if (progressBarFill != null)
                progressBarFill.fillAmount = Mathf.Clamp01(timer / resolvedCookTime);

            // Pan shaking animation during cooking
            if (panMesh != null)
            {
                float shakeY = Mathf.Sin(timer * panShakeSpeed) * panShakeAmount;
                panMesh.localPosition = originalPanLocalPos + new Vector3(0, shakeY, 0);
            }

            yield return null;
        }

        // End cooking animation
        if (panMesh != null)
        {
            panMesh.localPosition = originalPanLocalPos;
        }

        GameObject prefabToSpawn = isMatch && matchedDish != null ? matchedDish.prefab : config.failureDishPrefab;
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("CookController: Missing dish prefab to spawn.");
            currentIngredients.Clear();
            isCooking = false;
            ResetUI();
            RefreshStoveVisualState();
            yield break;
        }

        // Spawn the cooked dish above the pan
        currentFinishedDish = Instantiate(prefabToSpawn, panDropPoint.position + Vector3.up * dishOffsetY, panDropPoint.rotation);
        if (currentFinishedDish.TryGetComponent(out CookedDish dishController))
        {
            DishType type = isMatch ? matchedDish.type : DishType.FailedDish;
            dishController.Setup(isMatch, targetOrderIndex, orderManager, this, trashPoint, type);
        }

        currentIngredients.Clear();
        isCooking = false;
        ResetUI();
        RefreshStoveVisualState();
        onSFXRequest.Raise(GameplaySFXType.COOKING_END);
    }

    public bool ClearFinishedDish(Transform trashTargetPoint = null)
    {
        if (currentFinishedDish == null)
            return false;

        if (trashTargetPoint != null && currentFinishedDish.TryGetComponent(out CookedDish dishController))
        {
            if (dishController.TryFlyToTrash(trashTargetPoint))
            {
                currentFinishedDish = null;
                RefreshStoveVisualState();
                return true;
            }
        }

        Destroy(currentFinishedDish);
        currentFinishedDish = null;
        RefreshStoveVisualState();
        return true;
    }

    public void ReleaseFinishedDishReference(GameObject dishObject)
    {
        if (dishObject != null && currentFinishedDish == dishObject)
        {
            currentFinishedDish = null;
            RefreshStoveVisualState();
        }
    }

    public void SetDraggingHoverState(bool isHovering)
    {
        if (isHoveredByDraggingIngredient == isHovering)
            return;

        isHoveredByDraggingIngredient = isHovering;
        RefreshStoveVisualState();
    }

    private void ResetUI()
    {
        ClearSpawnedIngredientIcons();

        if (ingredientIconContainer != null)
            ingredientIconContainer.gameObject.SetActive(false);

        if (progressBarFill != null)
            progressBarFill.fillAmount = 0f;

        if (progressBarRoot != null)
            progressBarRoot.gameObject.SetActive(false);

        if (uiRoot != null && currentIngredients.Count == 0)
            uiRoot.SetActive(false);
    }

    private void HideIngredientIconsAtCookStart()
    {
        ClearSpawnedIngredientIcons();

        if (ingredientIconContainer != null)
            ingredientIconContainer.gameObject.SetActive(false);
    }

    private void ClearSpawnedIngredientIcons()
    {
        for (int i = 0; i < spawnedIngredientIcons.Count; i++)
        {
            if (spawnedIngredientIcons[i] != null)
                Destroy(spawnedIngredientIcons[i]);
        }

        spawnedIngredientIcons.Clear();
    }

    private void FaceCamera(Transform target, Transform cameraTransform)
    {
        target.LookAt(
            target.position + cameraTransform.rotation * Vector3.forward,
            cameraTransform.rotation * Vector3.up
        );
    }

    private bool CanStartCooking()
    {
        return !isCooking && currentFinishedDish == null && currentIngredients.Count > 0;
    }

    public bool CanAcceptIngredient()
    {
        return !isCooking && currentFinishedDish == null && currentIngredients.Count < maxIngredients;
    }

    private void RefreshStoveVisualState()
    {
        bool canAcceptIngredient = CanAcceptIngredient();
        bool canStartCooking = CanStartCooking();

        if (stoveOutline != null)
        {
            stoveOutline.OutlineWidth = outlineWidth;

            if (IsHovered && (canAcceptIngredient || canStartCooking))
            {
                stoveOutline.enabled = true;
                stoveOutline.OutlineColor = canStartCooking ? readyOutlineColor : defaultOutlineColor;
            }
            else
            {
                stoveOutline.enabled = false;
            }
        }

        if (fireObject != null)
        {
            bool isHoverReady = IsHovered && canStartCooking;
            bool shouldShowFire = isHoverReady || isCooking;

            if (fireObject.activeSelf != shouldShowFire)
                fireObject.SetActive(shouldShowFire);

            if (!shouldShowFire)
                fireObject.transform.localScale = fireBaseScale;
        }
    }

    private void UpdateReadyPulseAnimation()
    {
        bool shouldPulseReadyState = CanStartCooking() && !isCooking;
        float pulseT = 0.5f + 0.5f * Mathf.Sin(Time.time * readyPulseSpeed);

        if (stoveOutline != null && stoveOutline.enabled && IsHovered && shouldPulseReadyState)
        {
            stoveOutline.OutlineWidth = Mathf.Lerp(readyPulseMinOutlineWidth, readyPulseMaxOutlineWidth, pulseT);
        }

        if (fireObject != null && fireObject.activeSelf)
        {
            if (shouldPulseReadyState)
            {
                float firePulseScale = Mathf.Lerp(readyFirePulseMinScale, readyFirePulseMaxScale, pulseT);
                fireObject.transform.localScale = fireBaseScale * firePulseScale;
            }
            else
            {
                fireObject.transform.localScale = fireBaseScale;
            }
        }
    }
}