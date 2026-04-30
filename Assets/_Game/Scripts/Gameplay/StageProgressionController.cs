using System;
using System.Collections.Generic;
using UnityEngine;

public class StageProgressionController : MonoBehaviour
{
    [SerializeField] private GameLoopManager gameLoopManager;
    [SerializeField] private SpawnDirector spawnDirector;
    [SerializeField] private Transform player;
    [SerializeField] private BossController bossPrefab;
    [SerializeField] private float bossSpawnTimeSeconds = 120f;
    [SerializeField] private Vector2 bossSpawnOffset = new(0f, 9f);

    private bool bossSpawned;
    private bool victoryHandled;

    public event Action<BossController> OnBossSpawned;

    private void Update()
    {
        if (gameLoopManager == null || spawnDirector == null || player == null || bossPrefab == null) return;
        if (gameLoopManager.CurrentState != RunState.Running) return;
        if (bossSpawned) return;

        if (spawnDirector.ElapsedSeconds < bossSpawnTimeSeconds) return;
        SpawnBoss();
    }

    private void SpawnBoss()
    {
        bossSpawned = true;
        gameLoopManager.TriggerBossWarning(1.25f);

        Vector3 spawnPos = player.position + (Vector3)bossSpawnOffset;
        BossController boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        boss.Initialize(player, hpScale: 1.0f);
        boss.OnBossDefeated += HandleBossDefeated;
        OnBossSpawned?.Invoke(boss);

        ServiceLocator.Instance?.AnalyticsService?.Track("boss_spawned", new Dictionary<string, object>
        {
            ["spawn_time"] = Mathf.RoundToInt(spawnDirector.ElapsedSeconds)
        });
    }

    private void HandleBossDefeated()
    {
        if (victoryHandled) return;
        victoryHandled = true;
        gameLoopManager.EndRun(true);
        ServiceLocator.Instance?.AnalyticsService?.Track("boss_defeated");
    }
}
