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

            // Show shield visual if you have damage reduction
            if (PlayerStats.Instance.damageReduction > 0f && shieldVisual != null)
                shieldVisual.ShowShield();
        }
        else
        {
            currentHealth = maxHealth;
            if (shieldVisual != null)
                shieldVisual.HideShield();
        }

        UpdateUI();
    }

    //Reset for next wave (health only)
    public void ResetForNextWave()
    {
        currentHealth = maxHealth;   

        UpdateUI();
    }

    public void TakeDamage(int damage, bool isCrit = false)
    {
        int finalDamage = damage;

        // --- Apply player’s damage reduction ---
        if (isPlayer && PlayerStats.Instance != null && PlayerStats.Instance.damageReduction > 0f)
        {
            float reduction = PlayerStats.Instance.damageReduction;
            finalDamage = Mathf.RoundToInt(finalDamage * (1f - reduction));
        }

        currentHealth -= finalDamage;

        if (spriteRenderer != null)
            StartCoroutine(FlashRed());
        StartCoroutine(ShakeOnHit());

        // --- Floating text (normal red/black) ---
        if (floatingTextPrefab != null)
        {
            Canvas mainCanvas = GameObject.FindAnyObjectByType<Canvas>();
            Vector3 worldPos = transform.position + new Vector3(0f, 3.5f, 0f);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            var popup = Instantiate(floatingTextPrefab, screenPos, Quaternion.identity, mainCanvas.transform);
            popup.GetComponent<FloatingDamageText>().Initialize(finalDamage, isCrit);
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath?.Invoke(this);
        }

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

    public void UpdateUI()
    {
        if (healthText != null)
            healthText.text = $"HP: {currentHealth}/{maxHealth}";

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

        if (shieldVisual != null)
            shieldVisual.HideShield();

        UpdateUI();
    }
}

