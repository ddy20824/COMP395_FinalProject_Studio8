using UnityEngine;

public class WasteBin : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private IngredientTypeEventChannel _onIngredientTrashed;
    [SerializeField] private SFXTypeEventChannel onSFXRequest;

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
        onSFXRequest.Raise(GameplaySFXType.TRASH_INTO);

        //TODO: Just a log for now, UI can listen to this event for updateing the score
        Debug.Log($"Trashed: {ingredientController.GetIngredientType()}");

        // Destroy the ingredient
        Destroy(other.gameObject);
    }
}
