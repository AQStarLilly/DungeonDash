using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public SpawnManager spawnManager;

    public void RegisterEnemy(HealthSystem enemy)
    {
        enemy.OnDeath += HandleEnemyDeath;
    }

    private void HandleEnemyDeath(HealthSystem enemy)
    {
        // Reward currency
        CurrencyManager.Instance.AddCurrency(10);

        // Spawn a new enemy
        spawnManager.SpawnEnemy();
    }
}