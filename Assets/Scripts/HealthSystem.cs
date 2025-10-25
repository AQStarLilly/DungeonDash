using UnityEngine;
using TMPro;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int maxShield = 0;
    public int currentShield = 0;

    [Header("Attack Stats")]
    public int attackDamage = 10;
    [Range(0f, 1f)] public float critChance = 0.1f;
    public float critMultiplier = 2f;

    [Header("Shield Settings")]
    public bool regenerateShieldOnReset = true;

    [Header("Shield UI (World-Space)")]
    public TMP_Text shieldValueText;

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
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        // Don’t reset health automatically — GameManager handles initialization
        if (healthBarUI == null)
        {
            var bars = Object.FindObjectsByType<HealthBarUI>(FindObjectsSortMode.None);
            foreach (var bar in bars)
            {
                string lower = bar.name.ToLower();
                if (isPlayer && lower.Contains("player")) { healthBarUI = bar; break; }
                if (!isPlayer && lower.Contains("enemy")) { healthBarUI = bar; break; }
            }
        }

        UpdateUI();
    }

    // Initialize values from PlayerStats when the run starts
    public void InitializeFromPlayerStats(bool firstSpawnOfRun)
    {
        if (isPlayer && PlayerStats.Instance != null)
        {
            maxHealth = Mathf.RoundToInt(maxHealth * PlayerStats.Instance.healthMultiplier);
            currentHealth = maxHealth;

            maxShield = PlayerStats.Instance.shield;

            if (firstSpawnOfRun)
                currentShield = maxShield; // start with full shield
            else
                currentShield = Mathf.Min(currentShield, maxShield);

            regenerateShieldOnReset = false; // don’t regen each wave
        }
        else
        {
            currentHealth = maxHealth;
            currentShield = 0;
            regenerateShieldOnReset = true;
        }

        RefreshShieldVisual();
        UpdateUI();
    }

    //Reset for next wave (health only)
    public void ResetForNextWave()
    {
        currentHealth = maxHealth;

        if (regenerateShieldOnReset)
            currentShield = maxShield;
        else
            currentShield = Mathf.Min(currentShield, maxShield);

        RefreshShieldVisual();
        UpdateUI();
    }

    public void TakeDamage(int damage, bool isCrit = false)
    {
        int remaining = damage;

        // Apply shield first
        if (currentShield > 0)
        {
            int shieldHit = Mathf.Min(currentShield, remaining);
            currentShield -= shieldHit;
            remaining -= shieldHit;

            if (shieldVisual != null)
            {
                shieldVisual.UpdateShieldVisual(currentShield, maxShield);
                shieldVisual.FlashHit();
            }

            if (floatingTextPrefab != null)
            {
                Canvas mainCanvas = GameObject.FindAnyObjectByType<Canvas>();
                Vector3 worldPos = transform.position + new Vector3(0f, 3.5f, 0f);
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                var popup = Instantiate(floatingTextPrefab, screenPos, Quaternion.identity, mainCanvas.transform);
                popup.GetComponent<FloatingDamageText>().Initialize(shieldHit, false, true);
            }
        }

        // Remaining damage applies to health
        if (remaining > 0)
        {
            currentHealth -= remaining;

            if (spriteRenderer != null)
                StartCoroutine(FlashRed());
                StartCoroutine(ShakeOnHit());

            if (floatingTextPrefab != null)
            {
                Canvas mainCanvas = GameObject.FindAnyObjectByType<Canvas>();
                Vector3 worldPos = transform.position + new Vector3(0f, 3.5f, 0f);
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                var popup = Instantiate(floatingTextPrefab, screenPos, Quaternion.identity, mainCanvas.transform);
                popup.GetComponent<FloatingDamageText>().Initialize(remaining, isCrit);
            }

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                OnDeath?.Invoke(this);
            }
        }

        RefreshShieldVisual();
        UpdateUI();
    }

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
        while(elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    private void RefreshShieldVisual()
    {
        if (shieldVisual == null) return;

        if (maxShield > 0 && currentShield > 0)
        {
            shieldVisual.ShowShield();
            shieldVisual.UpdateShieldVisual(currentShield, maxShield);
        }
        else
        {
            shieldVisual.HideShield();
        }
    }

    public void UpdateUI()
    {
        if (healthText != null)
        {
            if (currentShield > 0)
                healthText.text = $"HP: {currentHealth}/{maxHealth} | Shield: {currentShield}/{maxShield}";
            else
                healthText.text = $"HP: {currentHealth}/{maxHealth}";
        }

        if (healthBarUI != null)
            healthBarUI.SetHealth(currentHealth, maxHealth);
    }

    public (int damage, bool isCrit) CalculateAttackDamage()
    {
        int dmg = attackDamage;
        bool crit = false;

        if (isPlayer && PlayerStats.Instance != null)
            dmg = Mathf.RoundToInt(dmg * PlayerStats.Instance.damageMultiplier);

        if (Random.value <= critChance)
        {
            dmg = Mathf.RoundToInt(dmg * critMultiplier);
            crit = true;
        }

        return (dmg, crit);
    }

    public void InitializeEnemy()
    {
        currentHealth = maxHealth;
        currentShield = 0;

        if (shieldVisual != null)
            shieldVisual.HideShield();

        UpdateUI();
    }
}
