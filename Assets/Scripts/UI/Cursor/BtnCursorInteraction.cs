using UnityEngine;
using UnityEngine.EventSystems;

public class BtnCursorInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorManager.Instance.SetPointerCursor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.Instance.SetNormalCursor();
    }

    private void OnDisable()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormalCursor();
    }
}