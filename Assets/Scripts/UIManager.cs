using UnityEngine;
using static GameManager;

public class UIManager : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject gameplayUI;
    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;
    public GameObject upgradesUI;
    public GameObject resultsUI;
    public GameObject winUI;

    public void UpdateUI(GameState state)
    {
        // Disable all
        mainMenuUI.SetActive(false);
        gameplayUI.SetActive(false);
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        upgradesUI.SetActive(false);
        resultsUI.SetActive(false);
        winUI.SetActive(false);      

        // Enable current
        switch (state)
        {
            case GameState.MainMenu: mainMenuUI.SetActive(true); break;
            case GameState.Gameplay: gameplayUI.SetActive(true); break;
            case GameState.Pause: pauseMenuUI.SetActive(true); break;
            case GameState.Options: optionsMenuUI.SetActive(true); break;
            case GameState.Upgrades: upgradesUI.SetActive(true); break;
            case GameState.Results: resultsUI.SetActive(true); break;
            case GameState.Win: winUI.SetActive(true); break;
        }
    }

    public void GoToMainMenu() => GameManager.Instance.ChangeState(GameState.MainMenu);
    public void GoToGameplay() => GameManager.Instance.ChangeState(GameState.Gameplay);
    public void GoToPause() => GameManager.Instance.ChangeState(GameState.Pause);
    public void GoToOptions() => GameManager.Instance.ChangeState(GameState.Options);
    public void GoToUpgrades() => GameManager.Instance.ChangeState(GameState.Upgrades);
    public void GoToResults() => GameManager.Instance.ChangeState(GameState.Results);
    public void GoToWin() => GameManager.Instance.ChangeState(GameState.Win);
}
