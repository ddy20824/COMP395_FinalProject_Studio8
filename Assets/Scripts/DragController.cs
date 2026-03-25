using UnityEngine;
using UnityEngine.InputSystem;

public class DragController : MonoBehaviour
{
    [SerializeField] private LayerMask ignoredLayers; // Ignore raycasts to the ingredient itself and other draggable items
    [SerializeField] private LayerMask surfaceLayers; // Only raycast to these layers (e.g. tables, floor) to determine where to place the ingredient
    [SerializeField] private float surfaceOffset = 0.1f; // Offset from the surface to prevent z-fighting and allow "climbing" onto tables
    [SerializeField] private float returnToStartDuration = 0.2f;
    [SerializeField] private SFXTypeEventChannel onSFXRequest;

    private bool isDragging = false;
    private BaseStorage currentAimedStorage;
    private Rigidbody rb;
    private CookController currentAimedStove;
    private CookController currentHoveredStove;
    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;
    private Coroutine returnToStartRoutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        // 1. Start dragging when left mouse button is pressed on the ingredient
        if (mouse.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            int raycastMask = ~ignoredLayers.value;
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastMask, QueryTriggerInteraction.Ignore) &&
                (hit.transform == transform || hit.transform.IsChildOf(transform)))
            {
                StartDrag();
            }
        }

        // 2. Update position while dragging
        if (mouse.leftButton.isPressed && isDragging)
        {
            UpdatePositionOnSurface(mouse.position.ReadValue());
            UpdateStoveHoverByRaycast(mouse.position.ReadValue());
        }

        // 3. Release
        if (mouse.leftButton.wasReleasedThisFrame && isDragging)
        {
            EndDrag();
        }
    }

    private void UpdatePositionOnSurface(Vector2 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        // Raycast into the scene, only hitting surfaceLayers (tables, floor)
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, surfaceLayers))
        {
            // Set the ingredient position to the hit point + (surface normal * offset)
            // This allows the ingredient to "climb" onto surfaces
            transform.position = hit.point + (hit.normal * surfaceOffset);

            // If you want the ingredient to align with the surface angle (optional), you can add this line:
            // transform.up = hit.normal;
        }
    }

    public void StartDrag()
    {
        ClearAimedStorage();
        ClearAimedStove();

        isDragging = true;
        dragStartPosition = transform.position;
        dragStartRotation = transform.rotation;

        if (returnToStartRoutine != null)
        {
            StopCoroutine(returnToStartRoutine);
            returnToStartRoutine = null;
        }

        if (rb != null) rb.isKinematic = true; // Force disable physics while dragging

        if (onSFXRequest != null)
            onSFXRequest.Raise(GameplaySFXType.INGR_DRAG);

        CursorManager.Instance.SetGrabCursor();
    }

    private void EndDrag()
    {
        if (rb != null) rb.isKinematic = false; // Release hand to restore physics

        bool canTryStove = currentAimedStove != null && currentAimedStove.CanAcceptIngredient();
        if (canTryStove)
        {
            bool success = currentAimedStove.TryAddIngredient(this.gameObject);
            if (!success)
                returnToStartRoutine = StartCoroutine(ReturnToDragStartPoint());
        }
        else if (currentAimedStorage != null)
        {
            currentAimedStorage.StoreItem(this.gameObject);
        }

        isDragging = false;
        ClearAimedStorage();
        ClearAimedStove();

        if (onSFXRequest != null)
            onSFXRequest.Raise(GameplaySFXType.INGR_DROP);

        CursorManager.Instance.SetNormalCursor();
    }

    // Trigger Fridge highlight when hovering over it
    private void OnTriggerEnter(Collider other)
    {
        if (!isDragging) return;

        if (other.CompareTag("Fridge") || other.CompareTag("GeneralStorage"))
        {
            var storage = other.GetComponentInParent<BaseStorage>();
            if (storage != null)
            {
                SetAimedStorage(storage);
            }
        }
        if (other.CompareTag("Ground"))
        {
            GetComponent<IngredientController>().SetOnFloor(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isDragging) return;

        if (other.CompareTag("Fridge") || other.CompareTag("GeneralStorage"))
        {
            var storage = other.GetComponentInParent<BaseStorage>();
            if (storage != null)
            {
                if (currentAimedStorage == storage)
                    ClearAimedStorage();
            }
        }
        if (other.CompareTag("Ground"))
        {
            GetComponent<IngredientController>().SetOnFloor(false);
        }
    }

    private void UpdateStoveHoverByRaycast(Vector2 mousePos)
    {
        if (Camera.main == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        int raycastMask = ~ignoredLayers.value;

        CookController hitStove = null;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastMask))
        {
            hitStove = hit.transform.GetComponentInParent<CookController>();
        }

        if (hitStove == currentHoveredStove)
        {
            currentAimedStove = hitStove;
            return;
        }

        if (currentHoveredStove != null)
            currentHoveredStove.SetDraggingHoverState(false);

        currentHoveredStove = hitStove;
        currentAimedStove = hitStove;

        if (currentHoveredStove != null)
            currentHoveredStove.SetDraggingHoverState(true);
    }

    private void SetAimedStorage(BaseStorage storage)
    {
        if (storage == currentAimedStorage)
            return;

        ClearAimedStorage();
        currentAimedStorage = storage;
        currentAimedStorage.ToggleHighlight(true);
    }

    private void ClearAimedStorage()
    {
        if (currentAimedStorage != null)
            currentAimedStorage.ToggleHighlight(false);

        currentAimedStorage = null;
    }

    private void ClearAimedStove()
    {
        if (currentHoveredStove != null)
            currentHoveredStove.SetDraggingHoverState(false);

        currentHoveredStove = null;
        currentAimedStove = null;
    }

    private System.Collections.IEnumerator ReturnToDragStartPoint()
    {
        if (rb != null)
            rb.isKinematic = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < returnToStartDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / returnToStartDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            transform.position = Vector3.Lerp(startPos, dragStartPosition, t);
            transform.rotation = Quaternion.Slerp(startRot, dragStartRotation, t);
            yield return null;
        }

        transform.position = dragStartPosition;
        transform.rotation = dragStartRotation;

        if (rb != null)
            rb.isKinematic = false;

        returnToStartRoutine = null;
    }
}