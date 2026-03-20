using UnityEngine;
using UnityEngine.InputSystem;

public class DragController : MonoBehaviour
{
    [SerializeField] private LayerMask ignoredLayers; // Ignore raycasts to the ingredient itself and other draggable items
    [SerializeField] private LayerMask surfaceLayers; // Only raycast to these layers (e.g. tables, floor) to determine where to place the ingredient
    [SerializeField] private float surfaceOffset = 0.1f; // Offset from the surface to prevent z-fighting and allow "climbing" onto tables
    [SerializeField] private SFXTypeEventChannel onSFXRequest;

    private bool isDragging = false;
    private BaseStorage currentAimedStorage;
    private Rigidbody rb;

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
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastMask) &&
                (hit.transform == transform || hit.transform.IsChildOf(transform)))
            {
                StartDrag();
            }
        }

        // 2. Update position while dragging
        if (mouse.leftButton.isPressed && isDragging)
        {
            UpdatePositionOnSurface(mouse.position.ReadValue());
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
        isDragging = true;
        if (rb != null) rb.isKinematic = true; // Force disable physics while dragging

        if (onSFXRequest != null)
            onSFXRequest.Raise(GameplaySFXType.INGR_DRAG);

        CursorManager.Instance.SetGrabCursor();
    }

    private void EndDrag()
    {
        if (rb != null) rb.isKinematic = false; // Release hand to restore physics
        if (currentAimedStorage != null)
        {
            currentAimedStorage.StoreItem(this.gameObject);
        }

        isDragging = false;

        if (onSFXRequest != null)
            onSFXRequest.Raise(GameplaySFXType.INGR_DROP);

        CursorManager.Instance.SetNormalCursor();
    }

    // Trigger Fridge highlight when hovering over it
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fridge") || other.CompareTag("GeneralStorage"))
        {
            var storage = other.GetComponentInParent<BaseStorage>();
            if (storage != null)
            {
                currentAimedStorage = storage;
                storage.ToggleHighlight(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Fridge") || other.CompareTag("GeneralStorage"))
        {
            var storage = other.GetComponentInParent<BaseStorage>();
            if (storage != null)
            {
                storage.ToggleHighlight(false);
                if (currentAimedStorage == storage)
                    currentAimedStorage = null;
            }
        }
    }
}