using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [Header("Config")]
    public GameConfig gameConfig;
    public List<GameObject> orderBasePrefabs; // selectable order base prefabs for different visual styles
    public List<Transform> spawnPoints;

    [Header("Current Status")]
    private GameObject[] activeOrders;
    private List<DishType> availableDishes = new List<DishType>();
    private Level currentLevelIndex;
    private int initialOrderCount;
    private int maxOrderCount;
    private float orderSpawnRate;
    private float orderSpawnTimer;
    private int currentOrderCount;

    void Start()
    {
        // Initialize level-specific settings
        currentLevelIndex = GameSession.CurrentLevelIndex;
        LevelDataContent levelData = gameConfig.allLevels.Find(l => l.levelName == currentLevelIndex);
        availableDishes = new List<DishType>(levelData.availableDishes);
        initialOrderCount = levelData.initailOrderNumber;
        maxOrderCount = levelData.maxOrderNumber;
        orderSpawnRate = levelData.orderSpawnRate;

        // start the timer so that the first spawn happens after the defined interval
        orderSpawnTimer = orderSpawnRate;

        // Initialize the activeOrders array based on the maxOrderCount
        activeOrders = new GameObject[maxOrderCount];
        currentOrderCount = 0;

        // Spawn initial orders at the start of the level
        for (int i = 0; i < initialOrderCount; i++)
        {
            CreateNewOrder(availableDishes[Random.Range(0, availableDishes.Count)]);
        }
    }

    void Update()
    {
        // Only try to spawn new orders if we haven't reached the maximum limit
        if (currentOrderCount < maxOrderCount)
        {
            orderSpawnTimer -= Time.deltaTime;
            if (orderSpawnTimer <= 0f)
            {
                CreateNewOrder(availableDishes[Random.Range(0, availableDishes.Count)]);
                orderSpawnTimer = orderSpawnRate; // reset the timer
            }
        }
    }

    public void CreateNewOrder(DishType type)
    {
        // 1. Findq the first empty slot in activeOrders
        int spawnIndex = -1;
        for (int i = 0; i < activeOrders.Length; i++)
        {
            if (activeOrders[i] == null)
            {
                spawnIndex = i;
                break;
            }
        }

        // If no empty slot is found, we cannot create a new order
        if (spawnIndex == -1)
        {
            Debug.Log("Order limit reached, cannot create new order.");
            return;
        }

        // 2. Find the corresponding DishTypeMapping and required IngredientMappings
        DishTypeMapping dishMapping = gameConfig.dishTypeMappings.Find(m => m.type == type);
        if (dishMapping == null) return;

        List<IngredientMapping> requiredMappings = new List<IngredientMapping>();
        foreach (var ingType in dishMapping.requiredIngredients)
        {
            var mapping = gameConfig.ingredientMappings.Find(m => m.type == ingType);
            if (mapping != null) requiredMappings.Add(mapping);
        }

        // 3. Pick a random order base prefab and spawn the new order at the corresponding spawn point
        GameObject randomPrefab = orderBasePrefabs[Random.Range(0, orderBasePrefabs.Count)];
        Transform targetSpot = spawnPoints[spawnIndex];

        GameObject newOrder = Instantiate(randomPrefab, targetSpot.position, targetSpot.rotation);

        activeOrders[spawnIndex] = newOrder;
        currentOrderCount++;
        Debug.Log($"New order created at position {spawnIndex} for dish {type}. Current order count: {currentOrderCount}");

        // 4. initialize the order's UI with the dish and ingredient info
        if (newOrder.TryGetComponent(out OrderUIController controller))
        {
            controller.SetupOrder(dishMapping, requiredMappings);
        }
    }

    // TODO: CompleteOrder should also trigger some score calculation and UI update in the future
    public void CompleteOrder(int index)
    {
        if (index >= 0 && index < activeOrders.Length && activeOrders[index] != null)
        {
            Destroy(activeOrders[index]);
            activeOrders[index] = null; // Clear the record, the slot becomes empty again
            currentOrderCount--;
            Debug.Log($"Position {index} order has been completed and removed. Current order count: {currentOrderCount}");
        }
    }
}