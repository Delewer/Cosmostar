using UnityEngine;

public class BossRadialAttack : MonoBehaviour
{
    [SerializeField] private ProjectileEmitter projectileEmitter;
    [SerializeField] private int projectileCount = 12;
    [SerializeField] private float attackInterval = 2.5f;
    [SerializeField] private float projectileDamage = 15f;
    [SerializeField] private float rotationPerBurst = 8f;

    private float timer;
    private float burstAngleOffset;

    private void Update()
    {
        if (projectileEmitter == null || projectileCount <= 0) return;

        timer += Time.deltaTime;
        if (timer < attackInterval) return;

        timer = 0f;
        FireRadialBurst();
    }

    private void FireRadialBurst()
    {
        float step = 360f / projectileCount;
        Vector3 origin = transform.position;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = burstAngleOffset + (step * i);
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
            projectileEmitter.Emit(origin, rotation, projectileDamage);
        }

        burstAngleOffset = (burstAngleOffset + rotationPerBurst) % 360f;
    }
}
