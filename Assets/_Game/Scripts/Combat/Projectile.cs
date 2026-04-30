using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float maxLifeSeconds = 3f;

    private float speed;
    private float damage;
    private float lifeRemaining;
    private bool active;

    public event Action<Projectile> OnExpired;

    public void Initialize(float speedValue, float damageValue)
    {
        speed = speedValue;
        damage = damageValue;
        lifeRemaining = maxLifeSeconds;
        active = true;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!active) return;

        transform.position += transform.up * speed * Time.deltaTime;
        lifeRemaining -= Time.deltaTime;
        if (lifeRemaining <= 0f)
        {
            Expire();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!active) return;
        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;

        damageable.TakeDamage(new DamageContext(damage, DamageType.Energy, gameObject));
        Expire();
    }

    private void Expire()
    {
        if (!active) return;
        active = false;
        OnExpired?.Invoke(this);
    }
}
