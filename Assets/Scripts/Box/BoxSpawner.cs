using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    [Header("Item Settings")]
    public GameObject boxPrefab;    // Drag in the corresponding box (vegetable or meat)
    public GameObject glowEffectPrefab;

    [Header("Detection Settings")]
    public float checkRadius = 0.5f; // Detection radius
    public LayerMask itemLayer;      // Recommended to assign a dedicated Layer to the box

    [Header("Timer Settings")]
    public float spawnInterval = 2.0f;
    private float timer;

    void Start()
    {
        timer = spawnInterval;
    }

    void Update()
    {
        // Use physics detection to check if anything is within the sphere radius
        bool isOccupied = Physics.CheckSphere(transform.position, checkRadius, itemLayer);

        if (!isOccupied)
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                SpawnItem();
                timer = 0f;
            }
        }
        else
        {
            // If the position is occupied, reset the timer (or pause, depending on your needs)
            timer = 0f;
        }
    }

    void SpawnItem()
    {
        Instantiate(boxPrefab, transform.position, transform.rotation);
        var glowEffect = Instantiate(glowEffectPrefab, transform.position, Quaternion.identity);
        timer = -spawnInterval; // Prevent immediate re-spawn before physics updates
        Destroy(glowEffect, 0.8f);
    }

    // Draw the detection range in the Scene view for easy adjustment
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}
