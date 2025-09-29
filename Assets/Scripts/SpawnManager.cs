using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Setup")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;

    [Header("References")]
    public EnemyManager enemyManager;

    public HealthSystem SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        HealthSystem hs = enemy.GetComponent<HealthSystem>();

        int extraHealth = ProgressionManager.Instance.GetEnemyHealthBonus();
        int extraDamage = ProgressionManager.Instance.GetEnemyDamageBonus();

        hs.maxHealth += extraHealth;
        hs.currentHealth = hs.maxHealth;
        hs.attackDamage += extraDamage;

        hs.ResetHealth();

        if (enemyManager != null)
        {
            enemyManager.RegisterEnemy(hs);
        }

        Debug.Log($"Spawned Enemy -> HP: {hs.maxHealth}, DMG: {hs.attackDamage}");
        return hs;
    }
}