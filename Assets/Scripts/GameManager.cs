using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Gameplay,
        Pause,
        Options,
        Upgrades,
        Results,
        Win
    }
    
    public static GameManager Instance;
    public GameState currentState;
    public UIManager uiManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ChangeState(GameState.MainMenu);
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        uiManager.UpdateUI(newState);
    }
}
