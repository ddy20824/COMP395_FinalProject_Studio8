using UnityEngine;
using UnityEngine.InputSystem;

public class DragController : MonoBehaviour
{
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
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                mZCoord = Camera.main.WorldToScreenPoint(transform.position).z;
                mOffset = transform.position - GetMouseWorldPos();
                isDragging = true;
            }
        }

        if (mouse.leftButton.isPressed && isDragging)
        {
            transform.position = GetMouseWorldPos() + mOffset;
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Mouse.current.position.ReadValue();
        mousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}