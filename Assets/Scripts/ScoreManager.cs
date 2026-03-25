using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private IntTypeEventChannel onScoreChangedChannel;
    [SerializeField] private IngredientTypeEventChannel onIngredientRottenChannel;
    [SerializeField] private DishTypeEventChannel onDishDeliveredChannel;
    [SerializeField] private IngredientTypeEventChannel onFreshIngredientTrashedChannel;

    [Header("Config")]
    public GameConfig gameConfig;

    // Parameters for score calculation
    private Level currentLevelIndex;
    private int currentScore;

    private void OnEnable()
    {
        onIngredientRottenChannel.Subscribe(HandleIngredientRotten);
        onDishDeliveredChannel.Subscribe(HandleDishDelivered);
        onFreshIngredientTrashedChannel.Subscribe(HandleFreshIngredientTrashed);
    }
    private void OnDisable()
    {
        onIngredientRottenChannel.Unsubscribe(HandleIngredientRotten);
        onDishDeliveredChannel.Unsubscribe(HandleDishDelivered);
        onFreshIngredientTrashedChannel.Unsubscribe(HandleFreshIngredientTrashed);
    }

    void Start()
    {
        // Initialize score to 0 at the start of the game
        currentScore = 0;
        onScoreChangedChannel.Raise(currentScore);

        // Initialize level-specific settings
        currentLevelIndex = GameSession.CurrentLevelIndex;
        LevelDataContent levelData = gameConfig.allLevels.Find(l => l.levelName == currentLevelIndex);
    }

    private void HandleIngredientRotten(IngredientType ingredientType)
    {
        Debug.Log($"Ingredient Rotten: {ingredientType} - Score Penalty Applied: {gameConfig.penaltyScore.rottenIngredient}");
        HandleScoreUpdate(-gameConfig.penaltyScore.rottenIngredient);
    }

    private void HandleDishDelivered(DishType dishType)
    {
        if (dishType == DishType.FailedDish)
        {
            Debug.Log($"Failed Dish Delivered - Score Penalty Applied: {-gameConfig.penaltyScore.failureDish}");
            HandleScoreUpdate(-gameConfig.penaltyScore.failureDish);
        }
        else
        {
            DishTypeMapping dishMapping = gameConfig.dishTypeMappings.Find(d => d.type == dishType); if (dishMapping != null)
            {
                Debug.Log($"Dish Delivered: {dishType} - Score Awarded: {dishMapping.scoreValue}");
                HandleScoreUpdate(dishMapping.scoreValue);
            }
        }
    }

    private void HandleFreshIngredientTrashed(IngredientType ingredientType)
    {
        Debug.Log($"Trashed: {ingredientType} - Score Penalty Applied: {gameConfig.penaltyScore.wastedIngredient}");
        HandleScoreUpdate(-gameConfig.penaltyScore.wastedIngredient);
    }

    private void HandleScoreUpdate(int scoreChange)
    {
        currentScore += scoreChange;
        onScoreChangedChannel.Raise(currentScore);
        Debug.Log($"Score Updated: {currentScore}");
    }

    //TODO: Penalty for leaving rotten ingredients at the end of the level
    public void ApplyRottenPenaltyAtLevelEnd()
    {
        IngredientController[] allIngredients = FindObjectsByType<IngredientController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        int rottenCount = 0;
        for (int i = 0; i < allIngredients.Length; i++)
        {
            IngredientController ingredient = allIngredients[i];
            if (ingredient != null && ingredient.IsRotted())
                rottenCount++;
        }

        if (rottenCount <= 0)
            return;

        int totalPenalty = rottenCount * gameConfig.penaltyScore.rottenIngredient;
        Debug.Log($"Level End Rotten Check: {rottenCount} rotten ingredients - Total Penalty: {totalPenalty}");
        HandleScoreUpdate(-totalPenalty);
    }
}
