using System.Collections;
using UnityEngine;

public class GeneralStorageController : BaseStorage
{
    void Awake()
    {
        maxCapacity = gameConfig.capacityOfBox;
    }

    protected override void HandleMouseInput(Vector2 screenPoint)
    {
        if (Camera.main == null)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            GameObject hitObj = hit.transform.gameObject;

            if (storedItems.Contains(hitObj))
            {
                TakeOutItem(hitObj);
                return;
            }
        }
    }

    protected override void OnItemStored(GameObject item)
    {
        if (storageRoot != null)
        {
            StartCoroutine(FlyToStoragePoint(item));
        }
    }

    protected override void OnItemRemoved(GameObject item)
    {
        if (item.TryGetComponent(out DragController drag))
        {
            drag.StartDrag();
        }
    }


    private IEnumerator FlyToStoragePoint(GameObject item)
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

            item.transform.position = Vector3.Lerp(startPos, storageRoot.position, t);
            item.transform.rotation = Quaternion.Lerp(startRot, storageRoot.rotation, t);
            yield return null;
        }

        if (item.TryGetComponent(out Rigidbody rbFinal))
        {
            rbFinal.isKinematic = false;
        }
    }

}
