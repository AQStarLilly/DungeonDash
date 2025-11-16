using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AbilityButton : MonoBehaviour
{
    public string upgradeId;
    public Image icon;
    public TMP_Text cooldownText;
    public Button button;

    private float cooldown;
    private int damage;
    private bool onCooldown = false;

    private UpgradeManager.Upgrade upgrade;
    private GameManager gm => GameManager.Instance;

    private void Start()
    {
        button.onClick.AddListener(UseAbility);
        cooldownText.text = "";
        gameObject.SetActive(false); // hidden until unlocked
    }

    public void Initialize(UpgradeManager.Upgrade up)
    {
        upgrade = up;
        cooldown = up.cooldown;
        damage = up.abilityDamage;

        if (up.abilityIcon != null)
            icon.sprite = up.abilityIcon;

        gameObject.SetActive(true);
    }

    private void UseAbility()
    {
        if (onCooldown) return;
        if (gm.currentEnemy == null) return;

        // Deal damage
        gm.currentEnemy.TakeDamage(damage, false);

        // Start cooldown
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        onCooldown = true;
        button.interactable = false;

        float timer = cooldown;

        while (timer > 0)
        {
            cooldownText.text = Mathf.CeilToInt(timer).ToString();
            timer -= Time.deltaTime;
            yield return null;
        }

        cooldownText.text = "";
        button.interactable = true;
        onCooldown = false;
    }
}
