using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] uiClips;
    [SerializeField] private AudioClip[] gameplayClips;

    private static SoundManager instance;

    private AudioSource bgmSource;
    [SerializeField] private float bgmVolume = 0.5f;
    private AudioSource uiSource;
    [SerializeField] private float uiVolume = 1.0f;
    private AudioSource gameplaySource;
    [SerializeField] private float gameplayVolume = 0.7f;

    [SerializeField] private SFXTypeEventChannel sfxEventChannel;

    public static SoundManager Instance { get => instance; set => instance = value; }

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
        gameplaySource = gameObject.AddComponent<AudioSource>();
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
                Instance.bgmSource.volume = bgmVolume;
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
    private void TriggerGameplaySound(GameplaySFXType type)
    {
        AudioClip clip = Instance.gameplayClips[(int)type];
        Instance.gameplaySource.PlayOneShot(clip, gameplayVolume);
    }

    /* Public action members */
    public void PlayBtnClickSound()
    {
        PlayUISound(UISFXType.BTN_CLICK);
    }
}
