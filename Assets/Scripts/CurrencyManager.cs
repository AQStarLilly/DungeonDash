using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Currency Values")]
    public int totalCurrency = 0;   // Permanent across runs
    public int runCurrency = 0;     // Resets each run
    public float currencyMultiplier = 1f;

    [Header("UI")]
    public TMP_Text gameplayCurrencyText;
    public TMP_Text upgradesCurrencyText;
    public TMP_Text resultsCurrencyText;

    [Header("Last Run Data")]
    public int lastRunEarnings = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Duplicate CurrencyManager found. Destroying new one.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    // -------------------------------
    //         CURRENCY LOGIC
    // -------------------------------

    public void AddCurrency(int baseAmount)
    {
        int finalAmount = CurrencyMath.ApplyMultiplier(baseAmount, currencyMultiplier);

        runCurrency += finalAmount;
        runCurrency = Mathf.Max(0, runCurrency);
        UpdateUI();
    }

    public bool SpendCurrency(int amount)
    {
        if (totalCurrency >= amount)
        {
            totalCurrency -= amount;
            totalCurrency = Mathf.Max(0, totalCurrency);

            UpdateUI();
            return true;
        }

        Debug.Log("Not enough currency to spend.");
        return false;
    }

    public void CommitRunToTotal()
    {
        lastRunEarnings = runCurrency;

        totalCurrency += runCurrency;
        totalCurrency = Mathf.Max(0, totalCurrency);

        runCurrency = 0;
        UpdateUI();
    }

    public void ResetRunCurrency()
    {
        runCurrency = 0;
        UpdateUI();
    }

    // -------------------------------
    //              UI
    // -------------------------------

    public void UpdateUI()
    {
        if (gameplayCurrencyText != null)
            gameplayCurrencyText.text = runCurrency.ToString();
        else
            Debug.LogWarning("GameplayCurrencyText is not assigned.");

        if (upgradesCurrencyText != null)
            upgradesCurrencyText.text = totalCurrency.ToString();
        else
            Debug.LogWarning("UpgradesCurrencyText is not assigned.");

        if (resultsCurrencyText != null)
            resultsCurrencyText.text = runCurrency.ToString();
    }


    // -------------------------------
    //   STATIC UTILITY CLASS
    // -------------------------------
    public static class CurrencyMath
    {
        /// <summary>
        /// Safely applies multipliers with clamping.
        /// Demonstrates static class usage for assignment.
        /// </summary>
        public static int ApplyMultiplier(int baseAmount, float multiplier)
        {
            multiplier = Mathf.Max(0f, multiplier);
            return Mathf.RoundToInt(baseAmount * multiplier);
        }

    }
}