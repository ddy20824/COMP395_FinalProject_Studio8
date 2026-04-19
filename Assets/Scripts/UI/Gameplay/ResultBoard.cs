using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// TODO: Add event to display Result Board when the level is complete.

public class ResultBoard : BasePanel<ResultBoard>
{
    [SerializeField] private PauseMenu pauseMenuScript;
    [SerializeField] private Button btnSettings;

    [SerializeField] private Button btnNextLevel;
    [SerializeField] private Button btnBackToMain;
    // [SerializeField] private TextMeshProUGUI successfulDish;
    [SerializeField] private GameObject successfulDish_Ham;
    [SerializeField] private GameObject successfulDish_TomatoPizza;
    [SerializeField] private GameObject successfulDish_HamPizza;
    [SerializeField] private GameObject successfulDish_PumpkinPie;
    [SerializeField] private GameObject successfulDish_NoDish;
    [SerializeField] private GameObject dishesGroup;

    [SerializeField] private TextMeshProUGUI failedDish;
    [SerializeField] private TextMeshProUGUI noOrderedDish;
    [SerializeField] private TextMeshProUGUI rottenIngr;
    [SerializeField] private TextMeshProUGUI wastedIngr;
    [SerializeField] private TextMeshProUGUI uncleanedIngr;
    [SerializeField] private TextMeshProUGUI totalScore;
    [SerializeField] private TextMeshProUGUI freeAdvice;
    [SerializeField] private GameObject blurVolume;
    [SerializeField] private Sprite[] starStates;
    [SerializeField] private Image[] stars;
    [SerializeField] private int testScore = 10;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameConfig gameConfig;

    [Header("Events")]
    [SerializeField] private SFXTypeEventChannel onSFXRequest;
    [SerializeField] private VoidEventChannel endGameEventChannel;
    [SerializeField] private IntTypeEventChannel onOrderTimeoutCountChangedChannel;

    private FinalScoreData scoreData;
    private int currentOrderTimeoutCount;
    private bool isTimeoutFailureGameOver;

    protected override void Awake()
    {
        base.Awake();
        blurVolume.SetActive(false);
        InitStarImages();
        HideMe();
    }

    private void Start()
    {
        btnBackToMain.onClick.AddListener(() =>
        {
            BackToMainMenu();
        });

        btnNextLevel.onClick.AddListener(() =>
        {
            GoNextLevel();
        });
    }

    private void OnEnable()
    {
        currentOrderTimeoutCount = 0;
        isTimeoutFailureGameOver = false;

        if (endGameEventChannel != null)
        {
            endGameEventChannel.Subscribe(ShowResultBoard);
        }
        if (onOrderTimeoutCountChangedChannel != null)
        {
            onOrderTimeoutCountChangedChannel.Subscribe(HandleOrderTimeoutCountChanged);
        }
        if (GameSession.CurrentLevelIndex == Level.Level3)
        {
            btnNextLevel.gameObject.SetActive(false);
        }
        else
        {
            btnNextLevel.gameObject.SetActive(true);
        }
        successfulDish_NoDish.SetActive(false);
        successfulDish_Ham.SetActive(false);
        successfulDish_TomatoPizza.SetActive(false);
        successfulDish_HamPizza.SetActive(false);
        successfulDish_PumpkinPie.SetActive(false);
        dishesGroup.SetActive(true);
    }

    private void OnDisable()
    {
        if (endGameEventChannel != null)
        {
            endGameEventChannel.Unsubscribe(ShowResultBoard);
        }
        if (onOrderTimeoutCountChangedChannel != null)
        {
            onOrderTimeoutCountChangedChannel.Unsubscribe(HandleOrderTimeoutCountChanged);
        }
    }

    private void Update()
    {
        // For testing purposes, press F to show the result board
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            ShowResultBoard();
        }
    }

    private void BackToMainMenu()
    {
        ResetGameState();
        SceneManager.LoadScene("MainMenu");
    }

    private void GoNextLevel()
    {
        ResetGameState();
        HideMe();
        GameSession.CurrentLevelIndex++;
        SceneManager.LoadScene("GameScene");
    }

    private void ShowResultBoard()
    {
        scoreData = scoreManager.getFinalScoreData();
        SoundManager.Instance.ToggleGameMusic(false);
        onSFXRequest.Raise(GameplaySFXType.LEVEL_COMPLETE);

        scoreManager.ApplyRottenPenaltyAtLevelEnd();

        DisableAllGameInteractions();
        PauseMenu.Instance.IsPause = true;
        blurVolume.SetActive(true);

        if (isTimeoutFailureGameOver)
        {
            btnNextLevel.gameObject.SetActive(false);
        }

        SetLevelSummary(scoreManager.getFinalScoreData().totalScore);
        ShowMe();
    }

    private void DisableAllGameInteractions()
    {
        Time.timeScale = 0f;
        List<MonoBehaviour> allInteractionScripts = PauseMenu.FindAllInteractionScripts();

        foreach (MonoBehaviour script in allInteractionScripts)
        {
            script.enabled = false;
        }

        // Disable the pause menu as well
        pauseMenuScript.enabled = false;
        btnSettings.enabled = false;
    }

    private void InitStarImages()
    {
        foreach (Image img in stars)
        {
            img.sprite = starStates[0];
        }
    }

    private void SetLevelSummary(int score)
    {
        InitStarImages();

        if (isTimeoutFailureGameOver)
        {
            SetTimeoutFailureAdvice();
        }
        else
        {
            SetStars(score);
            SetAdvice(score);
        }

        setScoreDetail();
    }

    private void SetStars(int score)
    {
        Level level = GameSession.CurrentLevelIndex;
        int star1 = 100, star2 = 200, star3 = 500;

        if (level == Level.Level2) { star1 = 500; star2 = 900; star3 = 1200; }
        else if (level == Level.Level3) { star1 = 800; star2 = 1500; star3 = 2200; }

        if (score >= star1)
        {
            stars[2].sprite = starStates[1];
        }
        if (score >= star2)
        {
            stars[1].sprite = starStates[1];
        }
        if (score >= star3 && scoreData.totalCarbonDistance == 0f)
        {
            stars[0].sprite = starStates[1];
        }
    }

    private void SetAdvice(int score)
    {

        if (scoreData.totalCarbonDistance > 0f)
        {
            freeAdvice.text = $"Your food waste created enough CO2 to drive a car for <color=red>{scoreData.totalCarbonDistance:F1} km</color>. " +
                              "Think about the planet, Chef!";
        }
        else
        {
            freeAdvice.text = "Zero Waste! You saved the planet today. Even the polar bears are cheering for you!";
        }
    }

    private void SetTimeoutFailureAdvice()
    {
        freeAdvice.text = "<color=red><b>Kitchen Closed!</b></color>\nManage your time as carefully as your ingredients.";
    }

    private void HandleOrderTimeoutCountChanged(int timeoutCount)
    {
        currentOrderTimeoutCount = timeoutCount;
        isTimeoutFailureGameOver = currentOrderTimeoutCount >= 5;
    }

    private void ResetGameState()
    {
        Time.timeScale = 1.0f;
        SoundManager.Instance.ToggleGameMusic(true);
        blurVolume.SetActive(false);
        pauseMenuScript.enabled = true;
        btnSettings.enabled = true;
        PauseMenu.Instance.IsPause = false;
    }

    private void setScoreDetail()
    {

        // Dish completion details
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (scoreData.dishCompletionData != null && scoreData.dishCompletionData.Count > 0)
        {
            foreach (var data in scoreData.dishCompletionData)
            {
                int unitScore = data.deliveredCount > 0 ? data.scoreGained / data.deliveredCount : 0;
                string dishesText = $"x{data.deliveredCount} = {data.scoreGained}";
                switch (data.dishType)
                {
                    case DishType.FriedHam:
                        successfulDish_Ham.SetActive(true);
                        successfulDish_Ham.GetComponentInChildren<TextMeshProUGUI>().text = dishesText;
                        break;
                    case DishType.TomatoPizza:
                        successfulDish_TomatoPizza.SetActive(true);
                        successfulDish_TomatoPizza.GetComponentInChildren<TextMeshProUGUI>().text = dishesText;
                        break;
                    case DishType.HamPizza:
                        successfulDish_HamPizza.SetActive(true);
                        successfulDish_HamPizza.GetComponentInChildren<TextMeshProUGUI>().text = dishesText;
                        break;
                    case DishType.PumpkinPie:
                        successfulDish_PumpkinPie.SetActive(true);
                        successfulDish_PumpkinPie.GetComponentInChildren<TextMeshProUGUI>().text = dishesText;
                        break;
                }
                // int unitScore = data.deliveredCount > 0 ? data.scoreGained / data.deliveredCount : 0;
                // sb.AppendLine($"{data.deliveredCount} x {unitScore} = {data.scoreGained}");
                // sb.AppendLine($"{data.dishType}: {data.deliveredCount} x {unitScore} = {data.scoreGained}");
            }
        }
        else
        {
            successfulDish_NoDish.SetActive(true);
            dishesGroup.SetActive(false);
        }

        // successfulDish.text = sb.ToString();

        // Dealing with failed dishes and rotten ingredients
        failedDish.text = $"{scoreData.failedDishCount} x {gameConfig.penaltyScore.failureDish} = {scoreData.failedDishPenalty}";
        noOrderedDish.text = $"{scoreData.noOrderDishCount} x {gameConfig.penaltyScore.noOrderDish} = {scoreData.noOrderDishPenalty}";

        rottenIngr.text = $"{scoreData.rottenIngredientCount} x {gameConfig.penaltyScore.rottenIngredient} = {scoreData.rottenIngredientPenalty}";
        wastedIngr.text = $"{scoreData.wastedIngredientCount} x {gameConfig.penaltyScore.wastedIngredient} = {scoreData.wastedIngredientPenalty}";
        uncleanedIngr.text = $"{scoreData.levelEndRottenCount} x {gameConfig.penaltyScore.leftRottenIngredient} = {scoreData.levelEndRottenPenalty}";

        totalScore.text = $"{scoreData.totalScore}";
    }
}
