using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string upgradeId;

    public void OnPointerEnter(PointerEventData eventData)
    {
        var up = UpgradeManager.Instance.GetUpgrade(upgradeId);
        if (up == null) return;

        UpgradeManager.Instance.tooltip.ShowTooltip(up);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UpgradeManager.Instance.tooltip.HideTooltip();
    }
}
