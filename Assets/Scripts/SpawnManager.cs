using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Setup")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Sprite[] enemySprites;
    public Sprite bossSprite;

    [Header("References")]
    public EnemyManager enemyManager;

    public HealthSystem SpawnEnemy()
    {
        int currentWave = ProgressionManager.Instance.GetCurrentLevel();
        int maxWaves = ProgressionManager.Instance.GetMaxWaves();
        bool isBossWave = (currentWave >= maxWaves);

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        HealthSystem hs = enemy.GetComponent<HealthSystem>();
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            if (isBossWave && bossSprite != null)
            {
                sr.sprite = bossSprite;
            }
            else if (enemySprites.Length > 0)
            {
                sr.sprite = enemySprites[Random.Range(0, enemySprites.Length)];
            }
        }             

        int extraHealth = ProgressionManager.Instance.GetEnemyHealthBonus();
        int extraDamage = ProgressionManager.Instance.GetEnemyDamageBonus();

        hs.maxHealth += extraHealth;
        hs.currentHealth = hs.maxHealth;
        hs.attackDamage += extraDamage;

        hs.InitializeEnemy();

        if (enemyManager != null)
        {
            enemyManager.RegisterEnemy(hs);
        }

        Debug.Log($"Spawned Enemy -> HP: {hs.maxHealth}, DMG: {hs.attackDamage}");
        return hs;
    }
}