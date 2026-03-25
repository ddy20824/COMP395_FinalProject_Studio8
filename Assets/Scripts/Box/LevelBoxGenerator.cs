using System.Collections.Generic;
using UnityEngine;

public class LevelBoxGenerator : MonoBehaviour
{
    [Header("Config")]
    public GameConfig gameConfig;
    [Header("Box Setup")]
    public GameObject boxPrefab;
    public GameObject glowEffectPrefab;
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Detection Settings")]
    public float checkRadius = 0.5f; // Detection radius
    public LayerMask itemLayer;      // Recommended to assign a dedicated Layer to the box

    [Header("Timer Settings")]
    public float spawnInterval = 2.0f;
    private float timer;

    private void Start()
    {
        timer = spawnInterval;

        foreach (var spawnPoint in spawnPoints)
        {
            spawnPoint.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        GenerateBoxes();
    }

    public void GenerateBoxes()
    {
        if (gameConfig == null || boxPrefab == null)
        {
            return;
        }

        LevelDataContent levelData = gameConfig.allLevels.Find(ld => ld.levelName == GameSession.CurrentLevelIndex);
        if (levelData.availableIngredients == null || levelData.availableIngredients.Count == 0)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            for (int i = 0; i < levelData.availableIngredients.Count; i++)
            {
                IngredientType type = levelData.availableIngredients[i];
                IngredientMapping mapping = gameConfig.ingredientMappings.Find(m => m.type == type);

                if (mapping == null || mapping.prefab == null)
                {
                    continue;
                }

                bool isOccupied = Physics.CheckSphere(spawnPoints[i].position, checkRadius, itemLayer);

                if (!isOccupied)
                {
                    SpawnItem(i, mapping);
                }
            }
            timer = 0f;
        }
    }

    private void SpawnItem(int index, IngredientMapping mapping)
    {
        spawnPoints[index].gameObject.SetActive(true);
        var rotation = spawnPoints[index].rotation * Quaternion.Euler(0f, 90f, 0f);

        var box = Instantiate(boxPrefab, spawnPoints[index].position, rotation);
        BoxInteraction interaction = box.GetComponent<BoxInteraction>();
        if (interaction != null)
        {
            interaction.ConfigureBoxContents(mapping.prefab, gameConfig.capacityOfBox);
        }

        var glowEffect = Instantiate(glowEffectPrefab, spawnPoints[index].position, Quaternion.identity);
        Destroy(glowEffect, 0.8f);
    }

    // Draw the detection range in the Scene view for easy adjustment
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, checkRadius);
            }
        }
    }
}
