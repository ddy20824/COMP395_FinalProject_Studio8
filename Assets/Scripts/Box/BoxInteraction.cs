using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class BoxInteraction : MonoBehaviour
{
    public GameObject openedBox;
    public GameObject closedBox;

    [Header("Ingredient Content")]
    public Transform ingredientSpawnPoint;
    public float ingredientSpacing = 0.3f;
    public float ingredientSpawnDelay = 0.2f;

    [Header("Open Conditions")]
    public LayerMask boxInsideCheckLayer;
    private Vector3 insideCheckPadding = new Vector3(0.02f, 0.02f, 0.02f);

    private float topCheckHeight = 0.3f;
    private Vector3 topCheckPadding = new Vector3(0.05f, 0f, 0.05f);

    private Collider selfCollider;
    private Rigidbody boxRigidbody;
    private GameObject ingredientPrefab;
    private int ingredientCount;
    private bool hasOpened;
    private Coroutine spawnDelayCoroutine;
    private readonly List<GameObject> lockedIngredients = new List<GameObject>();

    [Header("Events")]
    [SerializeField] private SFXTypeEventChannel onSFXRequest;
    private void Awake()
    {
        selfCollider = GetComponent<Collider>();
        boxRigidbody = GetComponentInParent<Rigidbody>();
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryOpenByScreenPoint(Mouse.current.position.ReadValue());
        }
    }

    private void TryOpenByScreenPoint(Vector2 screenPoint)
    {
        if (Camera.main == null)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider != null && hit.collider == selfCollider)
            {
                OpenBox();
            }
        }
    }

    private void OpenBox()
    {
        if (!hasOpened)
        {
            if (HasObjectOnTop())
            {
                return;
            }

            if (openedBox != null)
            {
                openedBox.SetActive(true);
                onSFXRequest.Raise(GameplaySFXType.UNBOX);
            }

            if (closedBox != null)
            {
                closedBox.SetActive(false);
            }

            if (spawnDelayCoroutine != null)
            {
                StopCoroutine(spawnDelayCoroutine);
            }
            spawnDelayCoroutine = StartCoroutine(RevealSpawnedIngredientsOnMouseRelease());
            hasOpened = true;
            return;
        }

        if (!HasObjectInsideBoxVolume())
        {
            Destroy(gameObject);
            onSFXRequest.Raise(GameplaySFXType.TRASH_INTO);
        }
    }

    private bool HasObjectInsideBoxVolume()
    {
        if (selfCollider == null)
        {
            return false;
        }

        Bounds bounds = selfCollider.bounds;
        Vector3 halfExtents = new Vector3(
            Mathf.Max(0.01f, bounds.extents.x - insideCheckPadding.x),
            Mathf.Max(0.01f, bounds.extents.y - insideCheckPadding.y),
            Mathf.Max(0.01f, bounds.extents.z - insideCheckPadding.z));

        int layerMask = boxInsideCheckLayer.value == 0 ? Physics.AllLayers : boxInsideCheckLayer.value;
        Collider[] hits = Physics.OverlapBox(bounds.center, halfExtents, Quaternion.identity, layerMask, QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            if (hit == selfCollider || hit.transform.IsChildOf(transform))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    public void ConfigureBoxContents(GameObject contentPrefab, int capacity)
    {
        ingredientPrefab = contentPrefab;
        ingredientCount = Mathf.Max(0, capacity);

        if (boxRigidbody != null)
        {
            boxRigidbody.isKinematic = true;
        }
        SpawnIngredientsIfNeeded();
    }

    private void SpawnIngredientsIfNeeded()
    {
        if (ingredientPrefab == null || ingredientCount <= 0)
        {
            return;
        }

        Transform anchor = ingredientSpawnPoint != null
            ? ingredientSpawnPoint
            : transform;

        int columns = Mathf.CeilToInt(Mathf.Sqrt(ingredientCount));
        float half = (columns - 1) * 0.5f;

        for (int i = 0; i < ingredientCount; i++)
        {
            int row = i / columns;
            int col = i % columns;

            Vector3 localOffset = new Vector3((col - half) * ingredientSpacing, 0f, row * ingredientSpacing);
            Vector3 spawnPos = anchor.TransformPoint(localOffset);
            GameObject spawned = Instantiate(ingredientPrefab, spawnPos, anchor.rotation);
            HideIngredientInteraction(spawned);
        }
    }

    private void HideIngredientInteraction(GameObject ingredient)
    {
        if (ingredient == null)
        {
            return;
        }

        SetIngredientInteractionEnabled(ingredient, false);
        lockedIngredients.Add(ingredient);
    }

    private void RevealSpawnedIngredients()
    {
        for (int i = 0; i < lockedIngredients.Count; i++)
        {
            if (lockedIngredients[i] != null)
            {
                SetIngredientInteractionEnabled(lockedIngredients[i], true);
            }
        }

        lockedIngredients.Clear();
    }

    private static void SetIngredientInteractionEnabled(GameObject ingredient, bool enabled)
    {
        DragController[] dragControllers = ingredient.GetComponentsInChildren<DragController>(true);
        for (int i = 0; i < dragControllers.Length; i++)
        {
            dragControllers[i].SetDragInteractionEnabled(enabled);
        }

        IngredientController[] ingredientControllers = ingredient.GetComponentsInChildren<IngredientController>(true);
        for (int i = 0; i < ingredientControllers.Length; i++)
        {
            ingredientControllers[i].SetHoverInteractionEnabled(enabled);
        }
    }

    private IEnumerator RevealSpawnedIngredientsOnMouseRelease()
    {
        // Wait for left mouse release so the opening click cannot immediately pick an ingredient.
        while (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            yield return null;
        }

        RevealSpawnedIngredients();
        spawnDelayCoroutine = null;
    }

    private bool HasObjectOnTop()
    {
        if (!TryGetTopCheckBox(out Vector3 center, out Vector3 halfExtents))
        {
            return false;
        }

        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, Physics.AllLayers, QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            if (hit == selfCollider || hit.transform.IsChildOf(transform))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private bool TryGetTopCheckBox(out Vector3 center, out Vector3 halfExtents)
    {
        Collider targetCollider = selfCollider != null ? selfCollider : GetComponent<Collider>();
        if (targetCollider == null)
        {
            center = Vector3.zero;
            halfExtents = Vector3.zero;
            return false;
        }

        Bounds bounds = targetCollider.bounds;
        halfExtents = new Vector3(
            Mathf.Max(0.01f, bounds.extents.x - topCheckPadding.x),
            Mathf.Max(0.01f, topCheckHeight * 0.5f),
            Mathf.Max(0.01f, bounds.extents.z - topCheckPadding.z));

        center = bounds.center + Vector3.up * (bounds.extents.y + halfExtents.y);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (!TryGetTopCheckBox(out Vector3 center, out Vector3 halfExtents))
        {
            return;
        }

        Gizmos.color = HasObjectOnTop() ? Color.red : Color.green;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
    }
}
