using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OrderManager : MonoBehaviour
{
    [Header("Config")]
    public GameConfig gameConfig;
    public List<GameObject> orderBasePrefabs; // selectable order base prefabs for different visual styles
    public List<Transform> spawnPoints;

    [Header("Events")]
    [SerializeField] private SFXTypeEventChannel onSFXRequest;
    [SerializeField] private VoidEventChannel endGameEventChannel;
    [SerializeField] private IntTypeEventChannel onOrderTimeoutCountChangedChannel;

    [Header("Camera Focus")]
    [SerializeField] private CameraFocusEventChannel cameraFocusChannel;
    [SerializeField] private Transform cameraSlot;

    [Header("Current Status")]
    private GameObject[] activeOrders;
    private DishType[] activeDishTypes;
    private bool[] reservedOrders;
    private float[] orderExpireTimes;
    private float[] orderCreatedTimes;
    private float[] orderTimeLimits;
    private OrderUIController[] orderUIControllers;

    private List<DishType> availableDishes = new List<DishType>();
    private Level currentLevelIndex;
    private int initialOrderCount;
    private int maxOrderCount;
    private float orderSpawnRate;
    private float orderSpawnTimer;
    private int currentOrderCount;
    private int timeoutFailedCount;
    private bool isGameOverTriggered;
    private bool isFocus = false;
    private const int MaxTimeoutFailuresForGameOver = 5;

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
        activeDishTypes = new DishType[maxOrderCount];
        reservedOrders = new bool[maxOrderCount];
        orderExpireTimes = new float[maxOrderCount];
        orderCreatedTimes = new float[maxOrderCount];
        orderTimeLimits = new float[maxOrderCount];
        orderUIControllers = new OrderUIController[maxOrderCount];
        currentOrderCount = 0;
        timeoutFailedCount = 0;
        isGameOverTriggered = false;
        RaiseOrderTimeoutCountChanged();

        // Spawn initial orders at the start of the level
        for (int i = 0; i < initialOrderCount; i++)
        {
            CreateNewOrder(availableDishes[Random.Range(0, availableDishes.Count)]);
        }
    }

    void Update()
    {
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsPause)
        {
            return;
        }

        if (isGameOverTriggered)
        {
            return;
        }

        UpdateOrderTimers();

        // Handle mouse input for order interactions (if needed)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseInput(Mouse.current.position.ReadValue());
        }

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
        // 1. Find the first empty slot in activeOrders
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
        activeDishTypes[spawnIndex] = type;
        orderCreatedTimes[spawnIndex] = Time.time;
        orderTimeLimits[spawnIndex] = Mathf.Max(0.1f, dishMapping.orderTimeLimit);
        orderExpireTimes[spawnIndex] = Time.time + orderTimeLimits[spawnIndex];
        currentOrderCount++;
        Debug.Log($"New order created at position {spawnIndex} for dish {type}. Current order count: {currentOrderCount}");

        // 4. initialize the order's UI with the dish and ingredient info
        if (newOrder.TryGetComponent(out OrderUIController controller))
        {
            orderUIControllers[spawnIndex] = controller;
            controller.SetupOrder(dishMapping, requiredMappings);
            controller.UpdateTimerVisual(1f);
        }
        else
        {
            orderUIControllers[spawnIndex] = null;
        }
    }

    // TODO: CompleteOrder should also trigger some score calculation and UI update in the future
    public void CompleteOrder(int index)
    {
        if (index >= 0 && index < activeOrders.Length && activeOrders[index] != null)
        {
            orderUIControllers[index] = null;
            orderExpireTimes[index] = 0f;
            orderCreatedTimes[index] = 0f;
            orderTimeLimits[index] = 0f;
            Destroy(activeOrders[index]);
            activeOrders[index] = null; // Clear the record, the slot becomes empty again
            activeDishTypes[index] = default; // Clear the dish type
            reservedOrders[index] = false;
            currentOrderCount--;
            onSFXRequest.Raise(GameplaySFXType.ORDER_SUBMIT); // Play order completion sound effect
            Debug.Log($"Position {index} order has been completed and removed. Current order count: {currentOrderCount}");
        }
    }

    public bool TryReserveOrderByDishType(DishType type, out int orderIndex, out Transform orderTransform)
    {
        orderIndex = -1;
        orderTransform = null;

        float bestRemainingTime = float.MaxValue;
        float bestCreatedTime = float.MaxValue;

        for (int i = 0; i < activeOrders.Length; i++)
        {
            if (activeOrders[i] != null && !reservedOrders[i] && activeDishTypes[i] == type)
            {
                float remainingTime = orderExpireTimes[i] - Time.time;
                if (remainingTime <= 0f)
                {
                    continue;
                }

                float createdTime = orderCreatedTimes[i];
                bool isBetterChoice = remainingTime < bestRemainingTime ||
                                      (Mathf.Approximately(remainingTime, bestRemainingTime) && createdTime < bestCreatedTime);

                if (isBetterChoice)
                {
                    bestRemainingTime = remainingTime;
                    bestCreatedTime = createdTime;
                    orderIndex = i;
                }
            }
        }

        if (orderIndex >= 0)
        {
            reservedOrders[orderIndex] = true;
            orderTransform = activeOrders[orderIndex].transform;
            return true;
        }

        return false;
    }

    public void ReleaseOrderReservation(int index)
    {
        if (index >= 0 && index < reservedOrders.Length)
        {
            reservedOrders[index] = false;
        }
    }

    public bool TryCompleteReservedOrder(int index)
    {
        if (index >= 0 && index < activeOrders.Length && activeOrders[index] != null && reservedOrders[index])
        {
            CompleteOrder(index);
            return true;
        }

        return false;
    }

    // Check if the current ingredients in the pan match any active order
    public bool CheckMatchAndReserve(List<IngredientMapping> currentIngredients, out DishTypeMapping matchedDish, out int targetOrderIndex)
    {
        matchedDish = null;
        targetOrderIndex = -1;

        List<IngredientType> panIngredients = new List<IngredientType>();
        foreach (var map in currentIngredients)
        {
            panIngredients.Add(map.type);
        }

        // Iterate through active orders and check for a match
        for (int i = 0; i < activeOrders.Length; i++)
        {
            if (activeOrders[i] != null)
            {
                DishType neededType = activeDishTypes[i];
                DishTypeMapping dishMapping = gameConfig.dishTypeMappings.Find(d => d.type == neededType);

                if (dishMapping != null)
                {
                    // Check if the ingredients match (ignoring order)
                    if (AreIngredientsEqual(panIngredients, dishMapping.requiredIngredients))
                    {
                        matchedDish = dishMapping;
                        targetOrderIndex = i;
                        return true;
                    }
                }
            }
        }

        // No match found
        return false;
    }

    public bool TryFindLevelDishByIngredients(List<IngredientMapping> currentIngredients, out DishTypeMapping matchedDish)
    {
        matchedDish = null;

        List<IngredientType> panIngredients = new List<IngredientType>();
        foreach (var map in currentIngredients)
        {
            panIngredients.Add(map.type);
        }

        for (int i = 0; i < availableDishes.Count; i++)
        {
            DishType dishType = availableDishes[i];
            DishTypeMapping dishMapping = gameConfig.dishTypeMappings.Find(d => d.type == dishType);
            if (dishMapping != null && AreIngredientsEqual(panIngredients, dishMapping.requiredIngredients))
            {
                matchedDish = dishMapping;
                return true;
            }
        }

        return false;
    }

    public Transform GetOrderTransform(int index)
    {
        if (index >= 0 && index < activeOrders.Length && activeOrders[index] != null)
        {
            return activeOrders[index].transform;
        }
        return null;
    }

    // Helper function to compare two ingredient lists regardless of order
    private bool AreIngredientsEqual(List<IngredientType> a, List<IngredientType> b)
    {
        if (a.Count != b.Count) return false;

        List<IngredientType> bCopy = new List<IngredientType>(b);
        foreach (var item in a)
        {
            if (bCopy.Contains(item))
            {
                bCopy.Remove(item); // Remove one instance of the item from the copy
            }
            else
            {
                return false; // If an item in a is not found in bCopy, the lists are not equal
            }
        }
        return true;
    }

    // Find the index of the active order that matches the given DishType. Returns -1 if not found.
    public int FindActiveOrderIndexByDishType(DishType type)
    {
        for (int i = 0; i < activeOrders.Length; i++)
        {
            if (activeOrders[i] != null && activeDishTypes[i] == type)
            {
                return i;
            }
        }
        return -1;
    }

    private void HandleMouseInput(Vector2 screenPoint)
    {
        if (Camera.main == null)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                ToggleFocus();
            }
        }
    }

    public void ToggleFocus()
    {
        isFocus = !isFocus;

        CameraFocusData data = new CameraFocusData
        {
            targetTransform = cameraSlot,
            isFocusing = isFocus
        };

        cameraFocusChannel.Raise(data);
    }

    private void UpdateOrderTimers()
    {
        for (int i = 0; i < activeOrders.Length; i++)
        {
            if (isGameOverTriggered)
            {
                return;
            }

            if (activeOrders[i] == null)
            {
                continue;
            }

            float timeLimit = orderTimeLimits[i];
            if (timeLimit <= 0f)
            {
                continue;
            }

            float remainingTime = orderExpireTimes[i] - Time.time;
            float normalizedRemaining = Mathf.Clamp01(remainingTime / timeLimit);

            if (orderUIControllers[i] != null)
            {
                orderUIControllers[i].UpdateTimerVisual(normalizedRemaining);
            }

            if (remainingTime > 0f)
            {
                continue;
            }

            TimeoutOrder(i);
        }
    }

    private void TimeoutOrder(int index)
    {
        if (index < 0 || index >= activeOrders.Length || activeOrders[index] == null)
        {
            return;
        }

        if (reservedOrders[index])
        {
            reservedOrders[index] = false;
        }

        orderUIControllers[index] = null;
        orderExpireTimes[index] = 0f;
        orderCreatedTimes[index] = 0f;
        orderTimeLimits[index] = 0f;

        Destroy(activeOrders[index]);
        activeOrders[index] = null;
        activeDishTypes[index] = default;
        currentOrderCount = Mathf.Max(0, currentOrderCount - 1);

        onSFXRequest?.Raise(GameplaySFXType.ORDER_TIMEOUT);

        timeoutFailedCount++;
        RaiseOrderTimeoutCountChanged();
        Debug.Log($"Order at position {index} timed out. Timeout failures: {timeoutFailedCount}");

        if (!isGameOverTriggered && timeoutFailedCount >= MaxTimeoutFailuresForGameOver)
        {
            isGameOverTriggered = true;
            endGameEventChannel?.Raise();
        }
    }

    private void RaiseOrderTimeoutCountChanged()
    {
        onOrderTimeoutCountChangedChannel?.Raise(timeoutFailedCount);
    }
}