using System.Collections;
using UnityEngine;

public class FridgeController : BaseStorage
{
    [SerializeField]
    private Animator animator;

    [Header("Camera Focus")]
    [SerializeField]
    private CameraFocusEventChannel cameraFocusChannel;
    [SerializeField]
    private Transform cameraSlot;

    [Header("Exploded View Settings")]
    [SerializeField] private Transform explodeOrigin;

    [Header("Events")]
    [SerializeField] private SFXTypeEventChannel onSFXRequest;

    [Header("Power Outage Visuals")]
    [SerializeField] private Light fridgeInternalLight;
    [SerializeField] private MeshRenderer fridgeBodyRenderer;
    private Color originalEmissionColor;
    private Color originalInternalLightColor;

    private float itemSpacing = 0.3f;
    private int columns = 3;

    private bool isOpen = false;
    private bool isPowerOutage = false;

    void Awake()
    {
        if (fridgeBodyRenderer != null)
        {
            originalEmissionColor = fridgeBodyRenderer.material.GetColor("_EmissionColor");
        }
        if (fridgeInternalLight != null)
        {
            originalInternalLightColor = fridgeInternalLight.color;
        }

        LevelDataContent levelData = gameConfig.allLevels.Find(ld => ld.levelName == GameSession.CurrentLevelIndex);
        maxCapacity = levelData.capacityOfFridge;
    }


    protected override void Start()
    {
        base.Start();
        LevelDataContent levelData = gameConfig.allLevels.Find(ld => ld.levelName == GameSession.CurrentLevelIndex);
        if (levelData.hasPowerOutageEvent)
        {
            StartCoroutine(PowerOutageRoutine());
        }
    }

    private IEnumerator PowerOutageRoutine()
    {
        while (true)
        {
            float waitBeforeOutage = Random.Range(30f, 50f);
            yield return new WaitForSeconds(waitBeforeOutage);

            StartPowerOutage();

            float outageDuration = Random.Range(10f, 15f);
            yield return new WaitForSeconds(outageDuration);

            StopPowerOutage();
        }
    }

    private void StartPowerOutage()
    {
        isPowerOutage = true;

        if (fridgeInternalLight != null)
        {
            fridgeInternalLight.color = Color.red;
        }

        if (fridgeBodyRenderer != null)
        {
            Color targetColor = new Color(0.5f, 0, 0);
            fridgeBodyRenderer.material.SetColor("_EmissionColor", targetColor);
            fridgeBodyRenderer.material.EnableKeyword("_EMISSION");
        }
    }

    private void StopPowerOutage()
    {
        isPowerOutage = false;

        if (fridgeInternalLight != null)
        {
            fridgeInternalLight.color = originalInternalLightColor;
        }

        if (fridgeBodyRenderer != null)
        {
            Color targetColor = originalEmissionColor;
            fridgeBodyRenderer.material.SetColor("_EmissionColor", targetColor);
            fridgeBodyRenderer.material.DisableKeyword("_EMISSION");
        }

    }
    protected override void HandleMouseInput(Vector2 screenPoint)
    {
        if (Camera.main == null)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            GameObject hitObj = hit.transform.gameObject;

            // Situation A: Fridge is open and clicked on an ingredient inside the fridge
            if (isOpen && storedItems.Contains(hitObj))
            {
                TakeOutItem(hitObj);
                return;
            }

            // Situation B: Clicked on the fridge body (or clicked on air while fridge is open)
            if (hit.collider == selfCollider || (isOpen && hit.collider != null))
            {
                ToggleFridge();
                HideExplodedView();
            }
        }
    }

    public void ToggleFridge()
    {
        isOpen = !isOpen;

        animator.SetBool("isOpen", isOpen);

        if (isOpen)
        {
            onSFXRequest.Raise(GameplaySFXType.FRIDGE_OPEN);
            StartCoroutine(DelayedShowExplodedView(0.5f));
        }
        else
        {
            onSFXRequest.Raise(GameplaySFXType.FRIDGE_CLOSE);
            HideExplodedView();
            ToggleHighlight(false);
        }

        // Camera Focus when open fridge to show the ingredients, and turn back to original position when close fridge
        CameraFocusData data = new CameraFocusData
        {
            targetTransform = cameraSlot,
            isFocusing = isOpen
        };

        cameraFocusChannel.Raise(data);
    }

    private IEnumerator DelayedShowExplodedView(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isOpen) ShowExplodedView();
    }

    protected override void OnItemStored(GameObject item)
    {
        if (item.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        if (item.TryGetComponent(out Collider col))
        {
            col.enabled = false;
        }

        if (item.TryGetComponent(out DragController drag))
        {
            drag.enabled = false;
        }

        if (item.TryGetComponent(out IngredientController ingredient))
        {
            ingredient.SetInFridge(!isPowerOutage);
        }

        item.SetActive(false);
        Debug.Log($"Fridge save: {item.name}");
    }

    public override void ToggleHighlight(bool show)
    {
        base.ToggleHighlight(show);
        animator.SetBool("isNear", show);
    }

    private void ShowExplodedView()
    {
        for (int i = 0; i < storedItems.Count; i++)
        {
            GameObject item = storedItems[i];
            item.SetActive(true);

            int row = i / columns;
            int col = i % columns;

            Vector3 targetPos = explodeOrigin.position +
                                explodeOrigin.right * (col - (columns - 1) / 2f) * itemSpacing +
                                explodeOrigin.up * (-row * itemSpacing);

            StartCoroutine(AnimateToPosition(item.transform, targetPos, true));
        }
    }

    private void HideExplodedView()
    {
        foreach (var item in storedItems)
        {
            StartCoroutine(AnimateToPosition(item.transform, storageRoot.position, false));
        }
    }

    private IEnumerator AnimateToPosition(Transform target, Vector3 targetPos, bool isOpening)
    {
        float elapsed = 0f;
        float duration = 0.5f;
        Vector3 startPos = target.position;
        Quaternion startRot = target.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            t = Mathf.SmoothStep(0, 1, t);

            target.position = Vector3.Lerp(startPos, targetPos, t);

            if (isOpening)
                target.rotation = Quaternion.Lerp(startRot, explodeOrigin.rotation, t);

            yield return null;
        }

        if (!isOpening)
        {
            target.gameObject.SetActive(false);
        }
        else
        {
            if (target.TryGetComponent(out Collider col)) col.enabled = true;
        }
    }

    protected override void OnItemRemoved(GameObject item)
    {
        if (isOpen)
        {
            ToggleFridge();
        }
        if (item.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }
        if (item.TryGetComponent(out Collider col))
        {
            col.enabled = true;
        }
        if (item.TryGetComponent(out DragController drag))
        {
            drag.enabled = true;
            drag.StartDrag();
        }
        if (item.TryGetComponent(out IngredientController ingredient))
        {
            ingredient.SetInFridge(false);
        }
        item.SetActive(true);
    }
}
