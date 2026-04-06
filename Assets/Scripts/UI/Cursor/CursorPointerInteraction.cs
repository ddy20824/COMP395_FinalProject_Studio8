using UnityEngine;
using UnityEngine.EventSystems;

public class CursorPointerInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("ENTER");
        if (!DragController.IsDraggingEnabled)
        {
            CursorManager.Instance.SetPointerCursor();
        }            
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!DragController.IsDraggingEnabled)
        {
            CursorManager.Instance.SetNormalCursor();
        }
    }

    private void OnDisable()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormalCursor();
    }
}