using UnityEngine;
using TMPro;

public class PlayerStatsPanel : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text healthText;
    public TMP_Text damageText;
    public TMP_Text damageReductionText;
    public TMP_Text currencyMultiplierText;

    private void OnEnable()
    {
        UpdateStatsUI();
    }

    public void UpdateStatsUI()
    {
        if (PlayerStats.Instance == null) return;

        int finalHealth = Mathf.RoundToInt(PlayerStats.Instance.baseHealth * PlayerStats.Instance.healthMultiplier);
        int finalDamage = Mathf.RoundToInt(PlayerStats.Instance.baseDamage * PlayerStats.Instance.damageMultiplier);

        float dmgReduction = PlayerStats.Instance.damageReduction;
        float currencyMult = CurrencyManager.Instance != null ? CurrencyManager.Instance.currencyMultiplier : 1f;

        if (healthText != null)
            healthText.text = $"Health: {finalHealth}";

        if (damageText != null)
            damageText.text = $"Damage: {finalDamage}";

        if (damageReductionText != null)
            damageReductionText.text = $"Damage Reduction: {Mathf.RoundToInt(dmgReduction * 100)}%";

        if (currencyMultiplierText != null)
            currencyMultiplierText.text = $"Currency Bonus: +{Mathf.RoundToInt((currencyMult - 1f) * 100)}%";
    }
}
