using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{ 
    public static SoundManager Instance;

    [Header("Audio Source")]
    public AudioSource musicSource;

    [Header("Music Tracks")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip bossMusic;

    [Header("Sound Effects")]
    public AudioSource sfxSource; // separate source for short effects

    [Header("UI Sounds")]
    public AudioClip buttonClick;
    public AudioClip upgradePurchase;
    public AudioClip announcerSound;

    [Header("Player Sounds")]
    public AudioClip playerAttack;
    public AudioClip playerDeath;

    [Header("Enemy Sounds")]
    public AudioClip enemyAttack;
    public AudioClip enemyDeath;

    [Header("Boss Sounds")]
    public AudioClip bossHowl;       // when boss is about to spawn (wave 30)
    public AudioClip bossAttack;
    public AudioClip bossDeath;

    [Header("Settings")]
    public float fadeDuration = 0.7f;
    private AudioClip currentTrack;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    private const string VolumeKey = "MusicVolume";

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
        if (nextTrack != null && musicSource.clip != nextTrack)
        {
            StartCoroutine(FadeAndSwitch(nextTrack));
        }
    }

    public void PlayBossMusic()
    {
        if (bossMusic != null && bossMusic != currentTrack)
        {
            StartCoroutine(FadeAndSwitch(bossMusic));
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume * masterVolume);
    }

    private IEnumerator FadeAndSwitch(AudioClip nextTrack)
    {
        if (musicSource == null) yield break;

        float startVolume = musicSource.volume;

        // Fade OUT using unscaledDeltaTime
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            float progress = fadeDuration <= 0f ? 1f : t / fadeDuration;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = nextTrack;
        musicSource.Play();

        // Re-apply the current saved volume before fading in
        float targetVolume = PlayerPrefs.GetFloat("MusicVolume", masterVolume);
        masterVolume = targetVolume; // sync the internal variable

        // Fade IN using unscaledDeltaTime
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            float progress = fadeDuration <= 0f ? 1f : t / fadeDuration;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, progress);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    public void SetVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        if (musicSource != null)
            musicSource.volume = masterVolume;
        PlayerPrefs.SetFloat(VolumeKey, masterVolume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return masterVolume;
    }

    private void OnEnable()
    {
        if (PlayerPrefs.HasKey(VolumeKey))
            masterVolume = PlayerPrefs.GetFloat(VolumeKey);
        if (musicSource != null)
            musicSource.volume = masterVolume;
    }
}


