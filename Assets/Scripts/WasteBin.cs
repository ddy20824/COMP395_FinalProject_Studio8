using UnityEngine;

public class WasteBin : MonoBehaviour
{
       [SerializeField] private IngredientTrashedEventChannel _onIngredientTrashed;
       
       // When collide with the trash can
       private void OnTriggerEnter(Collider other)
       {
              // Debug.Log("Trigger hit by: " + other.name); 
              TrashIngrediant(other);
       }

       private void TrashIngrediant(Collider other)
       {
              // get the IngredientController component
              IngredientController ingredientController = other.GetComponent<IngredientController>();
              
              // If there is no ingrediant
              if (ingredientController == null)
              {
                     // Debug.Log("Ingrediant Not found");
                     return;
              }
              
              // Fire the event
              _onIngredientTrashed.Raise(ingredientController);
              
              //TODO: Just a log for now, UI can listen to this event for updateing the score
              Debug.Log($"Trashed: {ingredientController.GetIngredientName()} | Penalty: {ingredientController.GetPenaltyScore()}");
        
              // Destroy the ingredient
              Destroy(other.gameObject);
       }
}
