using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : BasePanel<MainMenu>
{
    [SerializeField] private Button btnStart;
    [SerializeField] private Button btnSettings;
    [SerializeField] private Button btnQuit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btnStart.onClick.AddListener(() =>
        {
            // TODO: SFX
            SceneManager.LoadScene("GameScene");
        });

        btnSettings.onClick.AddListener(() =>
        {
            HideMe();
            Settings.Instance.ShowMe();
        });

        btnQuit.onClick.AddListener(() =>
        {
            if (Application.isEditor)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
            else
            {
                Application.Quit();
            }
        });
    }
}
