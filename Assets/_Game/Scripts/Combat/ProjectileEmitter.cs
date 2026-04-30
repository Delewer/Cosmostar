using UnityEngine;

public class ProjectileEmitter : MonoBehaviour
{
    [SerializeField] private ProjectilePool pool;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float projectileSpeed = 12f;

    public void Emit(float damage)
    {
        if (muzzle == null) return;
        Emit(muzzle.position, muzzle.rotation, damage);
    }

    public void Emit(Vector3 position, Quaternion rotation, float damage)
    {
        if (pool == null) return;

        Projectile projectile = pool.Get();
        projectile.transform.SetPositionAndRotation(position, rotation);
        projectile.Initialize(projectileSpeed, damage);
    }
}
