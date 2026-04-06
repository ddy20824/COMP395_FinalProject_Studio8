using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameObjectPointerCursor : MonoBehaviour
{
    public static GameObjectPointerCursor CurrentHover;

    private void Update()
    {
        if (Camera.main == null || Mouse.current == null) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObjectPointerCursor hovered = hit.collider.GetComponentInParent<GameObjectPointerCursor>();

            if (hovered != null)
            {
                CurrentHover = hovered;
                CursorManager.Instance.SetPointerCursor();
                return;
            }
        }

        CurrentHover = null;
        CursorManager.Instance.SetNormalCursor();
    }
}