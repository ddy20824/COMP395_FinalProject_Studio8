using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenu : BasePanel<PauseMenu>
{
    [SerializeField] private Toggle musicOn;
    [SerializeField] private Button btnRestart;
    [SerializeField] private Button btnBackToMain;
    [SerializeField] private Button btnResume;
    [SerializeField] private Button btnSettings;

    private bool isPause = false;

    protected override void Awake()
    {
        base.Awake();
        HideMe();
        musicOn.isOn = SoundManager.Instance.IsBGM_On;
    }

    void Start()
    {
        btnRestart.onClick.AddListener(() =>
        {
            Time.timeScale = 1.0f;
            RestartLevel();
        });

        btnBackToMain.onClick.AddListener(() =>
        {
            Time.timeScale = 1.0f;
            BackToMainMenu();
        });

        btnResume.onClick.AddListener(() =>
        {
            isPause = false;
            Time.timeScale = 1.0f;
            HideMe();
        });

        btnSettings.onClick.AddListener(() =>
        {
            ControlGamePause();
        });
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SoundManager.Instance.PlayBtnClickSound();
            ControlGamePause();
        }
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void ControlGamePause()
    {
        isPause = !isPause;
        if (isPause)
        {
            Time.timeScale = 0.0f;
            //CursorManager.Instance.SetNormalCursor();
            ShowMe();
        }
        else
        {
            Time.timeScale = 1.0f;
            HideMe();
        }
    }
}
