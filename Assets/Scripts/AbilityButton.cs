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

    private void Awake()
    {
        if (button  == null) 
            button = GetComponent<Button>();

        button.onClick.AddListener(UseAbility);
    }

    private void Start()
    {
        if (cooldownText != null)
            cooldownText.text = "";

        onCooldown = false;
        if (button != null)
            button.interactable = true;
    }

    public void Initialize(UpgradeManager.Upgrade up)
    {
        gameObject.SetActive(true);

        upgrade = up;
        cooldown = up.cooldown;
        damage = up.abilityDamage;

        if (up.abilityIcon != null)
            icon.sprite = up.abilityIcon;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // reset cooldown visuals when we start a new run
        onCooldown = false;
        if (cooldownText != null)
            cooldownText.text = "";
        if (button != null)
            button.interactable = true;
    }

    private void UseAbility()
    {
        if (onCooldown) return;
        if (gm.currentEnemy == null) return;

        AbilityEffectController.Instance.PlayAbilityEffect(upgradeId,
        gm.playerHealth.transform, gm.currentEnemy.transform);

        // Deal damage
        gm.currentEnemy.TakeDamage(damage, false);

        // Start cooldown
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        onCooldown = true;
        button.interactable = false;
        if (icon != null)
            icon.color = new Color(1f, 1f, 1f, 0.35f);

        float timer = cooldown;

        while (timer > 0f)
        {
            if (cooldownText != null)
                cooldownText.text = Mathf.CeilToInt(timer).ToString();

            timer -= Time.deltaTime;
            yield return null;
        }

        if (cooldownText != null)
            cooldownText.text = "";

        if (icon != null)
            icon.color = Color.white;

        button.interactable = true;
        onCooldown = false;
    }
}
