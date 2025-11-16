using UnityEngine;
using TMPro;

public class UpgradeTooltip : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text levelText;
    public TMP_Text costText;

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

        // Show unlock requirement if locked 
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.IsLocked(currentUpgrade))
        {
            var required = UpgradeManager.Instance.GetUpgrade(currentUpgrade.requiresUpgradeId);

            string requiredName = required != null ? required.displayName : currentUpgrade.requiresUpgradeId;

            descriptionText.text =
                $"<color=#FF8080>Locked</color>\nRequires {requiredName} level {currentUpgrade.requiresLevel}";

            levelText.text = $"{currentUpgrade.level}/{currentUpgrade.maxLevel}";
            costText.text = $"{currentUpgrade.CurrentCost}";
            return;
        }

        // --- Normal behavior when UNLOCKED ---
        descriptionText.text = currentUpgrade.description;
        levelText.text = $"{currentUpgrade.level}/{currentUpgrade.maxLevel}";
        costText.text = $"{currentUpgrade.CurrentCost}";
    }

    public void HideTooltip()
    {
        currentUpgrade = null;
        gameObject.SetActive(false);
    }

    public bool IsVisible => gameObject.activeSelf;
}
