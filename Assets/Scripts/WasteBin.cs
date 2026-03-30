using System.Collections;
using UnityEngine;

public class WasteBin : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private IngredientTypeEventChannel _onFreshIngredientTrashed;
    [SerializeField] private SFXTypeEventChannel onSFXRequest;
    [SerializeField] private VoidEventChannel trashBinBounceChannel;

    [Header("Animation Settings")]
    [SerializeField] private float bounceScaleMultiplier = 1.2f;
    [SerializeField] private float bounceSpeed = 5f;

    private Vector3 initialScale;
    private DragController currentDraggedItem;
    private Collider binCollider;

    private void Awake()
    {
        initialScale = transform.localScale;
        binCollider = GetComponent<Collider>();
    }

    // Subscribe to the bounce event when enabled, and unsubscribe when disabled
    private void OnEnable() => trashBinBounceChannel.Subscribe(PlayBounce);
    private void OnDisable() => trashBinBounceChannel.Unsubscribe(PlayBounce);

    // Track when ingredients collide with bin
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's a dragged ingredient
        DragController dragController = other.GetComponent<DragController>();
        if (dragController != null && dragController.IsDragging)
        {
            currentDraggedItem = dragController;
            return;
        }

        // Non-dragged items trash immediately
        TrashIngrediant(other);
    }

    private void OnTriggerExit(Collider other)
    {
        DragController dragController = other.GetComponent<DragController>();
        if (dragController != null && dragController == currentDraggedItem)
        {
            currentDraggedItem = null;
        }
    }

    private void Update()
    {
        // Check if dragged item was released while still in bin
        if (currentDraggedItem != null && !currentDraggedItem.IsDragging)
        {
            // Only trash if item is still overlapping bin bounds
            Collider itemCollider = currentDraggedItem.GetComponent<Collider>();
            if (itemCollider != null && binCollider != null && binCollider.bounds.Intersects(itemCollider.bounds))
            {
                TrashIngrediantObject(currentDraggedItem.gameObject);
            }
            currentDraggedItem = null;
        }
    }
    private void TrashIngrediant(Collider other)
    {
        if (other == null) return;
        TrashIngrediantObject(other.gameObject);
    }

    private void TrashIngrediantObject(GameObject ingredientObject)
    {
        // get the IngredientController component
        IngredientController ingredientController = ingredientObject.GetComponent<IngredientController>();

        // If there is no ingredient
        if (ingredientController == null) return;

        // Fire the event
        if (!ingredientController.IsRotted())
        {
            _onFreshIngredientTrashed.Raise(ingredientController.GetIngredientType());
            Debug.Log($"Trashed: {ingredientController.GetIngredientType()}");
        }
        CursorManager.Instance.SetNormalCursor();

        // Play bounce animation and sound effect
        PlayBounce();

        // Destroy the ingredient
        Destroy(ingredientObject);
    }

    private void PlayBounce()
    {
        onSFXRequest.Raise(GameplaySFXType.TRASH_INTO);
        StopAllCoroutines();
        StartCoroutine(BounceRoutine());
    }

    private IEnumerator BounceRoutine()
    {
        Vector3 targetBounceScale = initialScale * bounceScaleMultiplier;

        // First quickly scale up to the target bounce scale
        transform.localScale = targetBounceScale;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * bounceSpeed;
            // Then smoothly scale back down to the initial scale
            transform.localScale = Vector3.Lerp(targetBounceScale, initialScale, t);
            yield return null;
        }

        // Ensure the scale is reset to the initial scale at the end
        transform.localScale = initialScale;
    }
}
