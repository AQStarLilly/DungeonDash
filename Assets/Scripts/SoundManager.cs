using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{ 
    public static SoundManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;

    [Header("Music Tracks")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip bossMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource sfxSource; 

    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip upgradePurchase;
    [SerializeField] private AudioClip announcerSound;

    [Header("Player Sounds")]
    [SerializeField] private AudioClip playerAttack;
    [SerializeField] private AudioClip playerDeath;

    [Header("Enemy Sounds")]
    [SerializeField] private AudioClip enemyAttack;
    [SerializeField] private AudioClip enemyDeath;

    [Header("Boss Sounds")]
    [SerializeField] private AudioClip bossHowl;     
    [SerializeField] private AudioClip bossAttack;
    [SerializeField] private AudioClip bossDeath;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.7f;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;

    private const string VolumeKey = "MusicVolume";

    private AudioClip currentTrack;
    private Coroutine fadeCoroutine;

    // --- Unity Methods ---
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        if (PlayerPrefs.HasKey(VolumeKey))
            masterVolume = PlayerPrefs.GetFloat(VolumeKey);

        if (musicSource != null)
            musicSource.volume = masterVolume;
    }

    /// <summary>
    /// Switches Music based on the game state
    /// </summary>
    public void PlayMusicForState(GameManager.GameState state)
    {
        AudioClip nextTrack = null;

        switch (state)
        {
            // Menu music
            case GameManager.GameState.MainMenu:
            case GameManager.GameState.MainMenuOptions:
            case GameManager.GameState.Instructions:
            case GameManager.GameState.Upgrades:
                nextTrack = menuMusic;
                break;


            // Gameplay music
            case GameManager.GameState.Gameplay:
            case GameManager.GameState.Pause:
            case GameManager.GameState.PauseMenuOptions:
            case GameManager.GameState.Results:
                nextTrack = gameplayMusic;
                break;

            default:
                return;
        }
        if (nextTrack != null && nextTrack != currentTrack)
            StartFade(nextTrack);
    }

    public void PlayBossMusic()
    {
        if (bossMusic != null && bossMusic != currentTrack)
        {
            StartFade(bossMusic);
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundManager] Attempted to play a null SFX clip.");
            return;
        }

        if (sfxSource == null)
        {
            Debug.LogWarning("[SoundManager] No SFX AudioSource assigned.");
            return;
        }

        sfxSource.PlayOneShot(clip, volume * masterVolume);
    }

    public void SetVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);

        if (musicSource != null)
            musicSource.volume = masterVolume;

        PlayerPrefs.SetFloat(VolumeKey, masterVolume);
        PlayerPrefs.Save();
    }

    public float GetVolume() => masterVolume;

    public void PlayButtonClick() => PlaySFX(buttonClick);
    public void PlayUpgradePurchase() => PlaySFX(upgradePurchase);
    public void PlayAnnouncer() => PlaySFX(announcerSound);

    public void PlayPlayerAttack() => PlaySFX(playerAttack);
    public void PlayPlayerDeath() => PlaySFX(playerDeath);

    public void PlayEnemyAttack() => PlaySFX(enemyAttack);
    public void PlayEnemyDeath() => PlaySFX(enemyDeath);

    public void PlayBossHowl() => PlaySFX(bossHowl);
    public void PlayBossAttack() => PlaySFX(bossAttack);
    public void PlayBossDeath() => PlaySFX(bossDeath);

    // --- Internal Utilities ---
    private void StartFade(AudioClip nextTrack)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeAndSwitch(nextTrack));
    }

    private IEnumerator FadeAndSwitch(AudioClip nextTrack)
    {
        if (musicSource == null)
        {
            Debug.LogWarning("[SoundManager] No music source found.");
            yield break;
        }

        currentTrack = nextTrack;

        float startVolume = musicSource.volume;

        // Fade OUT
        for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            float progress = fadeDuration <= 0f ? 1f : t / fadeDuration;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = nextTrack;
        musicSource.Play();

        // Refresh saved volume
        float targetVolume = PlayerPrefs.GetFloat(VolumeKey, masterVolume);
        masterVolume = targetVolume;

        // Fade IN
        for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            float progress = fadeDuration <= 0f ? 1f : t / fadeDuration;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, progress);
            yield return null;
        }

        musicSource.volume = targetVolume;
        fadeCoroutine = null;
    }
}


