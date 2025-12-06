using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class UIManager : MonoBehaviour
{
    [Header("UI Screens")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameInstructionsUI;
    [SerializeField] private GameObject mainMenuOptionsUI;
    [SerializeField] private GameObject pauseMenuOptionsUI;
    [SerializeField] private GameObject upgradesUI;
    [SerializeField] private GameObject resultsUI;
    [SerializeField] private GameObject creditsUI;
    [SerializeField] private GameObject winUI;

    [Header("Main Menu Button")]
    [SerializeField] private Button loadGameButton;
    public Button LoadGameButton => loadGameButton;

    [Header("Background Reference")]
    [SerializeField] private ScrollingBackground scrollingBackground;

    [Header("Popups")]
    [SerializeField] private ConfirmPopup confirmPopup;

    private GameObject[] allScreens;

    private void Awake()
    {
        // Cache screens for cleaner logic later
        allScreens = new[]
        {
            mainMenuUI, gameplayUI, pausePanel, gameInstructionsUI,
            mainMenuOptionsUI, pauseMenuOptionsUI, upgradesUI,
            resultsUI, creditsUI, winUI
        };
    }

    // --- Public Methods ---
    public void UpdateUI(GameState state)
    {
        DisableAllScreens();

        switch (state)
        {
            case GameState.MainMenu:
                Activate(mainMenuUI);
                break;

            case GameState.Gameplay:
                Activate(gameplayUI);
                break;

            case GameState.Pause:
                Activate(gameplayUI);
                Activate(pausePanel);
                break;

            case GameState.Instructions:
                Activate(gameInstructionsUI);
                break;

            case GameState.MainMenuOptions:
                Activate(mainMenuOptionsUI);
                break;

            case GameState.PauseMenuOptions:
                Activate(pauseMenuOptionsUI);
                break;

            case GameState.Upgrades:
                Activate(upgradesUI);
                break;

            case GameState.Results:
                Activate(resultsUI);
                break;

            case GameState.Credits:
                Activate(creditsUI);
                break;

            case GameState.Win:
                Activate(winUI);
                break;
        }
    }

    public void UpdateLoadButtonInteractable(bool hasSave)
    {
        if(loadGameButton != null)
        {
            loadGameButton.interactable = hasSave;
        }
    }

    // --- NAVIGATION FUNCTIONS ---
    public void GoToMainMenu()
    {
        PlayClick();
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }

    public void GoToGameplay(bool fromResume = false, bool fromUpgrades = false)
    {
        if (!fromResume)
        {
            PlayStartGameSFX();
            ResetBackgroundScroll();
        }

        GameManager.Instance.StartGameplay(fromUpgrades);
    }

    public void GoToPause()
    {
        PlayClick();
        GameManager.Instance.ChangeState(GameState.Pause);
    }

    public void GoToInstructions()
    {
        PlayClick();
        GameManager.Instance.ChangeState(GameState.Instructions);
    }

    public void GoToMainMenuOptions()
    {
        PlayClick();
        GameManager.Instance.ChangeState(GameState.MainMenuOptions);
    }

    public void GoToPauseMenuOptions()
    {
        PlayClick();
        GameManager.Instance.ChangeState(GameState.PauseMenuOptions);
    }

    public void GoToUpgrades()
    {
        PlayClick();
        GameManager.Instance.ChangeState(GameState.Upgrades);
    }

    public void GoToResults()
        => GameManager.Instance.ChangeState(GameState.Results);

    public void GoToCredits()
    {
        PlayClick();
        GameManager.Instance.ChangeState(GameState.Credits);
    }

    public void GoToWin()
        => GameManager.Instance.ChangeState(GameState.Win);


    // --- Save System Popup ---
    public void ClearSaveData()
    {
        if (confirmPopup == null)
        {
            Debug.LogWarning("ConfirmPopup not assigned in UIManager!");
            return;
        }

        confirmPopup.Show(
            "Are you sure you want to delete all save data?",
            () =>
            {
                SaveSystem.ClearSave();
                var gm = GameManager.Instance;

                gm.currencyManager.totalCurrency = 0;
                gm.currencyManager.runCurrency = 0;
                gm.progressionManager.ResetLevel();
                gm.upgradeManager.ResetUpgrades();

                UpdateLoadButtonInteractable(false);

                Debug.Log("Save data cleared!");
            },
            () =>
            {
                Debug.Log("Clear save canceled.");
            }
        );
    }


    // --- Button-Friendly Shortcuts ---
    public void GoToGameplayFromMenu() => GoToGameplay(false, false);
    public void GoToGameplayFromPause() => GoToGameplay(true, false);
    public void GoToGameplayFromUpgrades() => GoToGameplay(false, true);


    // --- Private Helper Methods ---
    private void DisableAllScreens()
    {
        foreach (var screen in allScreens)
        {
            if (screen != null)
                screen.SetActive(false);
        }
    }

    private void Activate(GameObject obj)
    {
        if (obj != null)
            obj.SetActive(true);
    }

    private void PlayClick()
    {
        SoundManager.Instance?.PlaySFX(SoundManager.Instance.buttonClick);
    }

    private void PlayStartGameSFX()
    {
        SoundManager.Instance?.PlaySFX(SoundManager.Instance.announcerSound);
    }

    private void ResetBackgroundScroll()
    {
        if (scrollingBackground != null)
        {
            scrollingBackground.ResetScroll();
        }
        else
        {
            // Fallback
            var scroll = FindFirstObjectByType<ScrollingBackground>();
            scroll?.ResetScroll();
        }
    }
}
