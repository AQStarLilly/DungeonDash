using UnityEngine;
using TMPro;

public class HealthSystem : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int attackDamage = 10; // Player fixed, Enemy scales

    [Header("UI")]
    public TMP_Text healthText; // assigned dynamically by GameManager

    public delegate void DeathEvent(HealthSystem hs);
    public event DeathEvent OnDeath;

    private void Start()
    {
        ResetHealth();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log($"[HealthSystem] {name} died firing OnDeath");
            OnDeath?.Invoke(this); // notify GameManager
        }
        UpdateUI();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (healthText != null)
        {
            healthText.text = $"Health: {currentHealth}/{maxHealth}";
        }
    }
}