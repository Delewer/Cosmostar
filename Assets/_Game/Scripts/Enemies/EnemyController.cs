using System;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHP = 30f;
    [SerializeField] private float baseMoveSpeed = 2f;
    [SerializeField] private int experienceReward = 1;

    private float currentHP;
    private float currentMoveSpeed;
    private Transform target;

    public bool IsAlive => currentHP > 0f;

    public event Action<Vector3, int> OnDied;
    public event Action<EnemyController> OnDespawned;

    public void Initialize(Transform playerTarget, float hpScale, float speedScale)
    {
        target = playerTarget;
        currentHP = maxHP * hpScale;
        currentMoveSpeed = baseMoveSpeed * speedScale;

        EnemyShooter enemyShooter = GetComponent<EnemyShooter>();
        if (enemyShooter != null)
        {
            enemyShooter.SetTarget(playerTarget);
        }

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!IsAlive || target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * currentMoveSpeed * Time.deltaTime;
    }

    public void TakeDamage(in DamageContext context)
    {
        currentHP -= context.Amount;
        if (currentHP > 0f) return;

        currentHP = 0f;
        OnDied?.Invoke(transform.position, experienceReward);
        Despawn();
    }

    public void Despawn()
    {
        OnDespawned?.Invoke(this);
    }
}
