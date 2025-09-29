using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public SpawnManager spawnManager;

    public void RegisterEnemy(HealthSystem enemy)
    {
        Debug.Log($"Enemy Registered: {enemy.name}");
    }

}