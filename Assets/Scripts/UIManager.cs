using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class UIManager : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject gameplayUI;
    public GameObject pausePanel;
    public GameObject gameInstructionsUI;
    public GameObject mainMenuOptionsUI;
    public GameObject pauseMenuOptionsUI;
    public GameObject upgradesUI;
    public GameObject resultsUI;
    public GameObject winUI;

    [Header("Main Menu Button")]
    public Button loadGameButton;

    [Header("Popups")]
    public ConfirmPopup confirmPopup;


    public void UpdateUI(GameState state)
    {
        // Disable all
        mainMenuUI.SetActive(false);
        gameplayUI.SetActive(false);
        gameInstructionsUI.SetActive(false);
        mainMenuOptionsUI.SetActive(false);
        pauseMenuOptionsUI.SetActive(false);
        upgradesUI.SetActive(false);
        resultsUI.SetActive(false);
        winUI.SetActive(false);

        pausePanel.SetActive(false);

        // Enable current
        switch (state)
        {
            case GameState.MainMenu: mainMenuUI.SetActive(true); break;
            case GameState.Gameplay: gameplayUI.SetActive(true); break;
            case GameState.Pause:
                gameplayUI.SetActive(true);
                pausePanel.SetActive(true);
                break;
            case GameState.Instructions: gameInstructionsUI.SetActive(true); break;
            case GameState.MainMenuOptions: mainMenuOptionsUI.SetActive(true); break;
            case GameState.PauseMenuOptions: pauseMenuOptionsUI.SetActive(true); break;
            case GameState.Upgrades: upgradesUI.SetActive(true); break;
            case GameState.Results: resultsUI.SetActive(true); break;
            case GameState.Win: winUI.SetActive(true); break;
        }
    }

    public void UpdateLoadButtonInteractable(bool hasSave)
    {
        if(loadGameButton != null)
        {
            loadGameButton.interactable = hasSave;
        }
    }

    public void GoToMainMenu() => GameManager.Instance.ChangeState(GameState.MainMenu);
    public void GoToGameplay() => GameManager.Instance.ChangeState(GameState.Gameplay);
    public void GoToPause() => GameManager.Instance.ChangeState(GameState.Pause);
    public void GoToInstructions() => GameManager.Instance.ChangeState(GameState.Instructions);
    public void GoToMainMenuOptions() => GameManager.Instance.ChangeState(GameState.MainMenuOptions);
    public void GoToPauseMenuOptions() => GameManager.Instance.ChangeState(GameState.PauseMenuOptions);
    public void GoToUpgrades() => GameManager.Instance.ChangeState(GameState.Upgrades);
    public void GoToResults() => GameManager.Instance.ChangeState(GameState.Results);
    public void GoToWin() => GameManager.Instance.ChangeState(GameState.Win);


    public void ClearSaveData()
    {
        if (confirmPopup != null)
        {
            confirmPopup.Show("Are you sure you want to delete all save data?",() =>
                {
                    // On Confirm
                    SaveSystem.ClearSave();
                    GameManager.Instance.currencyManager.totalCurrency = 0;
                    GameManager.Instance.currencyManager.runCurrency = 0;
                    GameManager.Instance.progressionManager.ResetLevel();
                    GameManager.Instance.upgradeManager.ResetUpgrades();
                    UpdateLoadButtonInteractable(false);
                    Debug.Log("Save data cleared!");
                },
                () =>
                {
                    // On Cancel
                    Debug.Log("Clear save canceled.");
                }
            );
        }
        else
        {
            Debug.LogWarning("ConfirmPopup not assigned in UIManager!");
        }
    }
}
