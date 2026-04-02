using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Scriptable Objects/GameConfig")]
public class GameConfig : ScriptableObject
{
    public int capacityOfBox;
    public GameObject failureDishPrefab;
    public float failedDishCookTime = 5f;
    public PenaltyScore penaltyScore = new PenaltyScore();
    public List<LevelDataContent> allLevels = new List<LevelDataContent>();
    public List<IngredientMapping> ingredientMappings = new List<IngredientMapping>();
    public List<DishTypeMapping> dishTypeMappings = new List<DishTypeMapping>();
}

[System.Serializable]
public class LevelDataContent
{
    public Level levelName;
    public int timeLimit;
    public List<IngredientType> availableIngredients;

    public List<DishType> availableDishes;
    public float orderSpawnRate;
    public int maxOrderNumber;
    public int initailOrderNumber;
    public bool hasPowerOutageEvent;

    public int capacityOfFridge;
    public int panNumber;
}

[System.Serializable]
public class IngredientMapping
{
    public IngredientType type;
    public GameObject prefab;
    public Sprite sprite;
    public float iconScale = 0.1f;
}

[System.Serializable]
public class DishTypeMapping
{
    public DishType type;
    public GameObject prefab;
    public Sprite sprite;
    public float iconScale = 0.1f;
    public int scoreValue;
    public List<IngredientType> requiredIngredients;
    public int cookingTime; // in seconds
}

[System.Serializable]
public class PenaltyScore
{
    public int rottenIngredient = 10;
    public int wastedIngredient = 30;
    public int leftRottenIngredient = 30;
    public int failureDish = 50;
    public int noOrderDish = 30;
}