using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    public int currentCurrency = 0;
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

    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
        UpdateUI();
    }

    public void ResetCurrency()
    {
        currentCurrency = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currencyText != null)
            currencyText.text = $"Currency: {currentCurrency}";
    }
}