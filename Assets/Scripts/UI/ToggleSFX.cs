using UnityEngine;
using UnityEngine.UI;

public class ToggleSFX : MonoBehaviour
{
    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        SoundManager.Instance.PlayBtnClickSound();
        SoundManager.Instance.ToggleGameMusic(isOn);
    }
}
