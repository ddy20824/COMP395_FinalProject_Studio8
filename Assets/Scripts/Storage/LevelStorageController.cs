using System.Collections.Generic;
using UnityEngine;

public class LevelStorageController : MonoBehaviour
{
    [Header("Config")]
    public GameConfig gameConfig;
    public List<GameObject> generalStorageList = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LevelDataContent levelData = gameConfig.allLevels.Find(ld => ld.levelName == GameSession.CurrentLevelIndex);
        for (int i = 0; i < generalStorageList.Count; i++)
        {
            if (i < levelData.availableIngredients.Count)
            {
                generalStorageList[i].SetActive(true);
            }
            else
            {
                generalStorageList[i].SetActive(false);
            }
        }
    }
}
