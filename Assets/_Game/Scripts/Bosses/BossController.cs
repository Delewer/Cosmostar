using System;
using UnityEngine;

public class BossController : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHP = 2000f;
    [SerializeField] private float moveSpeed = 1.5f;

    private float currentHP;
    private Transform target;

    public bool IsAlive => currentHP > 0f;
    public float HealthNormalized => maxHP <= 0f ? 0f : Mathf.Clamp01(currentHP / maxHP);

    public event Action OnBossDefeated;
    public event Action<float> OnBossHealthChanged;

    public void Initialize(Transform playerTarget, float hpScale)
    {
        target = playerTarget;
        currentHP = maxHP * Mathf.Max(0.1f, hpScale);
        BossAimedVolley aimedVolley = GetComponent<BossAimedVolley>();
        if (aimedVolley != null)
        {
            aimedVolley.SetTarget(playerTarget);
        }
        gameObject.SetActive(true);
        OnBossHealthChanged?.Invoke(HealthNormalized);
    }

    private void Update()
    {
        if (!IsAlive || target == null) return;
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    public void TakeDamage(in DamageContext context)
    {
        if (!IsAlive) return;

        currentHP -= context.Amount;
        currentHP = Mathf.Max(0f, currentHP);
        OnBossHealthChanged?.Invoke(HealthNormalized);

        if (currentHP > 0f) return;

        OnBossDefeated?.Invoke();
        Destroy(gameObject);
    }
}
