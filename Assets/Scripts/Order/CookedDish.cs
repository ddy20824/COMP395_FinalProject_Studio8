using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CookedDish : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] private float arcHeight = 2.0f;

    [Header("Events")]
    [SerializeField] private VoidEventChannel onTrashBinBounceChannel;
    [SerializeField] private DishTypeEventChannel onDishDeliveredChannel;
    private bool isSuccess;
    private DishType dishType;
    private OrderManager orderManager;
    private CookController sourceStation;
    private Transform trashTargetPoint;
    private bool isFlying = false;

    // This method is called right after instantiating the cooked dish prefab, to set up necessary references and data for its behavior
    public void Setup(bool success, int orderIndex, OrderManager manager, CookController station, Transform trashPoint, DishType type)
    {
        isSuccess = success;
        orderManager = manager;
        sourceStation = station;
        trashTargetPoint = trashPoint;
        dishType = type;
    }

    void Update()
    {
        if (isFlying) return;

        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    HandleClick();
                }
            }
        }
    }

    private void HandleClick()
    {
        if (!isSuccess)
        {
            GoToTrash();
            return;
        }

        int latestOrderIndex = orderManager.FindActiveOrderIndexByDishType(dishType);

        if (latestOrderIndex != -1)
        {
            Transform targetOrderTransform = orderManager.GetOrderTransform(latestOrderIndex);
            if (targetOrderTransform != null)
            {
                // fly to the order position, and complete the order after arrival
                StartCoroutine(FlyAndDestroy(targetOrderTransform, true, latestOrderIndex));
                return;
            }
        }

        // If we can't find a valid order , just fly to the trash as a fallback
        GoToTrash();
    }

    private void GoToTrash()
    {
        if (sourceStation != null)
            sourceStation.ClearFinishedDish(trashTargetPoint);
        else
            TryFlyToTrash(gameObject.transform); // If no stove, fly to the trash point
    }

    public bool TryFlyToTrash(Transform target)
    {
        if (isFlying || target == null) return false;
        StartCoroutine(FlyAndDestroy(target, false, -1));
        onTrashBinBounceChannel.Raise(); // Trigger the bounce effect on the trash bin
        onDishDeliveredChannel.Raise(dishType); // Trigger any dish delivered events (e.g. for UI updates)
        return true;
    }

    private IEnumerator FlyAndDestroy(Transform target, bool completeOrderAfterArrival, int orderIndex)
    {
        isFlying = true;
        if (sourceStation != null)
            sourceStation.ReleaseFinishedDishReference(gameObject);

        Vector3 startPos = transform.position;
        Vector3 endPos = target.position;
        Vector3 startScale = transform.localScale;

        // For fly to trash, we want a nice arc. For fly to order, we want a more direct path for better visual clarity.
        Vector3 controlPoint = startPos + (endPos - startPos) * 0.5f + Vector3.up * arcHeight;

        float flyDuration = 0.5f;
        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;

            if (completeOrderAfterArrival)
            {
                transform.position = Vector3.Lerp(startPos, endPos, t);
            }
            else
            {
                // bezier curve for arc movement: (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
                Vector3 m1 = Vector3.Lerp(startPos, controlPoint, t);
                Vector3 m2 = Vector3.Lerp(controlPoint, endPos, t);
                transform.position = Vector3.Lerp(m1, m2, t);

                // Rotate the dish for a more dynamic effect when flying to trash
                transform.Rotate(Vector3.forward, 720f * Time.deltaTime);
            }

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        if (completeOrderAfterArrival && orderManager != null)
        {
            orderManager.CompleteOrder(orderIndex);
        }

        Destroy(gameObject);
    }
}