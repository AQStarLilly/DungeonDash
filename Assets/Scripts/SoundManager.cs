using UnityEngine;

public class SoundManager : MonoBehaviour
{
    //SFX - 10(Minimum)
    //Player
        //Death (plays and goes into the results screen)
        //Attack
        //Take Damage

    //Enemy
        //Death
        //Attack
        //Take Damage

    //Boss
        //Some sort of line for the boss to say or have the boss howl/yell as you come up to them?
        //Death
        //Attack
        //Take Damage

    //DUNGEON DASH - play when you first load into the game - and when you hit dash again? (just yell dungeon dash with intense feel)
    //Upgrade Button SFX?
    //Game Won SFX - plays when you beat the final boss
    
    public static SoundManager Instance;

    [Header("Audio Source")]
    public AudioSource musicSource;

    [Header("Music Tracks")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip bossMusic;

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
        if (nextTrack == currentTrack) return;

        StartCoroutine(FadeAndSwitch(nextTrack));
    }

    public void PlayBossMusic()
    {
        if (bossMusic != null && bossMusic != currentTrack)
        {
            StartCoroutine(FadeAndSwitch(bossMusic));
        }
    }
    private System.Collections.IEnumerator FadeAndSwitch(AudioClip nextTrack)
    {
        if (musicSource.isPlaying)
        {
            // Fade out
            float startVolume = musicSource.volume;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
                yield return null;
            }
            musicSource.Stop();
        }

        // Switch tracks
        musicSource.clip = nextTrack;
        currentTrack = nextTrack;
        musicSource.Play();

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = 1f;
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
