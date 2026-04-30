using System;
using UnityEngine;

public class RunSessionTracker : MonoBehaviour
{
    [SerializeField] private GameLoopManager gameLoopManager;
    [SerializeField] private PlayerController player;
    [SerializeField] private SpawnDirector spawnDirector;

    public int Kills { get; private set; }
    public float ElapsedSeconds { get; private set; }
    public bool IsActive { get; private set; }

    public event Action<int> OnKillCountChanged;
    public event Action<float> OnTimeChanged;

    private void OnEnable()
    {
        if (gameLoopManager != null)
        {
            gameLoopManager.OnRunStarted += HandleRunStarted;
            gameLoopManager.OnRunEnded += HandleRunEnded;
        }

        if (player != null)
        {
            player.OnDied += HandlePlayerDied;
        }

        if (spawnDirector != null)
        {
            spawnDirector.OnEnemySpawned += HandleEnemySpawned;
        }
    }

    private void OnDisable()
    {
        if (gameLoopManager != null)
        {
            gameLoopManager.OnRunStarted -= HandleRunStarted;
            gameLoopManager.OnRunEnded -= HandleRunEnded;
        }

        if (player != null)
        {
            player.OnDied -= HandlePlayerDied;
        }

        if (spawnDirector != null)
        {
            spawnDirector.OnEnemySpawned -= HandleEnemySpawned;
        }
    }

    private void Update()
    {
        if (!IsActive) return;

        ElapsedSeconds += Time.deltaTime;
        OnTimeChanged?.Invoke(ElapsedSeconds);
    }

    private void HandleRunStarted()
    {
        Kills = 0;
        ElapsedSeconds = 0f;
        IsActive = true;
    }

    private void HandleRunEnded(bool victory)
    {
        IsActive = false;
    }

    private void HandlePlayerDied()
    {
        gameLoopManager?.EndRun(false);
    }

    private void HandleEnemySpawned(EnemyController enemy)
    {
        enemy.OnDied -= HandleEnemyDied;
        enemy.OnDied += HandleEnemyDied;
    }

    private void HandleEnemyDied(Vector3 pos, int xp)
    {
        Kills++;
        OnKillCountChanged?.Invoke(Kills);
    }
}
