using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// TODO: Add event to display Result Board when the level is complete.

public class ResultBoard : BasePanel<ResultBoard>
{
    [SerializeField] private Button btnNextLevel;
    [SerializeField] private Button btnBackToMain;
    [SerializeField] private TextMeshProUGUI successfulDish;
    [SerializeField] private TextMeshProUGUI failedDish;
    [SerializeField] private TextMeshProUGUI rottenIngr;
    [SerializeField] private TextMeshProUGUI totalScore;
    [SerializeField] private GameObject blurVolume;

    [Header("Events")]
    [SerializeField] private SFXTypeEventChannel onSFXRequest;
    protected override void Awake()
    {
        base.Awake();
        blurVolume.SetActive(false);
        HideMe();
    }

    private void Start()
    {
        TestUI(); // Only for testing

        btnBackToMain.onClick.AddListener(() =>
        {
            BackToMainMenu();
        });

        btnNextLevel.onClick.AddListener(() => {
            GoNextLevel();
        });
    }

    private void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            SoundManager.Instance.ToggleGameMusic(false);
            onSFXRequest.Raise(GameplaySFXType.LEVEL_COMPLETE);
            DisableAllGameInteractions();
            blurVolume.SetActive(true);
            ShowMe();
        }
    }

    private void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void GoNextLevel()
    {
        // TODO: Implement Next Level
        Time.timeScale = 1.0f;
        SoundManager.Instance.ToggleGameMusic(true);
        blurVolume.SetActive(false);
        HideMe();
    }

    //private List<MonoBehaviour> FindAllInteractionScripts()
    //{
    //    List<MonoBehaviour> scripts = new List<MonoBehaviour>();

    //    DragController[] allDraggables = Object.FindObjectsByType<DragController>(FindObjectsSortMode.None);
    //    IngredientController[] allIngredients = Object.FindObjectsByType<IngredientController>(FindObjectsSortMode.None);
    //    BaseStorage[] storages = Object.FindObjectsByType<BaseStorage>(FindObjectsSortMode.None);
    //    BoxInteraction[] boxInteractions = Object.FindObjectsByType<BoxInteraction>(FindObjectsSortMode.None);
    //    OrderManager[] orderManagers = Object.FindObjectsByType<OrderManager>(FindObjectsSortMode.None);
    //    CookController[] cookControllers = Object.FindObjectsByType<CookController>(FindObjectsSortMode.None);
    //    CookedDish[] cookedDishes = Object.FindObjectsByType<CookedDish>(FindObjectsSortMode.None);

    //    scripts.AddRange(allDraggables);
    //    scripts.AddRange(allIngredients);
    //    scripts.AddRange(storages);
    //    scripts.AddRange(boxInteractions);
    //    scripts.AddRange(orderManagers);
    //    scripts.AddRange(cookControllers);
    //    scripts.AddRange(cookedDishes);

    //    return scripts;
    //}

    private void DisableAllGameInteractions()
    {
        Time.timeScale = 0f;
        List<MonoBehaviour> allInteractionScripts = PauseMenu.FindAllInteractionScripts();

        foreach (MonoBehaviour script in allInteractionScripts)
        {
            script.enabled = false;
        }
    }

    private void TestUI() // Only for testing
    {
        successfulDish.text = $"3 x 100 = 300";
        failedDish.text = $"1 x 50 = -50";
        rottenIngr.text = $"10 x 10 = -100";
        totalScore.text = "150";
    }
}
