using System.Collections;
using UnityEngine;

public class WasteBin : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private IngredientTypeEventChannel _onIngredientTrashed;
    [SerializeField] private SFXTypeEventChannel onSFXRequest;
    [SerializeField] private VoidEventChannel trashBinBounceChannel;

    [Header("Animation Settings")]
    [SerializeField] private float bounceScaleMultiplier = 1.2f;
    [SerializeField] private float bounceSpeed = 5f;

    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    // Subscribe to the bounce event when enabled, and unsubscribe when disabled
    private void OnEnable() => trashBinBounceChannel.Subscribe(PlayBounce);
    private void OnDisable() => trashBinBounceChannel.Unsubscribe(PlayBounce);

    // When collide with the trash can
    private void OnTriggerEnter(Collider other) => TrashIngrediant(other);


    private void TrashIngrediant(Collider other)
    {
        // get the IngredientController component
        IngredientController ingredientController = other.GetComponent<IngredientController>();

        // If there is no ingrediant
        if (ingredientController == null) return;

        // Fire the event
        _onIngredientTrashed.Raise(ingredientController.GetIngredientType());
        CursorManager.Instance.SetNormalCursor();

        //TODO: Just a log for now, UI can listen to this event for updateing the score
        Debug.Log($"Trashed: {ingredientController.GetIngredientType()}");

        // Play bounce animation and sound effect
        PlayBounce();

        // Destroy the ingredient
        Destroy(other.gameObject);
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
