using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class IngredientController : MonoBehaviour
{
    [Header("Ingredient Data")]
    [SerializeField] private string ingredientName;
    [SerializeField] private IngredientType type;
    [SerializeField] private int capacity; // For bulk items, represents quantity; for others, it's 1
    [SerializeField] private float maxLifeTime = 20f;      // Total lifespan in seconds
    [SerializeField] private float decayRateInFridge = 0.2f; // Decay rate multiplier when in fridge

    [Header("Life Bar UI")]
    [SerializeField] private GameObject lifeBarRoot;  // Root Canvas/GameObject to show/hide
    [SerializeField] private Image lifeBarFill;       // The fill Image (Filled type)
    [SerializeField] private bool faceCamera = true;  // Keep life bar readable in 3D by billboard behavior

    [Header("Hover Feedback")]
    [SerializeField] private float hoverScaleMultiplier = 1.08f;
    [SerializeField] private Color hoverTintColor = new Color(1f, 1f, 0.9f, 1f);

    [Header("Events")]
    [SerializeField] private IngredientTypeEventChannel onIngredientRotten; // Event raised when ingredient rots
    [SerializeField] private SFXTypeEventChannel onSFXRequest; // Event channel to request sound effects

    private float decaySpeed => 1f / maxLifeTime;
    private float currentLife;
    private bool isInFridge = false;
    private float currentDecay;
    private Material foodMaterial;
    private Vector3 originalScale;
    private Color originalColor = Color.white;
    private bool isHovered;
    private bool isRotted;

    // Public getters for ingredient properties
    public string GetIngredientName() => ingredientName;
    public IngredientType GetIngredientType() => type;
    public int GetCapacity() => capacity;

    void Start()
    {
        currentLife = maxLifeTime;
        foodMaterial = GetComponent<Renderer>().material;
        originalScale = transform.localScale;

        if (foodMaterial != null)
        {
            if (foodMaterial.HasProperty("_BaseColor"))
                originalColor = foodMaterial.GetColor("_BaseColor");
            else if (foodMaterial.HasProperty("_Color"))
                originalColor = foodMaterial.GetColor("_Color");
        }

        currentDecay = 0f;
        foodMaterial.SetFloat("_DecayAmount", currentDecay);

        if (lifeBarRoot != null)
            lifeBarRoot.SetActive(false);
    }

    void Update()
    {
        UpdateHoverState();

        // Update life bar fill amount if hovered
        if (isHovered && lifeBarFill != null)
            lifeBarFill.fillAmount = Mathf.Clamp01(currentLife / maxLifeTime);

        if (isRotted) return; // No need to update decay if already rotted

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
            isRotted = true;
            Debug.Log(ingredientName + " has rotted!");
            onIngredientRotten.Raise(type);
            onSFXRequest.Raise(GameplaySFXType.INGR_ROT);
            // TODO: Somewhere need to subscribe to implement rotten logic (e.g. deduct score)
        }
    }

    void LateUpdate()
    {
        UpdateLifeBarBillboard();
    }

    private void UpdateHoverState()
    {
        var mouse = Mouse.current;
        if (mouse == null || Camera.main == null)
        {
            SetHoverState(false);
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
        bool isNowHovered = Physics.Raycast(ray, out RaycastHit hit) &&
                            (hit.transform == transform || hit.transform.IsChildOf(transform));

        SetHoverState(isNowHovered);
    }

    private void SetHoverState(bool hovered)
    {
        if (isHovered == hovered) return;
        isHovered = hovered;

        if (isHovered)
            onSFXRequest.Raise(GameplaySFXType.INGR_HOVER);

        if (lifeBarRoot != null)
            lifeBarRoot.SetActive(isHovered);

        if (isHovered && lifeBarFill != null)
            lifeBarFill.fillAmount = Mathf.Clamp01(currentLife / maxLifeTime);

        transform.localScale = isHovered ? originalScale * hoverScaleMultiplier : originalScale;

        if (foodMaterial != null)
        {
            Color targetColor = isHovered ? hoverTintColor : originalColor;

            if (foodMaterial.HasProperty("_BaseColor"))
                foodMaterial.SetColor("_BaseColor", targetColor);
            else if (foodMaterial.HasProperty("_Color"))
                foodMaterial.SetColor("_Color", targetColor);
        }
    }

    private void UpdateLifeBarBillboard()
    {
        if (!isHovered || !faceCamera || lifeBarRoot == null || Camera.main == null)
            return;

        Transform barTransform = lifeBarRoot.transform;
        Transform camTransform = Camera.main.transform;

        // Match camera orientation so the world-space UI is always readable.
        barTransform.LookAt(
            barTransform.position + camTransform.rotation * Vector3.forward,
            camTransform.rotation * Vector3.up
        );
    }

    public void SetInFridge(bool inFridge)
    {
        isInFridge = inFridge;
    }
}