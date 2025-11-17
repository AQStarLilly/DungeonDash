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
        public string id;
        public string displayName = "Upgrade";

        [TextArea]
        public string description;

        [Header("Progress")]
        public int level = 0;
        public int maxLevel = 10;

        [Header("Cost")]
        public int baseCost = 20;
        public float costMultiplier = 1.5f;

        [Header("Dependency (optional)")]
        public string requiresUpgradeId = "";
        public int requiresLevel = 0;

        [Header("Wave Unlock (optional)")]
        public int requiredWave = 0;

        [Header("UI")]
        public Button button;
        public TMP_Text buttonText;
        public Image displayImage;
        public List<Sprite> levelSprites;

        public int CurrentCost => Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, level));
        public bool IsMaxed => level >= maxLevel;

        [HideInInspector] public UpgradeTooltip tooltip;

        [Header("Active Ability Settings")]
        public bool isActiveAbility = false;
        public int abilityDamage = 20;
        public float cooldown = 10f;
        public Sprite abilityIcon;
    }

    [Header("Upgrades List")]
    public List<Upgrade> upgrades = new List<Upgrade>();

    [Header("UI Colors")]
    public Color cannotAffordColor = Color.red;
    public Color canAffordColor = Color.green;
    public Color lockedTextColor = new Color(0.7f, 0.7f, 0.7f);

    private Dictionary<string, Upgrade> map = new Dictionary<string, Upgrade>();

    [Header("Tooltip UI")]
    public UpgradeTooltip tooltip;

    public Upgrade GetUpgrade(string id)
    {
        map.TryGetValue(id, out var up);
        return up;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        map.Clear();
        foreach (var up in upgrades)
        {
            if (!string.IsNullOrEmpty(up.id) && !map.ContainsKey(up.id))
                map.Add(up.id, up);
        }
    }

    private void OnEnable()
    {
        UpdateAllButtons();
    }

    private void Start()
    {
        UpdateAllButtons();
    }

    public void TryBuyUpgrade(string upgradeId)
    {
        if (!map.TryGetValue(upgradeId, out var up) || up == null) return;

        if (IsLocked(up)) return;
        if (up.IsMaxed) return;

        int cost = up.CurrentCost;

        if (CurrencyManager.Instance.SpendCurrency(cost))
        {
            up.level++;
            SoundManager.Instance?.PlaySFX(SoundManager.Instance.upgradePurchase);

            ApplyUpgradeEffect(up);
            UpdateAllButtons();

            var statsPanel = FindFirstObjectByType<PlayerStatsPanel>();
            if (statsPanel != null) statsPanel.UpdateStatsUI();

            GameManager.Instance.UpdateUpgradesCurrencyUI();

            if (tooltip != null && tooltip.IsVisible)
                tooltip.Refresh();
        }
        else
        {
            StartCoroutine(FlashRed(up.buttonText));
        }
    }

    public bool IsLocked(Upgrade up)
    {
        // Dependency lock (damage2)
        if (string.IsNullOrEmpty(up.requiresUpgradeId) || up.requiresLevel <= 0)
            return false;

        if (!map.TryGetValue(up.requiresUpgradeId, out var req))
            return true;

        return req.level < up.requiresLevel;
    }

    private void ApplyUpgradeEffect(Upgrade up)
    {
        switch (up.id)
        {
            case "damage1":
                PlayerStats.Instance.damageMultiplier += 0.20f;
                break;

            case "damage2":
                PlayerStats.Instance.damageMultiplier += 0.40f;
                break;

            case "health":
                PlayerStats.Instance.healthMultiplier += 0.70f;
                break;

            case "shield":
                float reductionPerLevel = 0.05f;
                PlayerStats.Instance.damageReduction =
                    Mathf.Clamp01(up.level * reductionPerLevel);

                var player = FindFirstObjectByType<HealthSystem>();
                if (player != null && player.isPlayer && player.shieldVisual != null)
                    player.shieldVisual.ShowShield();
                break;

            case "currency":
                CurrencyManager.Instance.currencyMultiplier += 0.20f;
                break;

            case "janitor":
            case "hrlady":
            case "drunkCoworker":
                break;
        }
    }

    public void UpdateAllButtons()
    {
        foreach (var up in upgrades)
        {
            if (up == null) continue;

            // --- SAFETY FIX ---
            // Force empty dependency to behave correctly even if inspector saved whitespace
            if (up.requiresUpgradeId != null && up.requiresUpgradeId.Trim() == "")
                up.requiresUpgradeId = "";

            bool affordable = CurrencyManager.Instance.totalCurrency >= up.CurrentCost;
            bool maxed = up.IsMaxed;
            bool dependencyLocked = IsLocked(up);

            // ---- Wave Lock ----
            bool waveLocked =
                up.requiredWave > 0 &&
                GameManager.Instance.progressionManager.GetCurrentLevel() < up.requiredWave;

            // DEBUG JUST FOR THE CURRENCY UPGRADE
            if (up.id == "currency")
            {
                Debug.Log($"[Currency Upgrade] " +
                          $"level={up.level}, maxLevel={up.maxLevel}, " +
                          $"cost={up.CurrentCost}, totalCurrency={CurrencyManager.Instance.totalCurrency}, " +
                          $"affordable={affordable}, maxed={maxed}, " +
                          $"dependencyLocked={dependencyLocked}, waveLocked={waveLocked}");
            }

            // ---- Button Interactable ----
            up.button.interactable = !maxed && !dependencyLocked && !waveLocked && affordable;

            // ---- Cost Text (ONLY if assigned) ----
            if (up.buttonText != null)
            {
                if (maxed)
                {
                    up.buttonText.text = "MAXED";
                    up.buttonText.color = lockedTextColor;
                }
                else if (dependencyLocked || waveLocked)
                {
                    up.buttonText.text = "LOCKED";
                    up.buttonText.color = lockedTextColor;
                }
                else
                {
                    up.buttonText.text = $"{up.CurrentCost}";
                    up.buttonText.color = affordable ? canAffordColor : cannotAffordColor;
                }
            }

            // ---- Level Sprite ----
            if (up.displayImage != null && up.levelSprites != null && up.levelSprites.Count > 0)
            {
                int index = Mathf.Clamp(up.level, 0, up.levelSprites.Count - 1);
                up.displayImage.sprite = up.levelSprites[index];
            }

            
        }
    }

    public void ResetUpgrades()
    {
        foreach (var up in upgrades)
            up.level = 0;

        PlayerStats.Instance.ResetStats();
        UpdateAllButtons();
    }

    public void ApplyAllUpgradeEffects()
    {
        PlayerStats.Instance.ResetStats();

        foreach (var up in upgrades)
        {
            for (int i = 0; i < up.level; i++)
                ApplyUpgradeEffect(up);
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

    public void UnlockUpgradeById(string id)
    {
        if (map.TryGetValue(id, out var up))
        {
            up.requiresUpgradeId = "";
            up.requiresLevel = 0;
            UpdateAllButtons();
        }
    }
}


/*Last 3 Upgrades
 * Janitor - Throws a mop (Upgrade Box - Face on sticky Note, Gameplay - Face surrounded by black border box, Mop)
 * HR Lady - Yells "Your Fired" (Upgrade Box - Face on sticky Note, Gameplay - Face surrounded by black border box, Text/Speech Bubble Form - Your Fired(Red Color))
 * Drunk Coworker - Throws an empty beer bottle (Upgrade Box - Face on sticky Note, Gameplay - Face surrounded by black border box, Empty Beer Bottle)
 * 
 * Count hits for abilities
 * 
 * Cost text when you hover should be green if you can afford it or red if you can't
 */