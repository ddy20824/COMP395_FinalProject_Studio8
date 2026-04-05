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
    [SerializeField] private TextMeshProUGUI successfulDish;
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
        if (endGameEventChannel != null)
        {
            endGameEventChannel.Subscribe(ShowResultBoard);
        }
        if (GameSession.CurrentLevelIndex == Level.Level3)
        {
            btnNextLevel.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (endGameEventChannel != null)
        {
            endGameEventChannel.Unsubscribe(ShowResultBoard);
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
        SoundManager.Instance.ToggleGameMusic(false);
        onSFXRequest.Raise(GameplaySFXType.LEVEL_COMPLETE);

        scoreManager.ApplyRottenPenaltyAtLevelEnd();

        DisableAllGameInteractions();
        PauseMenu.Instance.IsPause = true;
        blurVolume.SetActive(true);

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
        SetStars(score);
        SetAdvice(score);
        setScoreDetail();
    }

    private void SetStars(int score)
    {
        if (score >= 500)
        {
            stars[2].sprite = starStates[1];
        }
        if (score >= 200)
        {
            stars[1].sprite = starStates[1];
        }
        if (score >= 100)
        {
            stars[0].sprite = starStates[1];
        }
    }

    private void SetAdvice(int score)
    {
        // if (score >= 500)
        // {
        //     freeAdvice.text = "You did a great job!";
        // }
        // else if (score >= 200)
        // {
        //     freeAdvice.text = "Good job! Pay attention to the expiring resources in the box to prevent rotten ingredients.";
        // }
        // else if (score >= 100)
        // {
        //     freeAdvice.text = "Well done! Don't waste too much food mkay!";
        // }
        // else
        // {
        //     freeAdvice.text = "Don't worry! An environmentalist will contact you shortly.";
        // }

        //TODO: add carbonImpact in GameConfig
        FinalScoreData scoreData = scoreManager.getFinalScoreData();

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
        FinalScoreData scoreData = scoreManager.getFinalScoreData();

        // Dish completion details
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (scoreData.dishCompletionData != null && scoreData.dishCompletionData.Count > 0)
        {
            foreach (var data in scoreData.dishCompletionData)
            {
                int unitScore = data.deliveredCount > 0 ? data.scoreGained / data.deliveredCount : 0;
                sb.AppendLine($"{data.deliveredCount} x {unitScore} = {data.scoreGained}");
                // sb.AppendLine($"{data.dishType}: {data.deliveredCount} x {unitScore} = {data.scoreGained}");
            }
        }
        else
        {
            sb.Append("No dishes delivered: 0");
        }

        successfulDish.text = sb.ToString();

        // Dealing with failed dishes and rotten ingredients
        failedDish.text = $"Failed: {scoreData.failedDishCount} x {gameConfig.penaltyScore.failureDish} = -{scoreData.failedDishPenalty}";
        noOrderedDish.text = $"No Order: {scoreData.noOrderDishCount} x {gameConfig.penaltyScore.noOrderDish} = -{scoreData.noOrderDishPenalty}";

        rottenIngr.text = $"{scoreData.rottenIngredientCount} x {gameConfig.penaltyScore.rottenIngredient} = -{scoreData.rottenIngredientPenalty}";
        wastedIngr.text = $"{scoreData.wastedIngredientCount} x {gameConfig.penaltyScore.wastedIngredient} = -{scoreData.wastedIngredientPenalty}";
        uncleanedIngr.text = $"{scoreData.levelEndRottenCount} x {gameConfig.penaltyScore.leftRottenIngredient} = -{scoreData.levelEndRottenPenalty}";

        totalScore.text = $"{scoreData.totalScore}";
    }
}
