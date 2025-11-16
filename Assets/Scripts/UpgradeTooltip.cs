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
