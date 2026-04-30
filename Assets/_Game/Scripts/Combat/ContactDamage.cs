using UnityEngine;

public class ContactDamage : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float cooldownSeconds = 0.5f;

    private float cooldownRemaining;

    private void Update()
    {
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (cooldownRemaining > 0f) return;
        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;

        damageable.TakeDamage(new DamageContext(damage, DamageType.Kinetic, gameObject));
        cooldownRemaining = cooldownSeconds;
    }
}
