using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private Projectile prefab;
    [SerializeField] private int initialSize = 64;

    private readonly Queue<Projectile> pool = new();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            Projectile projectile = CreateInstance();
            Return(projectile);
        }
    }

    public Projectile Get()
    {
        Projectile projectile = pool.Count > 0 ? pool.Dequeue() : CreateInstance();
        projectile.gameObject.SetActive(true);
        return projectile;
    }

    public void Return(Projectile projectile)
    {
        if (projectile == null) return;

        projectile.gameObject.SetActive(false);
        pool.Enqueue(projectile);
    }

    private Projectile CreateInstance()
    {
        Projectile projectile = Instantiate(prefab, transform);
        projectile.OnExpired -= Return;
        projectile.OnExpired += Return;
        return projectile;
    }
}
