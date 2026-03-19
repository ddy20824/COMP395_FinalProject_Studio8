using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private static CursorManager instance;
    public static CursorManager Instance { get => instance; private set => instance = value; }

    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D pointerCursor; // For buttons
    [SerializeField] private Texture2D grabCursor;    // For dragging

    [SerializeField] private Vector2 normalHotspot;
    [SerializeField] private Vector2 pointerHotspot;
    [SerializeField] private Vector2 grabHotspot;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SetNormalCursor();
    }

    public void SetNormalCursor() => ChangeCursor(normalCursor, normalHotspot);
    public void SetPointerCursor() => ChangeCursor(pointerCursor, pointerHotspot);
    public void SetGrabCursor() => ChangeCursor(grabCursor, grabHotspot);

    private void ChangeCursor(Texture2D cursorTexture, Vector2 hotSpot)
    {
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }
}