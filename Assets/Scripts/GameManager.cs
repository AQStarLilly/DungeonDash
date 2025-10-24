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
        Instructions,
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
    public SoundManager soundManager;
    public UpgradeManager upgradeManager;

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

    [Header("Gameplay Container")]
    public GameObject gameplayContainer;

    private HealthSystem playerHealth;
    private HealthSystem currentEnemy;
    private Coroutine battleLoop;
    private bool isPaused = false;
    private bool loadingFromSave = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ChangeState(GameState.MainMenu);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Gameplay)
            {
                ChangeState(GameState.Pause);
            }
            else if (currentState == GameState.Pause)
            {
                ChangeState(GameState.Gameplay);
            }
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayMusicForState(newState);
        uiManager.UpdateUI(newState);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMusicForState(newState);
        }

        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                ResetGame(resetUpgrades: false, resetCurrency: false);
                gameplayContainer.SetActive(false);

                BackgroundManager.Instance.ShowMainMenuBackground();
                uiManager.UpdateLoadButtonInteractable(SaveSystem.HasSave());
                break;
            case GameState.Gameplay:
                gameplayContainer.SetActive(true);
                BackgroundManager.Instance.ShowGameplayBackground();
                Time.timeScale = 1f;
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
                gameplayContainer.SetActive(true);
                Time.timeScale = 0f;
                break;

            case GameState.PauseMenuOptions:
                gameplayContainer.SetActive(false);
                break;

            case GameState.Results:
                gameplayContainer.SetActive(false);
                break;
            case GameState.Win:
                EndBattle();
                StartCoroutine(ReturnToMenuMusicWithDelay(2f));
                break;
            case GameState.Upgrades:
                if (upgradesCurrencyText != null)
                    upgradesCurrencyText.text = $"{currencyManager.totalCurrency}";
                if (UpgradeManager.Instance != null)
                    UpgradeManager.Instance.UpdateAllButtons();
                break;
        }
    }

    // Save / Load Handlers //
    public void StartNewGame()
    {
        SaveSystem.ClearSave();
        ResetGame(resetUpgrades: true, resetCurrency: true);

        var scroll = Object.FindFirstObjectByType<ScrollingBackground>();
        if (scroll != null)
            scroll.ResetScroll();

        ChangeState(GameState.Instructions);
    }

    public void LoadSavedGame()
    {
        if (SaveSystem.HasSave())
        {
            int savedTotalCurrency;
            int savedRunCurrency;
            int savedLastRun;
            int savedWave;

            SaveSystem.LoadGame(out savedTotalCurrency, out savedRunCurrency, out savedLastRun, out savedWave);

            currencyManager.totalCurrency = savedTotalCurrency;
            currencyManager.runCurrency = savedRunCurrency;
            currencyManager.lastRunEarnings = savedLastRun;
            progressionManager.SetCurrentLevel(savedWave);

            currencyManager.UpdateUI();

            loadingFromSave = true;
            ChangeState(GameState.Gameplay);
        }
    }

    public void QuitToMainMenuAndSave()
    {
        SaveSystem.SaveGame(currencyManager.totalCurrency, currencyManager.runCurrency, currencyManager.lastRunEarnings, progressionManager.GetCurrentLevel());
        ChangeState(GameState.MainMenu);
    }


    // Battle Logic //
    private void StartBattle()
    {
        // Clear old results
        if (resultsCurrencyText != null) resultsCurrencyText.text = "";
        if (resultsWavesText != null) resultsWavesText.text = "";

        if (!loadingFromSave)
        {
            currencyManager.ResetRunCurrency();
            progressionManager.ResetLevel();   // wave = 1 (baseline)
        }

        // Clear the flag so it only applies once
        loadingFromSave = false;

        // Safety: nuke any leftovers from previous runs
        CleanupAllEnemiesInScene();

        // Spawn fresh player
        if (playerHealth != null)
        {
            Destroy(playerHealth.gameObject);
            playerHealth = null;
        }
        GameObject playerObj = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, gameplayContainer.transform);
        playerObj.transform.position = new Vector3(playerObj.transform.position.x, playerObj.transform.position.y, 0f);

        playerHealth = playerObj.GetComponent<HealthSystem>();
        playerHealth.isPlayer = true;
        playerHealth.healthText = playerHealthText;
        playerHealth.healthBarUI = GameObject.Find("PlayerHealthBarBG").GetComponent<HealthBarUI>();

        // initialize player stats for the start of the run
        bool firstSpawnOfRun = !loadingFromSave;
        playerHealth.InitializeFromPlayerStats(firstSpawnOfRun);

        SubscribeToPlayer();

        // First enemy (baseline, no scaling)
        currentEnemy = spawnManager.SpawnEnemy();
        currentEnemy.transform.SetParent(gameplayContainer.transform);
        currentEnemy.healthText = enemyHealthText;
        currentEnemy.healthBarUI = GameObject.Find("EnemyHealthBarBG").GetComponent<HealthBarUI>();
        // Force Z = 0 just in case
        currentEnemy.transform.position = new Vector3(currentEnemy.transform.position.x, currentEnemy.transform.position.y, 0f);
        currentEnemy.InitializeEnemy();
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
            {
                var (enemyDamage, enemyCrit) = currentEnemy.CalculateAttackDamage();
                playerHealth.TakeDamage(enemyDamage, enemyCrit);
            }

            // Player hits enemy
            if (playerHealth != null && currentEnemy != null)
            {
                var (playerDamage, playerCrit) = playerHealth.CalculateAttackDamage();
                currentEnemy.TakeDamage(playerDamage, playerCrit);
            }

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

        int currentWave = progressionManager.GetCurrentLevel();
        int maxWaves = progressionManager.GetMaxWaves();

        // Switch to boss music on the last wave
        if (currentWave == maxWaves - 1 && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBossMusic();
        }

        // if level > max, go to Win
        if (progressionManager.GetCurrentLevel() > progressionManager.GetMaxWaves())
        {
            ChangeState(GameState.Win);
            yield break;
        }

        currentEnemy = spawnManager.SpawnEnemy();
        currentEnemy.healthText = enemyHealthText;
        currentEnemy.transform.position = new Vector3(currentEnemy.transform.position.x, currentEnemy.transform.position.y, 0f);
        currentEnemy.InitializeEnemy();
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

    public void UpdateUpgradesCurrencyUI()
    {
        if (upgradesCurrencyText != null)
            upgradesCurrencyText.text = $"{currencyManager.totalCurrency}";
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
            resultsCurrencyText.text = $"{earnedThisRun}";

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
        if (playerHealth != null) playerHealth.ResetForNextWave();

        var scroll = Object.FindFirstObjectByType<ScrollingBackground>();
        if (scroll != null)
            StartCoroutine(scroll.ScrollLeft());

        int currentWave = progressionManager.GetCurrentLevel();
        int maxWaves = progressionManager.GetMaxWaves();

        if(currentWave >= maxWaves)
        {
            Debug.Log("You beat your Boss! Congratulations!");
            ChangeState(GameState.Win);
            return;
        }

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

    private void ResetGame(bool resetUpgrades = true, bool resetCurrency = true)
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

        if (resetCurrency && currencyManager != null)
        {
            currencyManager.totalCurrency = 0;
            currencyManager.ResetRunCurrency();
        }

        if (resetUpgrades && UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.ResetUpgrades();
        }

        CleanupAllEnemiesInScene();

        // Reset managers
        if (currencyManager != null) currencyManager.totalCurrency = 0;
        if (currencyManager != null) currencyManager.ResetRunCurrency();
        if (progressionManager != null) progressionManager.ResetLevel();

        Debug.Log("Game fully reset - back to Main Menu");
    }

    private IEnumerator ReturnToMenuMusicWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayMusicForState(GameState.MainMenu);
    }

    public void Quit()
    {
        Application.Quit();
    }
}