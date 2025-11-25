using UnityEngine;
using TMPro;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Attack Stats")]
    public int attackDamage = 10;
    [Range(0f, 1f)] public float critChance = 0.1f;
    public float critMultiplier = 2f;

    [Header("UI")]
    public TMP_Text healthText;
    public HealthBarUI healthBarUI;

    [Header("Visuals")]
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public GameObject floatingTextPrefab;
    public ShieldVisual shieldVisual;

    [Header("Identity")]
    public bool isPlayer = false;

    public delegate void DeathEvent(HealthSystem hs);
    public event DeathEvent OnDeath;

    private void Awake()
    {
        if (!TryGetComponent(out spriteRenderer))
        {
            Debug.LogWarning($"{name} has no SpriteRenderer. FlashRed() will be skipped.");
        }
        else
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        // If UI reference isn't assigned, attempt auto-match
        AutoAssignHealthBar();
        UpdateUI();
    }

    /// <summary>
    /// Initialize health using PlayerStats (run start).
    /// </summary>
    public void InitializeFromPlayerStats(bool firstSpawnOfRun)
    {
        if (isPlayer && PlayerStats.Instance != null)
        {
            maxHealth = Mathf.RoundToInt(maxHealth * PlayerStats.Instance.healthMultiplier);
            currentHealth = maxHealth;

            if (PlayerStats.Instance.damageReduction > 0 && shieldVisual != null)
                shieldVisual.ShowShield();
        }
        else
        {
            currentHealth = maxHealth;
            shieldVisual?.HideShield();
        }

        UpdateUI();
    }

    /// <summary>
    /// Used by enemy waves.
    /// </summary>
    public void ResetForNextWave()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    /// <summary>
    /// Pure enemy initialization if needed.
    /// </summary>
    public void InitializeEnemy()
    {
        currentHealth = maxHealth;
        shieldVisual?.HideShield();
        UpdateUI();
    }

    // -------------------------------
    //          DAMAGE
    // -------------------------------

    public void TakeDamage(int damage, bool isCrit = false)
    {
        int finalDamage = damage;

        // Apply PlayerStats damage reduction
        if (isPlayer && PlayerStats.Instance != null)
        {
            float reduction = Mathf.Clamp01(PlayerStats.Instance.damageReduction);
            finalDamage = Mathf.RoundToInt(finalDamage * (1f - reduction));
        }

        currentHealth -= finalDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (spriteRenderer != null)
            StartCoroutine(FlashRed());

        StartCoroutine(ShakeOnHit());

        SpawnFloatingText(finalDamage, isCrit);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath?.Invoke(this);
        }

        UpdateUI();
    }

    // -------------------------------
    //        VISUAL FEEDBACK
    // -------------------------------

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private IEnumerator ShakeOnHit(float duration = 0.15f, float magnitude = 0.1f)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    private void SpawnFloatingText(int damage, bool isCrit)
    {
        if (floatingTextPrefab == null)
            return;

        Canvas mainCanvas = GameObject.FindAnyObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogWarning("No Canvas found for floating text!");
            return;
        }

        Vector3 worldPos = transform.position + new Vector3(0f, 3.5f, 0f);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        GameObject obj = Instantiate(floatingTextPrefab, screenPos, Quaternion.identity, mainCanvas.transform);

        if (obj.TryGetComponent(out FloatingDamageText fdt))
            fdt.Initialize(damage, isCrit);
        else
            Debug.LogWarning("FloatingTextPrefab is missing FloatingDamageText component!");
    }

    // -------------------------------
    //        DAMAGE CALCULATION
    // -------------------------------

    public (int damage, bool isCrit) CalculateAttackDamage()
    {
        int dmg = attackDamage;

        if (isPlayer && PlayerStats.Instance != null)
            dmg = Mathf.RoundToInt(dmg * PlayerStats.Instance.damageMultiplier);

        int final = DamageCalculator.CalculateCritDamage(dmg, critChance, critMultiplier, out bool crit);

        return (final, crit);
    }

    // -------------------------------
    //             UI
    // -------------------------------

    public void UpdateUI()
    {
        if (healthText != null)
            healthText.text = $"HP: {currentHealth}/{maxHealth}";

        if (healthBarUI != null)
            healthBarUI.SetHealth(currentHealth, maxHealth);
    }

    private void AutoAssignHealthBar()
    {
        if (healthBarUI != null)
            return;

        var bars = Object.FindObjectsByType<HealthBarUI>(FindObjectsSortMode.None);
        foreach (var bar in bars)
        {
            string lower = bar.name.ToLower();
            if (isPlayer && lower.Contains("player")) { healthBarUI = bar; break; }
            if (!isPlayer && lower.Contains("enemy")) { healthBarUI = bar; break; }
        }
    }


    // ------------------------------------------------------------------
    //                      STATIC DAMAGE CALCULATOR
    // ------------------------------------------------------------------

    /// <summary>
    /// Static utility class to demonstrate static OOP usage and separation of concerns.
    /// </summary>
    public static class DamageCalculator
    {
        public static int CalculateCritDamage(int baseDamage, float critChance, float critMultiplier, out bool isCrit)
        {
            isCrit = Random.value <= critChance;
            return isCrit ? Mathf.RoundToInt(baseDamage * critMultiplier) : baseDamage;
        }
    }
}


