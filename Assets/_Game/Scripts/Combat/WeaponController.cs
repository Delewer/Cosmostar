using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private ProjectileEmitter emitter;
    [SerializeField] private float fireRate = 4f;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private bool autoFireEnabled = true;

    private float nextFireTime;

    private void Update()
    {
        if (!autoFireEnabled || emitter == null) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
        emitter.Emit(baseDamage);
    }

    public void RebuildFromStats(PlayerStats stats)
    {
        if (stats == null) return;

        fireRate = Mathf.Max(0.01f, stats.FireRate);
        baseDamage = Mathf.Max(0f, stats.BaseDamage);
    }

    public void SetAutoFire(bool enabled)
    {
        autoFireEnabled = enabled;
    }
}
