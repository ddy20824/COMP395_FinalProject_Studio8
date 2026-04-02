using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FinalScoreData
{
    public int totalScore;

    public int totalPositiveScore;
    public int totalNegativeScore;

    public int dishScoreGained;
    public int failedDishPenalty;
    public int noOrderDishPenalty;
    public int rottenIngredientPenalty;
    public int wastedIngredientPenalty;
    public int levelEndRottenPenalty;

    public int deliveredDishCount;
    public int failedDishCount;
    public int noOrderDishCount;
    public int rottenIngredientCount;
    public int wastedIngredientCount;
    public int levelEndRottenCount;
    public float totalCarbonDistance;

    public List<DishCompletionData> dishCompletionData;
}

[System.Serializable]
public class DishCompletionData
{
    public DishType dishType;
    public int deliveredCount;
    public int scoreGained;
}

public class ScoreManager : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private IntTypeEventChannel onScoreChangedChannel;
    [SerializeField] private IngredientTypeEventChannel onIngredientRottenChannel;
    [SerializeField] private DishTypeEventChannel onDishDeliveredChannel;
    [SerializeField] private DishTypeEventChannel onDishWastedChannel;
    [SerializeField] private IngredientTypeEventChannel onFreshIngredientTrashedChannel;

    [Header("Config")]
    public GameConfig gameConfig;

    // Parameters for score calculation
    private Level currentLevelIndex;
    private int currentScore;

    // Cumulative score stats
    private int totalPositiveScore;
    private int totalNegativeScore;
    private int dishScoreGained;
    private int failedDishPenalty;
    private int noOrderDishPenalty;
    private int rottenIngredientPenalty;
    private int wastedIngredientPenalty;
    private int levelEndRottenPenalty;

    private int deliveredDishCount;
    private int failedDishCount;
    private int noOrderDishCount;
    private int rottenIngredientCount;
    private int wastedIngredientCount;
    private int levelEndRottenCount;
    private float totalCarbonDistance;

    private readonly Dictionary<DishType, int> deliveredDishCountByType = new Dictionary<DishType, int>();
    private readonly Dictionary<DishType, int> deliveredDishScoreByType = new Dictionary<DishType, int>();

    private void OnEnable()
    {
        onIngredientRottenChannel.Subscribe(HandleIngredientRotten);
        onDishDeliveredChannel.Subscribe(HandleDishDelivered);
        onDishWastedChannel.Subscribe(HandleDishWaste);
        onFreshIngredientTrashedChannel.Subscribe(HandleFreshIngredientTrashed);
    }
    private void OnDisable()
    {
        onIngredientRottenChannel.Unsubscribe(HandleIngredientRotten);
        onDishDeliveredChannel.Unsubscribe(HandleDishDelivered);
        onDishWastedChannel.Unsubscribe(HandleDishWaste);
        onFreshIngredientTrashedChannel.Unsubscribe(HandleFreshIngredientTrashed);
    }

    void Start()
    {
        // Initialize score to 0 at the start of the game
        currentScore = 0;
        totalPositiveScore = 0;
        totalNegativeScore = 0;
        dishScoreGained = 0;
        failedDishPenalty = 0;
        noOrderDishPenalty = 0;
        rottenIngredientPenalty = 0;
        wastedIngredientPenalty = 0;
        levelEndRottenPenalty = 0;
        deliveredDishCount = 0;
        failedDishCount = 0;
        noOrderDishCount = 0;
        rottenIngredientCount = 0;
        wastedIngredientCount = 0;
        levelEndRottenCount = 0;
        totalCarbonDistance = 0f;

        deliveredDishCountByType.Clear();
        deliveredDishScoreByType.Clear();

        onScoreChangedChannel.Raise(currentScore);

        // Initialize level-specific settings
        currentLevelIndex = GameSession.CurrentLevelIndex;
        LevelDataContent levelData = gameConfig.allLevels.Find(l => l.levelName == currentLevelIndex);
    }

    private void HandleIngredientRotten(IngredientType ingredientType)
    {
        int penalty = gameConfig.penaltyScore.rottenIngredient;
        rottenIngredientCount++;
        rottenIngredientPenalty += penalty;

        Debug.Log($"Ingredient Rotten: {ingredientType} - Score Penalty Applied: {penalty}");
        HandleScoreUpdate(-penalty);
    }

    private void HandleDishDelivered(DishType dishType)
    {
        if (dishType == DishType.FailedDish)
        {
            int penalty = gameConfig.penaltyScore.failureDish;
            failedDishCount++;
            failedDishPenalty += penalty;

            Debug.Log($"Failed Dish Delivered - Score Penalty Applied: {-penalty}");
            HandleScoreUpdate(-penalty);
        }
        else
        {
            DishTypeMapping dishMapping = gameConfig.dishTypeMappings.Find(d => d.type == dishType);
            if (dishMapping != null)
            {
                deliveredDishCount++;
                dishScoreGained += dishMapping.scoreValue;
                UpdatePerDishStats(dishType, dishMapping.scoreValue);

                Debug.Log($"Dish Delivered: {dishType} - Score Awarded: {dishMapping.scoreValue}");
                HandleScoreUpdate(dishMapping.scoreValue);
            }
        }
    }

    // 處理成品料理（含失敗料理）進垃圾桶
    public void HandleDishWaste(DishType type)
    {
        DishTypeMapping mapping = gameConfig.dishTypeMappings.Find(m => m.type == type);
        float dishCarbonDist = 0f;
        int penalty;

        if (type == DishType.FailedDish)
        {
            penalty = gameConfig.penaltyScore.failureDish;
            failedDishCount++;
            failedDishPenalty += penalty;

            dishCarbonDist = 30f;
            Debug.Log($"Failed Dish Wasted - Score Penalty Applied: {penalty}");
        }
        else
        {
            penalty = gameConfig.penaltyScore.noOrderDish;
            noOrderDishCount++;
            noOrderDishPenalty += penalty;

            int ingredientCount = mapping != null ? mapping.requiredIngredients.Count : 0;
            dishCarbonDist = (ingredientCount * 20f) + 10f;

            Debug.Log($"Dish Wasted With No Order: {type} - Score Penalty Applied: {penalty}");
        }

        totalCarbonDistance += dishCarbonDist;
        HandleScoreUpdate(-penalty);
    }

    private void HandleFreshIngredientTrashed(IngredientType ingredientType)
    {
        int penalty = gameConfig.penaltyScore.wastedIngredient;
        wastedIngredientCount++;
        wastedIngredientPenalty += penalty;

        Debug.Log($"Trashed: {ingredientType} - Score Penalty Applied: {penalty}");
        HandleScoreUpdate(-penalty);
    }

    private void HandleScoreUpdate(int scoreChange)
    {
        currentScore += scoreChange;

        if (scoreChange >= 0)
            totalPositiveScore += scoreChange;
        else
            totalNegativeScore += -scoreChange;

        onScoreChangedChannel.Raise(currentScore);
        Debug.Log($"Score Updated: {currentScore}");
    }

    private void UpdatePerDishStats(DishType dishType, int score)
    {
        if (!deliveredDishCountByType.ContainsKey(dishType))
            deliveredDishCountByType[dishType] = 0;

        if (!deliveredDishScoreByType.ContainsKey(dishType))
            deliveredDishScoreByType[dishType] = 0;

        deliveredDishCountByType[dishType]++;
        deliveredDishScoreByType[dishType] += score;
    }

    private List<DishCompletionData> BuildDishCompletionData()
    {
        List<DishCompletionData> result = new List<DishCompletionData>();

        foreach (KeyValuePair<DishType, int> pair in deliveredDishCountByType)
        {
            DishType dishType = pair.Key;
            int deliveredCount = pair.Value;
            int scoreGained = deliveredDishScoreByType.ContainsKey(dishType)
                ? deliveredDishScoreByType[dishType]
                : 0;

            result.Add(new DishCompletionData
            {
                dishType = dishType,
                deliveredCount = deliveredCount,
                scoreGained = scoreGained
            });
        }

        return result;
    }

    //TODO: Penalty for leaving rotten ingredients at the end of the level
    public void ApplyRottenPenaltyAtLevelEnd()
    {
        Debug.Log("Applying Rotten Ingredient Penalty at Level End...");
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

        int totalPenalty = rottenCount * gameConfig.penaltyScore.leftRottenIngredient;
        levelEndRottenCount += rottenCount;
        levelEndRottenPenalty += totalPenalty;

        Debug.Log($"Level End Rotten Check: {rottenCount} rotten ingredients - Total Penalty: {totalPenalty}");
        HandleScoreUpdate(-totalPenalty);
    }

    public FinalScoreData getFinalScoreData()
    {
        return new FinalScoreData
        {
            totalScore = currentScore,
            totalPositiveScore = totalPositiveScore,
            totalNegativeScore = totalNegativeScore,
            dishScoreGained = dishScoreGained,
            failedDishPenalty = failedDishPenalty,
            noOrderDishPenalty = noOrderDishPenalty,
            rottenIngredientPenalty = rottenIngredientPenalty,
            wastedIngredientPenalty = wastedIngredientPenalty,
            levelEndRottenPenalty = levelEndRottenPenalty,
            deliveredDishCount = deliveredDishCount,
            failedDishCount = failedDishCount,
            noOrderDishCount = noOrderDishCount,
            rottenIngredientCount = rottenIngredientCount,
            wastedIngredientCount = wastedIngredientCount,
            levelEndRottenCount = levelEndRottenCount,
            totalCarbonDistance = totalCarbonDistance,
            dishCompletionData = BuildDishCompletionData()
        };
    }
}
