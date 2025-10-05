using UnityEngine;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Gameplay,
        Pause,
        MainMenuOptions,
        PauseMenuOptions,
        Upgrades,
        Results,
        Win
    }

    public static GameManager Instance;

    public GameState currentState;

    [Header("Managers")]
    public UIManager uiManager;
    public SpawnManager spawnManager;
    public EnemyManager enemyManager;
    public ProgressionManager progressionManager;
    public CurrencyManager currencyManager;

    [Header("Prefabs & Spawns")]
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;

    [Header("UI References")]
    public TMP_Text playerHealthText;
    public TMP_Text enemyHealthText;
    public TMP_Text waveCounterText;

    [Header("Results UI")]
    public TMP_Text resultsCurrencyText;
    public TMP_Text resultsWavesText;

    [Header("Upgrades UI")]
    public TMP_Text upgradesCurrencyText;

    private HealthSystem playerHealth;
    private HealthSystem currentEnemy;
    private Coroutine battleLoop;
    private bool isPaused = false;

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

        switch (newState)
        {
            case GameState.MainMenu:
                ResetGame();
                break;
            case GameState.Gameplay:
                if (playerHealth == null || currentEnemy == null)
                {
                    // Fresh run
                    StartBattle();
                }
                else
                {
                    // Resume battle if paused
                    ResumeBattle();
                }
                break;

            case GameState.Pause:
                PauseBattle();
                break;

            case GameState.Results:
            case GameState.Win:
                EndBattle();
                break;
            case GameState.Upgrades:
                if (upgradesCurrencyText != null)
                    upgradesCurrencyText.text = $"Gold Available: {currencyManager.totalCurrency}";
                break;
        }
    }

    private void StartBattle()
    {
        // Clear old results
        if (resultsCurrencyText != null) resultsCurrencyText.text = "";
        if (resultsWavesText != null) resultsWavesText.text = "";
        currencyManager.ResetRunCurrency();
        progressionManager.ResetLevel();   // wave = 1 (baseline)

        // Safety: nuke any leftovers from previous runs
        CleanupAllEnemiesInScene();

        // Spawn fresh player
        if (playerHealth != null)
        {
            Destroy(playerHealth.gameObject);
            playerHealth = null;
        }
        GameObject playerObj = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        // Force Z = 0 just in case
        playerObj.transform.position = new Vector3(playerObj.transform.position.x, playerObj.transform.position.y, 0f);

        playerHealth = playerObj.GetComponent<HealthSystem>();
        playerHealth.healthText = playerHealthText;
        playerHealth.ResetHealth();
        SubscribeToPlayer();

        // First enemy (baseline, no scaling)
        currentEnemy = spawnManager.SpawnEnemy();
        currentEnemy.healthText = enemyHealthText;
        // Force Z = 0 just in case
        currentEnemy.transform.position = new Vector3(currentEnemy.transform.position.x, currentEnemy.transform.position.y, 0f);
        currentEnemy.ResetHealth();
        SubscribeToEnemy(currentEnemy);

        UpdateWaveCounter();

        if (battleLoop != null) StopCoroutine(battleLoop);
        battleLoop = StartCoroutine(BattleRoutine());
    }

    private IEnumerator BattleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (currentState != GameState.Gameplay)
                yield break;

            // If either side is gone - stop safely
            if (playerHealth == null || currentEnemy == null)
                yield break;

            // Enemy hits player
            if (playerHealth != null && currentEnemy != null)
                playerHealth.TakeDamage(currentEnemy.attackDamage);

            // Player hits enemy
            if (playerHealth != null && currentEnemy != null)
                currentEnemy.TakeDamage(playerHealth.attackDamage);

            // If either died during attacks, bail out immediately
            if (playerHealth == null || currentEnemy == null)
                yield break;

            if (playerHealth.currentHealth <= 0)
                yield break; // HandlePlayerDeath will trigger

            if (currentEnemy.currentHealth <= 0)
                yield break; // HandleEnemyDeath will trigger
        }
    }

    private IEnumerator SpawnNextEnemy()
    {
        yield return new WaitForSeconds(2f);

        progressionManager.IncreaseLevel();

        // if level > max, go to Win
        if (progressionManager.GetCurrentLevel() > progressionManager.GetMaxWaves())
        {
            ChangeState(GameState.Win);
            yield break;
        }

        currentEnemy = spawnManager.SpawnEnemy();
        currentEnemy.healthText = enemyHealthText;
        currentEnemy.transform.position = new Vector3(currentEnemy.transform.position.x, currentEnemy.transform.position.y, 0f);
        currentEnemy.ResetHealth();
        SubscribeToEnemy(currentEnemy);

        UpdateWaveCounter();

        battleLoop = StartCoroutine(BattleRoutine());
    }

    private void EndBattle()
    {
        if (battleLoop != null)
        {
            StopCoroutine(battleLoop);
            battleLoop = null;
        }

        UnsubscribeAll();

        if (currentEnemy != null)
        {
            Destroy(currentEnemy.gameObject);
            currentEnemy = null;
        }

        if (playerHealth != null)
        {
            Destroy(playerHealth.gameObject);
            playerHealth = null;
        }

        // Safety: if anything slipped through, clean it
        CleanupAllEnemiesInScene();
    }

    private void UpdateWaveCounter()
    {
        int current = progressionManager.GetCurrentLevel();
        int max = progressionManager.GetMaxWaves();

        if (waveCounterText != null)
            waveCounterText.text = $"Wave {current}/{max}";
    }

    private void HandlePlayerDeath(HealthSystem player)
    {
        Debug.Log("[GameManager] HandlePlayerDeath()");

        if (battleLoop != null)
        {
            StopCoroutine(battleLoop);
            battleLoop = null;
        }

        player.OnDeath -= HandlePlayerDeath;

        if (player != null) Destroy(player.gameObject);
        playerHealth = null;

        // Capture how much was earned BEFORE committing
        int earnedThisRun = currencyManager.runCurrency;

        // Save gold from this run to total (this resets runCurrency to 0)
        currencyManager.CommitRunToTotal();

        // Show Results screen
        ChangeState(GameState.Results);

        // Update Results UI
        int wavesCleared = progressionManager.GetCurrentLevel() - 1;

        if (resultsCurrencyText != null)
            resultsCurrencyText.text = $"Currency Earned: {earnedThisRun}";

        if (resultsWavesText != null)
            resultsWavesText.text = $"Waves Cleared: {wavesCleared}";
    }

    private void HandleEnemyDeath(HealthSystem enemy)
    {
        Debug.Log("[GameManager] HandleEnemyDeath()");

        // Stop battle loop first
        if (battleLoop != null)
        {
            StopCoroutine(battleLoop);
            battleLoop = null;
        }

        currencyManager.AddCurrency(10);

        // Unsubscribe
        enemy.OnDeath -= HandleEnemyDeath;

        // Destroy enemy
        if (enemy != null) Destroy(enemy.gameObject);
        currentEnemy = null;

        // Heal player back to full
        if (playerHealth != null) playerHealth.ResetHealth();

        // Spawn next enemy after a delay
        StartCoroutine(SpawnNextEnemy());
    }

    private void PauseBattle()
    {
        if (battleLoop != null)
        {
            StopCoroutine(battleLoop);
            battleLoop = null;
        }
        isPaused = true;
        Debug.Log("Battle paused");
    }

    private void ResumeBattle()
    {
        if (isPaused)
        {
            battleLoop = StartCoroutine(BattleRoutine());
            isPaused = false;
            Debug.Log("Battle resumed");
        }
    }

    private void CleanupAllEnemiesInScene()
    {
        // Destroy every HealthSystem that isn’t the current player
        var all = Object.FindObjectsByType<HealthSystem>(FindObjectsSortMode.None);
        foreach (var hs in all)
        {
            if (hs != null && hs != playerHealth)
            {
                Destroy(hs.gameObject);
            }
        }
    }

    private void SubscribeToPlayer()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath; // avoid double-subscribe
            playerHealth.OnDeath += HandlePlayerDeath;
        }
    }

    private void SubscribeToEnemy(HealthSystem enemy)
    {
        if (enemy != null)
        {
            enemy.OnDeath -= HandleEnemyDeath; // avoid double-subscribe
            enemy.OnDeath += HandleEnemyDeath;
        }
    }

    private void UnsubscribeAll()
    {
        if (currentEnemy != null) currentEnemy.OnDeath -= HandleEnemyDeath;
        if (playerHealth != null) playerHealth.OnDeath -= HandlePlayerDeath;
    }

    private void ResetGame()
    {
        // Stop any battle loops
        if (battleLoop != null)
        {
            StopCoroutine(battleLoop);
            battleLoop = null;
        }

        // Clean up any active combatants
        UnsubscribeAll();

        if (currentEnemy != null)
        {
            Destroy(currentEnemy.gameObject);
            currentEnemy = null;
        }

        if (playerHealth != null)
        {
            Destroy(playerHealth.gameObject);
            playerHealth = null;
        }

        CleanupAllEnemiesInScene();

        // Reset managers
        if (currencyManager != null) currencyManager.totalCurrency = 0;
        if (currencyManager != null) currencyManager.ResetRunCurrency();
        if (progressionManager != null) progressionManager.ResetLevel();

        Debug.Log("Game fully reset - back to Main Menu");
    }

    public void Quit()
    {
        Application.Quit();
    }
}