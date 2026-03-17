using UnityEngine;
using UnityEngine.InputSystem;

public class DragController : MonoBehaviour
{
    [SerializeField] private LayerMask ignoredLayers;
    [SerializeField] private SFXTypeEventChannel onSFXRequest; // Event channel to request sound effects

    private Vector3 mOffset;
    private float mZCoord;
    private bool isDragging = false;

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            int raycastMask = ~ignoredLayers.value;
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastMask) &&
                (hit.transform == transform || hit.transform.IsChildOf(transform)))
            {
                mZCoord = Camera.main.WorldToScreenPoint(transform.position).z;
                mOffset = transform.position - GetMouseWorldPos();
                isDragging = true;
                onSFXRequest.Raise(SFXType.IngredientDrag);

            }
        }

        if (mouse.leftButton.isPressed && isDragging)
        {
            transform.position = GetMouseWorldPos() + mOffset;
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
            onSFXRequest.Raise(SFXType.IngredientDrop);
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Mouse.current.position.ReadValue();
        mousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}