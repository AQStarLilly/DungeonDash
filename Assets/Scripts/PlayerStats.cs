using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public float damageMultiplier = 1f;
    public float healthMultiplier = 1f;
    public int shield = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ResetStats()
    {
        damageMultiplier = 1f;
        healthMultiplier = 1f;
        shield = 0;
    }
}
