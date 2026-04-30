using System;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private ProjectileEmitter projectileEmitter;
    [SerializeField] private float shotInterval = 1.6f;
    [SerializeField] private float telegraphDuration = 0.35f;
    [SerializeField] private int shotsPerBurst = 1;
    [SerializeField] private float burstSpreadAngle = 0f;
    [SerializeField] private float projectileDamage = 8f;

    private Transform target;
    private float timer;
    private float telegraphRemaining;
    private bool pendingShot;

    public event Action<Vector3> OnTelegraphStarted;

    public void SetTarget(Transform playerTarget)
    {
        target = playerTarget;
    }

    private void Update()
    {
        if (projectileEmitter == null || target == null) return;

        if (pendingShot)
        {
            telegraphRemaining -= Time.deltaTime;
            if (telegraphRemaining <= 0f)
            {
                pendingShot = false;
                FireAtTarget();
            }
            return;
        }

        timer += Time.deltaTime;
        if (timer < shotInterval) return;

        timer = 0f;
        pendingShot = true;
        telegraphRemaining = Mathf.Max(0f, telegraphDuration);
        OnTelegraphStarted?.Invoke(target.position);

        if (telegraphRemaining <= 0f)
        {
            pendingShot = false;
            FireAtTarget();
        }
    }

    private void FireAtTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        int shotCount = Mathf.Max(1, shotsPerBurst);
        float startAngle = baseAngle - (burstSpreadAngle * 0.5f);
        float step = shotCount == 1 ? 0f : burstSpreadAngle / (shotCount - 1);

        for (int i = 0; i < shotCount; i++)
        {
            float shotAngle = startAngle + (step * i);
            projectileEmitter.Emit(transform.position, Quaternion.Euler(0f, 0f, shotAngle), projectileDamage);
        }
    }
}
