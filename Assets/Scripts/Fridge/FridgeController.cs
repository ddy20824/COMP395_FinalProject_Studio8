using UnityEngine;
using UnityEngine.InputSystem;

public class FridgeController : BaseStorage
{
    [SerializeField]
    private Animator animator;

    [Header("Camera Focus")]
    [SerializeField]
    private CameraFocusEventChannel cameraFocusChannel;
    [SerializeField]
    private Transform cameraSlot;

    private Collider selfCollider;
    private bool isOpen = false;

    void Start()
    {
        selfCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ControllerDoorByScreenPoint(Mouse.current.position.ReadValue());
        }
    }

    private void ControllerDoorByScreenPoint(Vector2 screenPoint)
    {
        if (Camera.main == null)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != null && hit.collider == selfCollider)
            {
                ToggleFridge();
            }
        }
    }

    public void ToggleFridge()
    {
        isOpen = !isOpen;

        animator.SetBool("isOpen", isOpen);

        if (isOpen)
        {
            // TODO: Trigger the "exploded view" of ingredients inside the fridge, e.g. by enabling a child GameObject with a special layout and animation, or by sending an event to the ingredients to move to their "exploded" positions.
        }
        else
        {
            // TODO: Hide the "exploded view" and move ingredients back to their original positions inside the fridge.
        }

        // Camera Focus when open fridge to show the ingredients, and turn back to original position when close fridge
        CameraFocusData data = new CameraFocusData
        {
            targetTransform = cameraSlot,
            isFocusing = isOpen
        };

        cameraFocusChannel.Raise(data);
    }

    protected override void OnItemStored(GameObject item)
    {
        item.SetActive(false);
        Debug.Log($"Fridge save: {item.name}");
    }

    public override void ToggleHighlight(bool show)
    {
        base.ToggleHighlight(show);
        animator.SetBool("isNear", show);
    }
}
