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
    public TMP_Text currencyText;   

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddCurrency(int baseAmount)
    {
        int finalAmount = Mathf.RoundToInt(baseAmount * currencyMultiplier);
        runCurrency += finalAmount;
        UpdateUI();
    }

    public void CommitRunToTotal()
    {
        totalCurrency += runCurrency;
        runCurrency = 0; // reset for next run
        UpdateUI();
    }

    public void ResetRunCurrency()
    {
        runCurrency = 0;
        UpdateUI();
    }

    public int GetRunCurrency()
    {
        return runCurrency;
    }

    public int GetTotalCurrency()
    {
        return totalCurrency;
    }

    public bool SpendCurrency(int amount)
    {
        if(totalCurrency >= amount)
        {
            totalCurrency -= amount;
            UpdateUI();
            return true;
        }
        else
        {
            Debug.Log("Not enough currency to spend");
            return false;
        }
    }

    private void UpdateUI()
    {
        if (currencyText != null)
            currencyText.text = $"Run: {runCurrency}";
    }
}