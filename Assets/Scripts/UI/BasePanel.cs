using UnityEngine;

public class BasePanel<T> : MonoBehaviour where T : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private static T instance;
    public static T Instance => instance;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (instance != null && instance != this as T)
        {
            Destroy(gameObject);
            return;
        }

        instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this as T)
        {
            instance = null;
        }
    }

    public virtual void ShowMe()
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public virtual void HideMe()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
