using System;
using UnityEngine;

public class Clock : MonoBehaviour
{
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private VoidEventChannel endGameEventChannel;
    private static Clock instance;

    private float timer = 0f;
    private int minutes;
    private int seconds;
    private const float realSecond = 1f;

    public static Clock Instance { get => instance; private set => instance = value; }
    public event Action<int, int> OnTimeChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        if (gameConfig != null)
        {
            seconds = gameConfig.allLevels.Find(x => x.levelName == GameSession.CurrentLevelIndex).timeLimit;
            if (seconds >= 60)
            {
                minutes = seconds / 60;
                seconds = seconds % 60;
            }
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= realSecond)
        {
            timer -= realSecond;
            AddSeconds();
        }
    }

    private void AddSeconds()
    {
        seconds--;

        if (minutes <= 0 && seconds < 0)
        {
            endGameEventChannel?.Raise();
            return;
        }

        if (seconds < 0)
        {
            seconds = 59;
            minutes--;
        }

        OnTimeChanged?.Invoke(minutes, seconds);
    }

    public int GetSeconds()
    {
        return seconds;
    }

    public int GetMinutes()
    {
        return minutes;
    }
}
