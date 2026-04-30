using System.Collections;
using UnityEngine;

public class EnemyTelegraphWarningSystem : MonoBehaviour
{
    [SerializeField] private SpawnDirector spawnDirector;
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private float warningLifetime = 0.4f;

    private void OnEnable()
    {
        if (spawnDirector != null)
        {
            spawnDirector.OnEnemySpawned += HandleEnemySpawned;
        }
    }

    private void OnDisable()
    {
        if (spawnDirector != null)
        {
            spawnDirector.OnEnemySpawned -= HandleEnemySpawned;
        }
    }

    private void HandleEnemySpawned(EnemyController enemy)
    {
        if (enemy == null) return;

        EnemyShooter shooter = enemy.GetComponent<EnemyShooter>();
        if (shooter == null) return;

        shooter.OnTelegraphStarted -= HandleTelegraphStarted;
        shooter.OnTelegraphStarted += HandleTelegraphStarted;
    }

    private void HandleTelegraphStarted(Vector3 targetPosition)
    {
        ServiceLocator.Instance?.AnalyticsService?.Track("enemy_telegraph_started");

        if (warningPrefab == null) return;
        StartCoroutine(SpawnWarningIndicator(targetPosition));
    }

    private IEnumerator SpawnWarningIndicator(Vector3 worldPosition)
    {
        GameObject warning = Instantiate(warningPrefab, worldPosition, Quaternion.identity);
        yield return new WaitForSeconds(warningLifetime);
        if (warning != null)
        {
            Destroy(warning);
        }
    }
}
