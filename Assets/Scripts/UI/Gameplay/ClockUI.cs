using TMPro;
using UnityEngine;

public class ClockUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;

    private void OnEnable()
    {
        Clock.Instance.OnTimeChanged += UpdateClock;
    }

    private void OnDisable()
    {
        Clock.Instance.OnTimeChanged -= UpdateClock;
    }

    private void UpdateClock(int minutes, int seconds)
    {
        timeText.text = $"{minutes:00} : {seconds:00}";
    }
}
