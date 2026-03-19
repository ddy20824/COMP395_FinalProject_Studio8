using System;
using UnityEngine;

public class Clock : MonoBehaviour
{
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
        seconds++;

        if (seconds >= 60)
        {
            seconds = 0;
            minutes++;
        }

        OnTimeChanged?.Invoke(minutes, seconds);
    }
}
