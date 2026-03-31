using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;

public class PauseMenu : BasePanel<PauseMenu>
{
    [SerializeField] private Toggle musicOn;
    [SerializeField] private Button btnRestart;
    [SerializeField] private Button btnBackToMain;
    [SerializeField] private Button btnResume;
    [SerializeField] private Button btnSettings;
    //[SerializeField] private PhysicsRaycaster cameraRaycaster;

    private bool isPause = false;

    public bool IsPause { get { return isPause; } set { isPause = value; } } // Temporary fix for CookController Icon.

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
            ControlGamePause();
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

        List<MonoBehaviour> allRaycastingScripts = FindAllInteractionScripts();

        foreach (MonoBehaviour script in allRaycastingScripts)
        {
            script.enabled = !isPause;
        }

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

    /* Public action members */
    public static List<MonoBehaviour> FindAllInteractionScripts()
    {
        List<MonoBehaviour> scripts = new List<MonoBehaviour>();

        DragController[] allDraggables = Object.FindObjectsByType<DragController>(FindObjectsSortMode.None);
        IngredientController[] allIngredients = Object.FindObjectsByType<IngredientController>(FindObjectsSortMode.None);
        BaseStorage[] storages = Object.FindObjectsByType<BaseStorage>(FindObjectsSortMode.None);
        BoxInteraction[] boxInteractions = Object.FindObjectsByType<BoxInteraction>(FindObjectsSortMode.None);
        OrderManager[] orderManagers = Object.FindObjectsByType<OrderManager>(FindObjectsSortMode.None);
        //CookController[] cookControllers = Object.FindObjectsByType<CookController>(FindObjectsSortMode.None);
        CookedDish[] cookedDishes = Object.FindObjectsByType<CookedDish>(FindObjectsSortMode.None);

        scripts.AddRange(allDraggables);
        scripts.AddRange(allIngredients);
        scripts.AddRange(storages);
        scripts.AddRange(boxInteractions);
        scripts.AddRange(orderManagers);
        //scripts.AddRange(cookControllers);
        scripts.AddRange(cookedDishes);

        return scripts;
    }
}
