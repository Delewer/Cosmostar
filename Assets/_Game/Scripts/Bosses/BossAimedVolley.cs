using UnityEngine;

public class BossAimedVolley : MonoBehaviour
{
    [SerializeField] private ProjectileEmitter projectileEmitter;
    [SerializeField] private Transform target;
    [SerializeField] private int shotsPerVolley = 5;
    [SerializeField] private float spreadAngle = 22f;
    [SerializeField] private float volleyInterval = 3f;
    [SerializeField] private float projectileDamage = 12f;

    private float timer;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Update()
    {
        if (projectileEmitter == null || target == null || shotsPerVolley <= 0) return;

        timer += Time.deltaTime;
        if (timer < volleyInterval) return;

        timer = 0f;
        FireVolleyAtTarget();
    }

    private void FireVolleyAtTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        int clampedShots = Mathf.Max(1, shotsPerVolley);
        float startAngle = baseAngle - (spreadAngle * 0.5f);
        float step = clampedShots == 1 ? 0f : spreadAngle / (clampedShots - 1);

        for (int i = 0; i < clampedShots; i++)
        {
            float angle = startAngle + (step * i);
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
            projectileEmitter.Emit(transform.position, rotation, projectileDamage);
        }
    }
}
