using UnityEngine;

public class DecayController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxLifeTime = 20f;      // Total lifespan in seconds
    [SerializeField] private float decayRateInFridge = 0.2f; // Decay rate multiplier when in fridge

    private float decaySpeed => 1f / maxLifeTime;
    private float currentLife;
    private bool isInFridge = false;
    private float currentDecay;
    private Material foodMaterial;

    void Start()
    {
        currentLife = maxLifeTime;
        foodMaterial = GetComponent<Renderer>().material;

        currentDecay = 0f;
        foodMaterial.SetFloat("_DecayAmount", currentDecay);
    }

    void Update()
    {
        // Apply slower decay rate when inside the fridge
        float decayRate = isInFridge ? decayRateInFridge : 1.0f;
        currentLife -= Time.deltaTime * decayRate;
        currentDecay = 1f - (currentLife / maxLifeTime);

        // Update shader color based on freshness
        if (currentDecay < 1f)
        {
            currentDecay += Time.deltaTime * decaySpeed * decayRate;
            foodMaterial.SetFloat("_DecayAmount", currentDecay);
        }

        if (currentLife <= 0f)
        {
            Debug.Log(gameObject.name + " has rotted!");
            // TODO: trigger death logic (e.g. spawn trash, deduct score)
        }
    }

    // Trigger: detect whether the ingredient is inside the fridge area
    // (requires a Trigger Collider on the fridge object tagged "Fridge")
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fridge")) isInFridge = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Fridge")) isInFridge = false;
    }
}