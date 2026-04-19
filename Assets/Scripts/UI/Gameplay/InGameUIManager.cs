using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject inGameUI;

    [Header("Missed Order UI")]
    [SerializeField] private Image[] missedOrderSlots;
    [SerializeField] private Sprite normalOrderSprite;
    [SerializeField] private Sprite missedOrderSprite;
    [SerializeField] private bool consumeFromRight = true;

    [Header("Events")]
    [SerializeField] private IntTypeEventChannel onScoreChangedChannel;
    [SerializeField] private VoidEventChannel endGameEventChannel;
    [SerializeField] private IntTypeEventChannel onOrderTimeoutCountChangedChannel;

    private bool isClockSubscribed;

    private void OnEnable()
    {
        SubscribeClock();

        if (onScoreChangedChannel != null)
        {
            onScoreChangedChannel.Subscribe(UpdateScore);
        }
        if (endGameEventChannel != null)
        {
            endGameEventChannel.Subscribe(HideInGameUI);
        }
        if (onOrderTimeoutCountChangedChannel != null)
        {
            onOrderTimeoutCountChangedChannel.Subscribe(UpdateMissedOrderIcons);
        }
    }

    private void Start()
    {
        // Set defaults immediately so HUD is populated before first events arrive.
        UpdateClock(Clock.Instance.GetMinutes(), Clock.Instance.GetSeconds());
        UpdateLevel();
        UpdateScore(0);
        UpdateMissedOrderIcons(0);
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

        if (endGameEventChannel != null)
        {
            endGameEventChannel.Unsubscribe(HideInGameUI);
        }

        if (onOrderTimeoutCountChangedChannel != null)
        {
            onOrderTimeoutCountChangedChannel.Unsubscribe(UpdateMissedOrderIcons);
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
            timerText.text = $"Time: {minutes:00} : {seconds:00}";
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
            scoreText.text = $"Score: {score.ToString()}";
        }
    }

    private void HideInGameUI()
    {
        if (inGameUI != null)
        {
            inGameUI.SetActive(false);
        }
    }

    private void UpdateMissedOrderIcons(int timeoutCount)
    {
        if (missedOrderSlots == null || missedOrderSlots.Length == 0)
        {
            return;
        }

        int clampedTimeoutCount = Mathf.Clamp(timeoutCount, 0, missedOrderSlots.Length);

        for (int i = 0; i < missedOrderSlots.Length; i++)
        {
            Image slot = missedOrderSlots[i];
            if (slot == null)
            {
                continue;
            }

            bool shouldShowMissed = consumeFromRight
                ? i >= missedOrderSlots.Length - clampedTimeoutCount
                : i < clampedTimeoutCount;

            if (shouldShowMissed)
            {
                if (missedOrderSprite != null)
                {
                    slot.sprite = missedOrderSprite;
                }
            }
            else
            {
                if (normalOrderSprite != null)
                {
                    slot.sprite = normalOrderSprite;
                }
            }
        }
    }
}
