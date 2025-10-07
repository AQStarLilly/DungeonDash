using UnityEngine;
using TMPro;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int attackDamage = 10;

    [Header("UI")]
    public TMP_Text healthText;

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
    }

    public int GetAttackDamage()
    {
        if (isPlayer && PlayerStats.Instance != null)
        {
            return Mathf.RoundToInt(attackDamage * PlayerStats.Instance.damageMultiplier);
        }
        return attackDamage;
    }
}