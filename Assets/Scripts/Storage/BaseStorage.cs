using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BaseStorage : MonoBehaviour
{
    [Header("Storage Settings")]
    [SerializeField] protected Transform storageRoot;
    [SerializeField] protected Transform rejectPoint;
    [SerializeField] protected GameConfig gameConfig;


    protected int maxCapacity;
    protected List<GameObject> storedItems = new List<GameObject>();
    protected bool IsFull => storedItems.Count >= maxCapacity;

    protected Collider selfCollider;
    protected Outline outline;

    protected virtual void Start()
    {
        selfCollider = GetComponent<Collider>();
        outline = GetComponentInChildren<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseInput(Mouse.current.position.ReadValue());
        }
    }

    public virtual void ToggleHighlight(bool show)
    {
        Debug.Log($"{name} ToggleHighlight: {show}");

        if (outline != null)
        {
            outline.enabled = show;
            outline.OutlineWidth = 10f;
        }
    }


    public virtual bool StoreItem(GameObject item)
    {
        ToggleHighlight(false);
        if (IsFull)
        {
            Debug.Log($"{name} is full！");
            OnStoreFailed(item);
            return false;
        }

        storedItems.Add(item);

        item.transform.SetParent(storageRoot);

        OnItemStored(item);

        return true;
    }

    public virtual void TakeOutItem(GameObject item)
    {
        storedItems.Remove(item);

        item.transform.SetParent(null);

        OnItemRemoved(item);
    }

    protected virtual void OnStoreFailed(GameObject item)
    {
        if (rejectPoint != null)
        {
            StartCoroutine(FlyToRejectPoint(item));
        }
        else
        {
            if (item.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = false;
                rb.AddForce((transform.forward + Vector3.up) * 2f, ForceMode.Impulse);
            }
        }
    }

    private IEnumerator FlyToRejectPoint(GameObject item)
    {
        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 startPos = item.transform.position;
        Quaternion startRot = item.transform.rotation;

        if (item.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 3f);

            item.transform.position = Vector3.Lerp(startPos, rejectPoint.position, t);
            item.transform.rotation = Quaternion.Lerp(startRot, rejectPoint.rotation, t);
            yield return null;
        }

        if (item.TryGetComponent(out Rigidbody rbFinal))
        {
            rbFinal.isKinematic = false;
        }
    }

    protected abstract void OnItemStored(GameObject item);

    protected abstract void OnItemRemoved(GameObject item);
    protected abstract void HandleMouseInput(Vector2 screenPoint);

    public List<GameObject> GetStoredItems() => storedItems;
}
