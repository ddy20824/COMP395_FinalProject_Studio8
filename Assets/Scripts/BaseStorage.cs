using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStorage : MonoBehaviour
{
    [Header("Storage Settings")]
    public int maxCapacity = 8;
    public Transform storageRoot;


    protected List<GameObject> storedItems = new List<GameObject>();
    public bool IsFull => storedItems.Count >= maxCapacity;

    // [SerializeField] protected MeshRenderer outlineRenderer;

    public virtual void ToggleHighlight(bool show)
    {
        Debug.Log($"{name} ToggleHighlight: {show}");

        // TODO: Implement visual feedback for highlighting, e.g. by enabling an outline shader, changing material color, or showing a UI indicator. The actual implementation will depend on the art assets and design of the game.
        // if (outlineRenderer == null) return;
        // outlineRenderer.material.SetInt("_IsOutlineEnabled", show ? 1 : 0);
    }


    public virtual bool StoreItem(GameObject item)
    {
        if (IsFull)
        {
            Debug.Log($"{name} is full！");
            return false;
        }

        storedItems.Add(item);

        item.transform.SetParent(storageRoot);

        OnItemStored(item);

        ToggleHighlight(false);
        return true;
    }

    protected abstract void OnItemStored(GameObject item);

    public virtual void TakeOutItem(GameObject item)
    {
        if (storedItems.Contains(item))
        {
            storedItems.Remove(item);

            item.transform.SetParent(null);

            OnItemRemoved(item);
        }
    }

    protected abstract void OnItemRemoved(GameObject item);

    public List<GameObject> GetStoredItems() => storedItems;
}
