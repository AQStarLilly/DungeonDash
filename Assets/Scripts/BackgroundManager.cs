using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance;

    [Header("Backgrounds")]
    public GameObject gameplayBackground;
    public GameObject mainMenuBackground;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowGameplayBackground()
    {
        if (gameplayBackground) gameplayBackground.SetActive(true);
        if (mainMenuBackground) mainMenuBackground.SetActive(false);
    }

    public void ShowMainMenuBackground()
    {
        if (mainMenuBackground) mainMenuBackground.SetActive(true);
        if (gameplayBackground) gameplayBackground.SetActive(false);
    }
}
