using System.Collections.Generic;
using UnityEngine;

public class LevelPanSpawner : MonoBehaviour
{
    [Header("Config")]
    public GameConfig gameConfig;

    [Header("Pan Setup")]
    public List<GameObject> spawnPans = new List<GameObject>();



    private void Start()
    {
        LevelDataContent levelData = gameConfig.allLevels.Find(ld => ld.levelName == GameSession.CurrentLevelIndex);
        if (levelData.panNumber <= 0)
        {
            return;
        }

        for (int i = 0; i < levelData.panNumber && i < spawnPans.Count; i++)
        {
            spawnPans[i].SetActive(true);
        }
    }
}
