using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Scriptable Objects/GameConfig")]
public class GameConfig : ScriptableObject
{
    public List<LevelDataContent> allLevels = new List<LevelDataContent>();
    public List<IngredientMapping> ingredientMappings = new List<IngredientMapping>();
}

[System.Serializable]
public class LevelDataContent
{
    public string levelName;
    public float timeLimit;
    public List<IngredientType> availableIngredients;
    public float orderSpawnRate;
    public bool hasPowerOutageEvent;
}

[System.Serializable]
public class IngredientMapping
{
    public IngredientType type;
    public int penaltyScore; // Score deducted from the total score after ingrediant trashed
    public GameObject prefab;
    public int capacityOfBox;
}
