using UnityEngine;
using TMPro;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int attackDamage = 10;
    [Range(0f, 1f)] public float critChance = 0.1f;
    public float critMultiplier = 2f;

    [Header("UI")]
    public TMP_Text healthText;
    public HealthBarUI healthBarUI;

    [Header("Visuals")]
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (spriteRenderer != null)
            StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath?.Invoke(this);
        }

        UpdateUI();
    }

    public int CalculateAttackDamage()
    {
        int finalDamage = attackDamage;

        if (isPlayer && PlayerStats.Instance != null)
        {
            return Mathf.RoundToInt(finalDamage * PlayerStats.Instance.damageMultiplier);
        }

        if (Random.value <= critChance)
        {
            finalDamage = Mathf.RoundToInt(finalDamage * critMultiplier);
            Debug.Log($"{gameObject.name} landed a CRITICAL HIT for {finalDamage} damage!");
        }

        return finalDamage;
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
            maxHealth = Mathf.RoundToInt(maxHealth * PlayerStats.Instance.healthMultiplier) + PlayerStats.Instance.shield;
        }

        currentHealth = maxHealth;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (healthText != null)
            healthText.text = $"Health: {currentHealth}/{maxHealth}";

        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(currentHealth, maxHealth);
        }
    }
}