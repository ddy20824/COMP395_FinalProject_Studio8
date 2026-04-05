using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelect : BasePanel<LevelSelect>
{
    [SerializeField] private Button btnLevel1;
    [SerializeField] private Button btnLevel2;
    [SerializeField] private Button btnLevel3;
    [SerializeField] private Button btnBackToMain;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    protected override void Awake()
    {
        base.Awake();
        HideMe();
    }

    void Start()
    {
        btnLevel1.onClick.AddListener(() =>
        {
            GameSession.CurrentLevelIndex = Level.Level1;
            SceneManager.LoadScene("GameScene");
        });

        btnLevel2.onClick.AddListener(() =>
        {
            GameSession.CurrentLevelIndex = Level.Level2;
            SceneManager.LoadScene("GameScene");
        });

        btnLevel3.onClick.AddListener(() =>
        {
            GameSession.CurrentLevelIndex = Level.Level3;
            SceneManager.LoadScene("GameScene");
        });

        btnBackToMain.onClick.AddListener(() =>
        {
            HideMe();
            MainMenu.Instance.ShowMe();
        });
    }
}
