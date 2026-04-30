using System;
using UnityEngine;

public class SpawnDirector : MonoBehaviour
{
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private ExperiencePickupPool experiencePickupPool;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float spawnRadius = 10f;

    private float elapsed;
    private float timer;

    public float ElapsedSeconds => elapsed;

    public event Action<EnemyController> OnEnemySpawned;

    private void Update()
    {
        if (enemyPool == null || player == null) return;

        elapsed += Time.deltaTime;
        timer += Time.deltaTime;

        if (timer < spawnInterval) return;

        timer = 0f;
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        float hpScale = 1f + elapsed * 0.03f;
        float speedScale = 1f + elapsed * 0.01f;

        Vector2 offset = Random.insideUnitCircle;
        if (offset.sqrMagnitude < 0.001f) offset = Vector2.up;
        Vector2 spawnPos = (Vector2)player.position + offset.normalized * spawnRadius;

        EnemyController enemy = enemyPool.Get();
        enemy.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
        enemy.Initialize(player, hpScale, speedScale);
        enemy.OnDied -= HandleEnemyDied;
        enemy.OnDied += HandleEnemyDied;
        OnEnemySpawned?.Invoke(enemy);
    }

    private void HandleEnemyDied(Vector3 position, int experienceReward)
    {
        if (experiencePickupPool == null) return;

        ExperiencePickup pickup = experiencePickupPool.Get();
        pickup.transform.SetPositionAndRotation(position, Quaternion.identity);
        pickup.Initialize(experienceReward);
    }
}
