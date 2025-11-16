using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance;

    private int level = 0; // start at 0 so first spawn increments to 1
    public int maxWaves = 30;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ResetLevel() //called when you start a new run
    {
        level = 1;
    }

    public void IncreaseLevel()
    {
        if (level < maxWaves)
        {
            level++;
        }

        if(level == 5)
        {
            UpgradeManager.Instance.UnlockUpgradeById("janitor");
        }
        if (level == 10)
        {
            UpgradeManager.Instance.UnlockUpgradeById("hrlady");
        }
        if (level == 15)
        {
            UpgradeManager.Instance.UnlockUpgradeById("drunkCoworker");
        }
    }

    public int GetEnemyHealthBonus()
    {
        return (level - 1) * 10; // +10 HP per level
    }

    public int GetEnemyDamageBonus()
    {
        return (level - 1) * 2;  // +2 damage per level
    }

    public int GetCurrentLevel()
    {
        return level;
    }

    public int GetMaxWaves()
    {
        return maxWaves;
    }

    public void SetCurrentLevel(int wave)
    {
        level = Mathf.Clamp(wave, 1, maxWaves);
    }
}
