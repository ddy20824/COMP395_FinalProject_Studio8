using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D pointerCursor; // For buttons
    [SerializeField] private Texture2D grabCursor;    // For dragging ingredients
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

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

    public void SetNormalCursor() => ChangeCursor(normalCursor);
    public void SetPointerCursor() => ChangeCursor(pointerCursor);
    public void SetGrabCursor() => ChangeCursor(grabCursor);

    private void ChangeCursor(Texture2D cursorTexture)
    {
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }
}