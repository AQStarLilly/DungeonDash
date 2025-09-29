using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance;

    private int level = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void IncreaseLevel()
    {
        level++;
    }

    public int GetEnemyHealthBonus()
    {
        return level * 10; // Example scaling
    }

    public int GetEnemyDamageBonus()
    {
        return level * 2;
    }

    public int GetCurrentLevel()
    {
        return level;
    }
}
