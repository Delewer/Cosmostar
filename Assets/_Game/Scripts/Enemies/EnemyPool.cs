using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] private EnemyController prefab;
    [SerializeField] private int initialSize = 32;

    private readonly Queue<EnemyController> pool = new();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            EnemyController enemy = CreateInstance();
            Return(enemy);
        }
    }

    public EnemyController Get()
    {
        EnemyController enemy = pool.Count > 0 ? pool.Dequeue() : CreateInstance();
        enemy.gameObject.SetActive(true);
        return enemy;
    }

    public void Return(EnemyController enemy)
    {
        if (enemy == null) return;

        enemy.gameObject.SetActive(false);
        pool.Enqueue(enemy);
    }

    private EnemyController CreateInstance()
    {
        EnemyController enemy = Instantiate(prefab, transform);
        enemy.OnDespawned -= Return;
        enemy.OnDespawned += Return;
        return enemy;
    }
}
