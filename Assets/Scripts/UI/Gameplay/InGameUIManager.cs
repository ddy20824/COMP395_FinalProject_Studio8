using TMPro;
using UnityEngine;

public class InGameUIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Events")]
    [SerializeField] private IntTypeEventChannel onScoreChangedChannel;

    private bool isClockSubscribed;

    private void OnEnable()
    {
        SubscribeClock();

        if (onScoreChangedChannel != null)
        {
            onScoreChangedChannel.Subscribe(UpdateScore);
        }
    }

    private void Start()
    {
        // Set defaults immediately so HUD is populated before first events arrive.
        UpdateClock(Clock.Instance.GetMinutes(), Clock.Instance.GetSeconds());
        UpdateLevel();
        UpdateScore(0);
        SubscribeClock();
    }

    private void Update()
    {
        if (!isClockSubscribed)
        {
            SubscribeClock();
        }
    }

    private void OnDisable()
    {
        if (isClockSubscribed && Clock.Instance != null)
        {
            Clock.Instance.OnTimeChanged -= UpdateClock;
            isClockSubscribed = false;
        }

        if (onScoreChangedChannel != null)
        {
            onScoreChangedChannel.Unsubscribe(UpdateScore);
        }
    }

    private void SubscribeClock()
    {
        if (isClockSubscribed || Clock.Instance == null)
        {
            return;
        }

        Clock.Instance.OnTimeChanged += UpdateClock;
        isClockSubscribed = true;
    }

    private void UpdateClock(int minutes, int seconds)
    {
        if (timerText != null)
        {
            timerText.text = $"{minutes:00} : {seconds:00}";
        }
    }

    private void UpdateLevel()
    {
        if (levelText == null)
        {
            return;
        }

        int levelNumber = (int)GameSession.CurrentLevelIndex + 1;
        levelText.text = $"Level {levelNumber}";
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }
}
