using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public EnemyManager enemyManager;

    public void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        HealthSystem hs = enemy.GetComponent<HealthSystem>();
        enemyManager.RegisterEnemy(hs);

        // Scale stats with progression
        int extraHealth = ProgressionManager.Instance.GetEnemyHealthBonus();
        int extraDamage = ProgressionManager.Instance.GetEnemyDamageBonus();

        hs.maxHealth += extraHealth;
        hs.currentHealth = hs.maxHealth;
    }
}