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
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        // Automatically find and link the correct health bar if none is assigned
        if (healthBarUI == null)
        {        
            var bars = Object.FindObjectsByType<HealthBarUI>(FindObjectsSortMode.None);

            if (isPlayer)
            {
                // Assign the one with "Player" in its name
                foreach (var bar in bars)
                {
                    if (bar.name.ToLower().Contains("player"))
                    {
                        healthBarUI = bar;
                        break;
                    }
                }
            }
            else
            {
                // Assign the one with "Enemy" in its name
                foreach (var bar in bars)
                {
                    if (bar.name.ToLower().Contains("enemy"))
                    {
                        healthBarUI = bar;
                        break;
                    }
                }
            }
        }
        ResetHealth();
    }

    public void TakeDamage(int damage, bool isCrit = false)
    {
        int damageRemaining = damage;

        // Apply to Shield first
        if (currentShield > 0)
        {
            int shieldDamage = Mathf.Min(currentShield, damageRemaining);
            currentShield -= shieldDamage;
            damageRemaining -= shieldDamage;

            if (shieldVisual != null)
            {
                shieldVisual.UpdateShieldVisual(currentShield, maxShield);
                shieldVisual.FlashHit();
            }

            // Show shield damage popup
            if (floatingTextPrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 1.2f;
                var popup = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
                popup.GetComponent<FloatingDamageText>().Initialize(shieldDamage, false); 
            }
        }

        // Apply remaining to Health
        if (damageRemaining > 0)
        {
            currentHealth -= damageRemaining;

            if (spriteRenderer != null)
                StartCoroutine(FlashRed());

            if (floatingTextPrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 1.2f;
                var popup = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
                popup.GetComponent<FloatingDamageText>().Initialize(damageRemaining, isCrit);
            }

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                OnDeath?.Invoke(this);
            }
        }

        UpdateUI();
    }    

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

    public void ResetHealth()
    {
        if(isPlayer && PlayerStats.Instance != null)
        {
            maxHealth = Mathf.RoundToInt(maxHealth * PlayerStats.Instance.healthMultiplier);
            maxShield = PlayerStats.Instance.shield;
        }

        currentHealth = maxHealth;
        currentShield = maxShield;

        UpdateUI();

        if (shieldVisual != null)
        {
            if (maxShield > 0)
            {
                shieldVisual.ShowShield();
                shieldVisual.UpdateShieldVisual(currentShield, maxShield);
            }
            else
            {
                shieldVisual.HideShield();
            }
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
        {
            healthBarUI.SetHealth(currentHealth, maxHealth);
        }
    }

    public (int damage, bool isCrit) CalculateAttackDamage()
    {
        int finalDamage = attackDamage;
        bool isCrit = false;

        if (isPlayer && PlayerStats.Instance != null)
        {
            finalDamage = Mathf.RoundToInt(finalDamage * PlayerStats.Instance.damageMultiplier);
        }

        if (Random.value <= critChance)
        {
            finalDamage = Mathf.RoundToInt(finalDamage * critMultiplier);
            isCrit = true;
        }

        return (finalDamage, isCrit);
    }
}