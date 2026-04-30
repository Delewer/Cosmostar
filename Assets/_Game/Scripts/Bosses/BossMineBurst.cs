using UnityEngine;

public class BossMineBurst : MonoBehaviour
{
    [SerializeField] private ProjectileEmitter projectileEmitter;
    [SerializeField] private int mineCount = 6;
    [SerializeField] private float spawnRadius = 4f;
    [SerializeField] private float burstInterval = 4.5f;
    [SerializeField] private float detonationDelay = 1.1f;
    [SerializeField] private float projectileDamage = 18f;

    private float timer;

    private void Update()
    {
        if (projectileEmitter == null || mineCount <= 0) return;

        timer += Time.deltaTime;
        if (timer < burstInterval) return;

        timer = 0f;
        SpawnMineBurst();
    }

    private void SpawnMineBurst()
    {
        for (int i = 0; i < mineCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 minePosition = transform.position + new Vector3(offset.x, offset.y, 0f);
            StartCoroutine(DetonateMineAfterDelay(minePosition));
        }
    }

    private System.Collections.IEnumerator DetonateMineAfterDelay(Vector3 position)
    {
        yield return new WaitForSeconds(detonationDelay);

        const int ringProjectiles = 8;
        float step = 360f / ringProjectiles;
        for (int i = 0; i < ringProjectiles; i++)
        {
            Quaternion rotation = Quaternion.Euler(0f, 0f, step * i);
            projectileEmitter.Emit(position, rotation, projectileDamage);
        }
    }
}
