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
        Credits,
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
    public TMP_Text enemiesBeatText;
    public TMP_Text resultsCurrencyMultText;

    [Header("Upgrades UI")]
    public TMP_Text upgradesCurrencyText;

    [Header("Ability Buttons")]
    public AbilityButton janitorButton;
    public AbilityButton hrLadyButton;
    public AbilityButton drunkButton;

    [Header("Tutorial Popup")]
    public GameObject abilityTutorialPopup;

    [Header("Gameplay Container")]
    public GameObject gameplayContainer;

    private HealthSystem playerHealth;
    public HealthSystem currentEnemy;
    private Coroutine battleLoop;
    private bool isPaused = false;
    private bool loadingFromSave = false;
    private bool isSpawningEnemy = false;
    private Coroutine stageTransitionCR;

    public bool HasShownAbilityPopup
    {
        get => PlayerPrefs.GetInt("HasShownAbilityPopup", 0) == 1;
        set => PlayerPrefs.SetInt("HasShownAbilityPopup", value ? 1 : 0);
    }

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

                if (isSpawningEnemy)
                {
                    Debug.Log("GameManager gameplay resumed, waiting for next enemy spawn");
                    break;
                }

                if (playerHealth == null && currentEnemy == null)
                {
                    // Starting a new run
                    Debug.Log("[GameManager] Starting a fresh battle.");
                    StartBattle();
                }
                else if (playerHealth != null && currentEnemy != null)
                {
                    // Resuming an active battle
                    Debug.Log("[GameManager] Resuming ongoing battle.");
                    ResumeBattle();
                }
                else if (playerHealth != null && currentEnemy == null)
                {
                    // Between waves (during stage scroll) — do nothing, let the coroutine handle spawning
                    Debug.Log("[GameManager] Gameplay resumed mid-transition (waiting for next wave).");
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

    public void StartGameplay(bool fromUpgrades)
    {
        if (fromUpgrades)
        {
            Debug.Log("[GameManager] Starting fresh run from Upgrades screen.");

            // Fully reset combat-related things
            CleanupAllEnemiesInScene();
            progressionManager.ResetLevel();      // wave = 1
            currencyManager.ResetRunCurrency();   // clear temporary earnings
            playerHealth = null;
            currentEnemy = null;
            loadingFromSave = false;
        }

        ChangeState(GameState.Gameplay);
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
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.ApplyAllUpgradeEffects();
                UpgradeManager.Instance.UpdateAllButtons();
            }

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
        SetupAbilityButtons();

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
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            if (currentState != GameState.Gameplay)
                yield break;

            // If anyone is null, stop the loop
            if (playerHealth == null || currentEnemy == null)
                yield break;

            //
            // --- PLAYER TURN ---
            //
            var (playerDamage, playerCrit) = playerHealth.CalculateAttackDamage();
            currentEnemy.TakeDamage(playerDamage, playerCrit);
            SoundManager.Instance?.PlaySFX(SoundManager.Instance.playerAttack);

            // Wait briefly for hit animation / damage text
            yield return new WaitForSeconds(0.5f);

            // Check if enemy died
            if (currentEnemy == null || currentEnemy.currentHealth <= 0)
            {
                HandleEnemyDeath(currentEnemy);
                yield break;
            }

            //
            // --- ENEMY TURN ---
            //
            var (enemyDamage, enemyCrit) = currentEnemy.CalculateAttackDamage();
            playerHealth.TakeDamage(enemyDamage, enemyCrit);

            if (progressionManager.GetCurrentLevel() == progressionManager.GetMaxWaves())
            {
                SoundManager.Instance?.PlaySFX(SoundManager.Instance.bossAttack);
            }
            else
            {
                SoundManager.Instance?.PlaySFX(SoundManager.Instance.enemyAttack);
            }

            yield return new WaitForSeconds(0.5f);

            // Check if player died
            if (playerHealth == null || playerHealth.currentHealth <= 0)
            {
                HandlePlayerDeath(playerHealth);
                yield break;
            }

            // Short delay before next round
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator SpawnNextEnemy()
    {
        if (isSpawningEnemy) yield break;
        isSpawningEnemy = true;

        // Safety: stop if player paused or game is not in active gameplay
        if (currentState != GameState.Gameplay)
        {
            Debug.Log("[GameManager] SpawnNextEnemy aborted — game not in Gameplay state.");
            isSpawningEnemy = false;
            yield break;
        }

        progressionManager.IncreaseLevel();
        if (progressionManager.GetCurrentLevel() == progressionManager.GetMaxWaves())
        {
            SoundManager.Instance?.PlaySFX(SoundManager.Instance.bossHowl);
        }

        var visual = FindFirstObjectByType<WaveNumberVisual>();
        if (visual != null)
        {
            int newWave = progressionManager.GetCurrentLevel();
            visual.SetWave(newWave, false); // false = red (new wave active)
        }

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
            isSpawningEnemy = false;
            yield break;
        }

        currentEnemy = spawnManager.SpawnEnemy();
        currentEnemy.healthText = enemyHealthText;
        currentEnemy.transform.position = new Vector3(currentEnemy.transform.position.x, currentEnemy.transform.position.y, 0f);
        currentEnemy.InitializeEnemy();
        SubscribeToEnemy(currentEnemy);

        UpdateWaveCounter();

        isSpawningEnemy = false;

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

        var visual = FindFirstObjectByType<WaveNumberVisual>();
        if (visual != null)
        {
            visual.SetWave(current, false); // false = red (active)
        }
    }

    public void UpdateUpgradesCurrencyUI()
    {
        if (upgradesCurrencyText != null)
            upgradesCurrencyText.text = $"{currencyManager.totalCurrency}";
    }

    private void HandlePlayerDeath(HealthSystem player)
    {
        Debug.Log("[GameManager] HandlePlayerDeath()");
        SoundManager.Instance?.PlaySFX(SoundManager.Instance.playerDeath);

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
        int maxWaves = progressionManager.GetMaxWaves();

        int baseGoldPerWave = 10;
        float mult = CurrencyManager.Instance.currencyMultiplier;

        if (resultsWavesText != null)
            resultsWavesText.text = $"Waves Cleared: {wavesCleared} / {maxWaves}";

        if (enemiesBeatText != null)
            enemiesBeatText.text = $"Enemies Beat: {wavesCleared} x {baseGoldPerWave}";

        if (resultsCurrencyMultText != null)
            resultsCurrencyMultText.text = $"Currency Multiplier: x {mult:0.0}";

        if (resultsCurrencyText != null)
            resultsCurrencyText.text = $"{earnedThisRun}";       
    }

    private void HandleEnemyDeath(HealthSystem enemy)
    {
        Debug.Log("[GameManager] HandleEnemyDeath()");
        SoundManager.Instance?.PlaySFX(SoundManager.Instance.enemyDeath);
        if (progressionManager.GetCurrentLevel() >= progressionManager.GetMaxWaves())
        {
            SoundManager.Instance?.PlaySFX(SoundManager.Instance.bossDeath);
        }

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

        int currentWave = progressionManager.GetCurrentLevel();
        int maxWaves = progressionManager.GetMaxWaves();

        var visual = FindFirstObjectByType<WaveNumberVisual>();
        if (visual != null)
        {
            visual.SetWave(currentWave, true); // true = grey (wave cleared)
        }

        if (currentWave >= maxWaves)
        {
            Debug.Log("You beat your Boss! Congratulations!");
            ChangeState(GameState.Win);
            return;
        }

        var scroll = Object.FindFirstObjectByType<ScrollingBackground>();
        if (scroll != null)
        {
            if (stageTransitionCR != null)
                StopCoroutine(stageTransitionCR);

            // Chain the scroll and spawn together
            stageTransitionCR = StartCoroutine(ScrollThenSpawn(scroll));
        }
        else
        {
            // Fallback: spawn directly if no background found
            StartCoroutine(SpawnNextEnemy());
        }
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
            // Only resume if we’re not in the middle of spawning
            if (!isSpawningEnemy && playerHealth != null && currentEnemy != null && battleLoop == null)
            {
                battleLoop = StartCoroutine(BattleRoutine());
                Debug.Log("Battle resumed");
            }

            isPaused = false;         
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

    private IEnumerator ScrollThenSpawn(ScrollingBackground scroll)
    {
        // Run the scroll animation first
        yield return StartCoroutine(scroll.ScrollLeft());

        // brief 0.25s delay for polish
        yield return new WaitForSeconds(0.25f);

        // Then immediately start spawning next enemy
        StartCoroutine(SpawnNextEnemy());
    }

    private void SetupAbilityButtons()
    {
        void Setup(AbilityButton button, string id)
        {
            var up = UpgradeManager.Instance.GetUpgrade(id);
            if (up != null && up.level > 0 && up.isActiveAbility)
            {
                // <-- make sure it's ON before initializing
                if (!button.gameObject.activeSelf)
                    button.gameObject.SetActive(true);

                button.Initialize(up); // sets sprites/cooldown/damage, etc.
            }
            else
            {
                button.gameObject.SetActive(false);
            }
        }

        Setup(janitorButton, "janitor");
        Setup(hrLadyButton, "hrlady");
        Setup(drunkButton, "drunkCoworker");

        bool hasAnyAbilityUnlocked =
        UpgradeManager.Instance.GetUpgrade("janitor").level > 0 ||
        UpgradeManager.Instance.GetUpgrade("hrlady").level > 0 ||
        UpgradeManager.Instance.GetUpgrade("drunkCoworker").level > 0;

        if (hasAnyAbilityUnlocked && !HasShownAbilityPopup)
        {
            abilityTutorialPopup.SetActive(true);
            HasShownAbilityPopup = true;
        }
    }
    public void CloseAbilityPopup()
    {
        abilityTutorialPopup.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}