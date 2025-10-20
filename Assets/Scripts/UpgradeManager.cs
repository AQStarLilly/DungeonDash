using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [System.Serializable]
    public class Upgrade
    {
        [Header("Identity")]
        public string id;                 // e.g. "damage1", "damage2", "health", "shield", "currency"
        public string displayName = "Upgrade";

        [Header("Progress")]
        public int level = 0;
        public int maxLevel = 5;

        [Header("Cost")]
        public int baseCost = 20;
        public float costMultiplier = 1.5f;

        [Header("Dependency (optional)")]
        public string requiresUpgradeId = "";   // leave empty if none
        public int requiresLevel = 0;           // e.g. 5 to unlock Damage II after Damage I max

        [Header("UI")]
        public Button button;
        public TMP_Text buttonText;

        public int CurrentCost => Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, level));
        public bool IsMaxed => level >= maxLevel;
    }

    [Header("Upgrades List")]
    public List<Upgrade> upgrades = new List<Upgrade>();

    //  colors for states 
    [Header("UI Colors")]
    public Color normalText = Color.white;
    public Color affordText = new Color(0.6f, 1f, 0.6f); 
    public Color lockedText = new Color(0.7f, 0.7f, 0.7f); 

    // quick lookup
    private Dictionary<string, Upgrade> map = new Dictionary<string, Upgrade>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // build map
        map.Clear();
        foreach (var up in upgrades)
        {
            if (!string.IsNullOrEmpty(up.id) && !map.ContainsKey(up.id))
                map.Add(up.id, up);
        }
    }

    private void Start()
    {
        if (upgrades.Count > 0)
        {
            UpdateAllButtons();
        }
    }

    private void OnEnable()
    {
        if (upgrades.Count > 0)
        {
            UpdateAllButtons();
        }
    }

    public void TryBuyUpgrade(string upgradeId)
    {
        if (!map.TryGetValue(upgradeId, out var up) || up == null) return;

        if (IsLocked(up))
        {
            // locked  ignore; (button should already be disabled)
            return;
        }

        if (up.IsMaxed)
        {
            // maxed  ignore
            return;
        }

        int cost = up.CurrentCost;
        if (CurrencyManager.Instance.SpendCurrency(cost))
        {
            up.level++;
            ApplyUpgradeEffect(up);
            UpdateAllButtons();
            GameManager.Instance.UpdateUpgradesCurrencyUI();
        }
        else
        {
            // cannot afford  flash red text
            StartCoroutine(FlashRed(up.buttonText));
        }
    }

    private bool IsLocked(Upgrade up)
    {
        if (string.IsNullOrEmpty(up.requiresUpgradeId) || up.requiresLevel <= 0) return false;
        if (!map.TryGetValue(up.requiresUpgradeId, out var req)) return true; 
        return req.level < up.requiresLevel;
    }

    private void ApplyUpgradeEffect(Upgrade up)
    {
        switch (up.id)
        {
            case "damage1":
                // +10% dmg per level
                PlayerStats.Instance.damageMultiplier += 0.10f;
                break;

            case "damage2":
                // +15% dmg per level (unlocks after damage1 max)
                PlayerStats.Instance.damageMultiplier += 0.15f;
                break;

            case "health":
                // +20% max HP per level (applied on next spawn/reset)
                PlayerStats.Instance.healthMultiplier += 0.20f;
                break;

            case "shield":
                // +10 flat shield per level (baked into max HP on reset, will fix so it's a seperate value - shield icon above head, depletes as you take damage)
                PlayerStats.Instance.shield += 10;
                break;

            case "currency":
                // +20% currency per level (applies immediately to future kills)
                CurrencyManager.Instance.currencyMultiplier += 0.20f;
                break;

            default:
                Debug.Log($"{up.displayName} effect not implemented");
                break;
        }
    }

    public void UpdateAllButtons()
    {
        if (upgrades == null || upgrades.Count == 0)
        {
            Debug.LogWarning("[UpgradeManager] No upgrades configured.");
            return;
        }

        foreach (var up in upgrades)
        {
            if (up == null)
            {
                Debug.LogWarning("[UpgradeManager] Null upgrade entry found in list.");
                continue;
            }

            if (up.button == null || up.buttonText == null)
            {
                Debug.LogWarning($"[UpgradeManager] Missing UI reference for upgrade: {up.displayName} ({up.id})");
                continue;
            }

            bool locked = IsLocked(up);
            bool maxed = up.IsMaxed;
            bool affordable = CurrencyManager.Instance != null &&
                              CurrencyManager.Instance.totalCurrency >= up.CurrentCost;

            if (maxed)
            {
                up.button.interactable = false;
                up.buttonText.text = $"{up.displayName} (MAX)";
                up.buttonText.color = lockedText;
            }
            else if (locked)
            {
                up.button.interactable = false;
                up.buttonText.text = $"{up.displayName} (Locked)";
                up.buttonText.color = lockedText;
            }
            else
            {
                up.button.interactable = affordable;
                up.buttonText.text = $"{up.displayName}  Lv.{up.level}  —  Cost: {up.CurrentCost}";
                up.buttonText.color = affordable ? affordText : normalText;
            }
        }
    }

    private IEnumerator FlashRed(TMP_Text text)
    {
        if (text == null) yield break;
        Color original = text.color;
        text.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        text.color = original;
    }

    public void ResetUpgrades()
    {
        foreach (var up in upgrades)
            up.level = 0;

        PlayerStats.Instance.ResetStats();
        UpdateAllButtons();
    }
}


//Wave 1 enemy - Health (30), Damage (8)
//Final Boss (wave 30) - Health (320), Damage (66)

//Player Start - Health (50), Damage (10)
//Full Upgrades (Lvl 5) - Health (100), Shield (50), Damage(20)
//Maybe double levels (Lvl 10)- Health (150), Shield (100) - Total (250) health aspect(tweak this slightly will be perfect since were only behind 70 health
// Gain 15 health/shield per upgrade instead of 10. - Health (200), Shield (150) - Total (350) health aspect. 
// Damage (30) - only half of what the boss has - change damage to 3 damage per upgrade triples everything so roughly 70 damage at max
// Fix combat so its turn based or increase health/damage as both player and boss would die at the same time.

/* Definitely need to tweak (stats should be similar to boss by the time you get to final wave)
 * All upgrades or mostly all upgrades unlocked to beat boss?
 * 20-30 minutes to beat game?
 */