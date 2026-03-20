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

    }

    protected override void OnItemRemoved(GameObject item)
    {
        if (item.TryGetComponent(out DragController drag))
        {
            drag.StartDrag();
        }
    }

}
