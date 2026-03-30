using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    private bool isBGM_On = true;

    [Header("SFX Resources")]
    [SerializeField] private AudioClip[] uiClips;
    [SerializeField] private AudioClip[] gameplayClips;
    private AudioSource bgmSource;
    [SerializeField] private float bgmVolume = 0.5f;
    private AudioSource uiSource;
    [SerializeField] private float uiVolume = 1.0f;
    private AudioSource gameplayOneShotSource;
    [SerializeField] private float gameplayOneShotVolume = 0.7f;
    private AudioSource gameplayLoopingSource;
    [SerializeField] private float gameplayLoopingVolume = 0.7f;

    [Header("Events")]
    [SerializeField] private SFXTypeEventChannel sfxEventChannel;

    public static SoundManager Instance { get => instance; private set => instance = value; }

    public bool IsBGM_On { get => isBGM_On; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = GetComponent<AudioSource>();
        uiSource = gameObject.AddComponent<AudioSource>();
        gameplayOneShotSource = gameObject.AddComponent<AudioSource>();
        gameplayLoopingSource = gameObject.AddComponent<AudioSource>();
    }

    /* Game event subscription */
    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleBGM_OnSceneLoad;
        sfxEventChannel.Subscribe(TriggerGameplaySound);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleBGM_OnSceneLoad;
        sfxEventChannel.Unsubscribe(TriggerGameplaySound);
    }

    /* Private action members */
    // === UI SFX Section ===
    private void PlayUISound(UISFXType type)
    {
        if (Instance == null) return;

        switch (type)
        {
            case UISFXType.BGM1 or UISFXType.BGM2:
                Instance.bgmSource.clip = Instance.uiClips[(int)type];
                Instance.bgmSource.loop = true;
                Instance.bgmSource.volume = isBGM_On ? bgmVolume : 0.0f;
                Instance.bgmSource.Play();
                break;
            default:
                AudioClip clip = Instance.uiClips[(int)type];
                Instance.uiSource.PlayOneShot(clip, uiVolume);
                break;
        }
    }

    private void StopBGM()
    {
        if (Instance.bgmSource.isPlaying && instance.bgmSource.loop)
        {
            Instance.bgmSource.Stop();
            Instance.bgmSource.loop = false;
            Instance.bgmSource.clip = null;
        }
    }

    private void ToggleBGM(bool isOn)
    {
        isBGM_On = isOn;
        Instance.bgmSource.volume = isBGM_On ? bgmVolume : 0.0f;
    }

    private void PlayBGM()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "MainMenu")
        {
            PlayUISound(UISFXType.BGM1);
        }
        if (sceneName == "GameScene")
        {
            PlayUISound(UISFXType.BGM2);
        }
    }

    private void HandleBGM_OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        StopBGM();
        PlayBGM();
    }

    // === Gameplay SFX Section ===
    private void PlayOneShotGameplaySound(GameplaySFXType type)
    {
        AudioClip clip = Instance.gameplayClips[(int)type];
        Instance.gameplayOneShotSource.PlayOneShot(clip, gameplayOneShotVolume);
    }

    private void PlayLoopingGameplaySound(GameplaySFXType type)
    {
        AudioClip clip = Instance.gameplayClips[(int)type];

        if (Instance.gameplayLoopingSource.clip == clip && Instance.gameplayLoopingSource.isPlaying)
        {
            return;
        }

        Instance.gameplayLoopingSource.clip = clip;
        Instance.gameplayLoopingSource.loop = true;
        Instance.gameplayLoopingSource.volume = gameplayOneShotVolume;
        Instance.gameplayLoopingSource.Play();
    }

    private void TriggerGameplaySound(GameplaySFXType type)
    {
        switch (type)
        {
            case GameplaySFXType.IS_COOKING:
                PlayLoopingGameplaySound(type);
                break;
            case GameplaySFXType.COOKING_END:
                PlayOneShotGameplaySound(type);
                StopGameplayLoopingSound(GameplaySFXType.IS_COOKING);
                break;
            default:
                PlayOneShotGameplaySound(type);
                break;
        }
        //AudioClip clip = Instance.gameplayClips[(int)type];
        //Instance.gameplaySource.PlayOneShot(clip, gameplayVolume);
    }

    private void StopGameplayLoopingSound(GameplaySFXType type)
    {
        AudioClip current = Instance.gameplayLoopingSource.clip;
        AudioClip clipToStop = Instance.gameplayClips[(int)type];

        if (current == clipToStop)
        {
            Instance.gameplayLoopingSource.Stop();
            Instance.gameplayLoopingSource.clip = null;
            Instance.gameplayLoopingSource.loop = false;
        }
    }

    /* Public action members */
    public void PlayBtnClickSound()
    {
        PlayUISound(UISFXType.BTN_CLICK);
    }

    public void ToggleGameMusic(bool isOn)
    {
        ToggleBGM(isOn);
    }
}
