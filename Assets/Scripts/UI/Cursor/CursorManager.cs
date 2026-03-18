using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D pointerCursor; // For buttons
    [SerializeField] private Texture2D grabCursor;    // For dragging

    [SerializeField] private Vector2 normalHotspot;
    [SerializeField] private Vector2 pointerHotspot;
    [SerializeField] private Vector2 grabHotspot;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetNormalCursor();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetNormalCursor() => ChangeCursor(normalCursor, normalHotspot);
    public void SetPointerCursor() => ChangeCursor(pointerCursor, pointerHotspot);
    public void SetGrabCursor() => ChangeCursor(grabCursor, grabHotspot);

    private void ChangeCursor(Texture2D cursorTexture, Vector2 hotSpot)
    {
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }
}