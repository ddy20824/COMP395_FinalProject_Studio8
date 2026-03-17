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

        if (item.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        item.transform.SetParent(storageRoot);

        OnItemStored(item);

        ToggleHighlight(false);
        return true;
    }

    protected abstract void OnItemStored(GameObject item);

    public List<GameObject> GetStoredItems() => storedItems;
}
