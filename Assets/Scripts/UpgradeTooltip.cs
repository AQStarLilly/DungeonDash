using UnityEngine;
using TMPro;

public class UpgradeTooltip : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text levelText;
    public TMP_Text costText;

    [Header("Cost Colors")]
    public Color canAffordColor = Color.green;
    public Color cannotAffordColor = Color.red;

    private UpgradeManager.Upgrade currentUpgrade;

    public void ShowTooltip(UpgradeManager.Upgrade up)
    {
        currentUpgrade = up;
        gameObject.SetActive(true);
        Refresh();
    }

    public void Refresh()
    {
        if (currentUpgrade == null) return;

        titleText.text = currentUpgrade.displayName;

        int currentWave = GameManager.Instance.progressionManager.GetCurrentLevel();
        int cost = currentUpgrade.CurrentCost;
        bool canAfford = CurrencyManager.Instance.totalCurrency >= cost;

        //
        // --- WAVE LOCK (ability upgrades) ---
        //
        if (currentUpgrade.requiredWave > 0 && !currentUpgrade.permanentlyUnlocked && currentWave < currentUpgrade.requiredWave)
        {
            descriptionText.text =
                $"<color=#FF8080>Locked</color>\nReach wave {currentUpgrade.requiredWave} to unlock.";

            levelText.text = $"{currentUpgrade.level}/{currentUpgrade.maxLevel}";
            costText.text = $"{cost}";
            costText.color = canAfford ? canAffordColor : cannotAffordColor;
            return;
        }

        //
        // --- DEPENDENCY LOCK (damage2) ---
        //
        if (UpgradeManager.Instance.IsLocked(currentUpgrade))
        {
            var req = UpgradeManager.Instance.GetUpgrade(currentUpgrade.requiresUpgradeId);
            string reqName = req != null ? req.displayName : currentUpgrade.requiresUpgradeId;

            descriptionText.text =
                $"<color=#FF8080>Locked</color>\nRequires {reqName} level {currentUpgrade.requiresLevel}";

            levelText.text = $"{currentUpgrade.level}/{currentUpgrade.maxLevel}";
            costText.text = $"{cost}";
            costText.color = canAfford ? canAffordColor : cannotAffordColor;
            return;
        }

        //
        // --- NORMAL DESCRIPTION ---
        //
        descriptionText.text = currentUpgrade.description;
        levelText.text = $"{currentUpgrade.level}/{currentUpgrade.maxLevel}";

        costText.text = $"{cost}";
        costText.color = canAfford ? canAffordColor : cannotAffordColor;
    }

    public void HideTooltip()
    {
        currentUpgrade = null;
        gameObject.SetActive(false);
    }

    public bool IsVisible => gameObject.activeSelf;
}
