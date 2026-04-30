using System;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable, IUpgradeable
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private PlayerDash playerDash;
    [SerializeField] private float movementSmoothing = 20f;
    [SerializeField] private float invulnerabilitySecondsAfterHit = 0.25f;

    private Camera mainCamera;
    private bool isAlive = true;
    private float invulnerabilityRemaining;

    public bool IsAlive => isAlive;
    public event Action OnDied;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (stats != null)
        {
            stats.CurrentHP = stats.MaxHP;
        }
    }

    private void Update()
    {
        if (!isAlive) return;

        if (invulnerabilityRemaining > 0f)
        {
            invulnerabilityRemaining -= Time.deltaTime;
        }

        HandleMovement();
        HandleDashInput();
    }

    private void HandleMovement()
    {
        if (mainCamera == null) return;

        if (Input.touchCount > 0)
        {
            MoveToScreenPoint(Input.GetTouch(0).position);
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(0))
        {
            MoveToScreenPoint(Input.mousePosition);
        }
#endif
    }

    private void HandleDashInput()
    {
        if (playerDash == null) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector2 dir = Vector2.up;
            playerDash.TryDash(dir);
        }
#endif
    }

    private void MoveToScreenPoint(Vector3 screenPosition)
    {
        float zDistance = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        Vector3 world = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, zDistance));
        world.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, world, Time.deltaTime * movementSmoothing);
    }

    public void TakeDamage(in DamageContext context)
    {
        if (!isAlive || stats == null || invulnerabilityRemaining > 0f) return;

        float damage = stats.GetMitigatedDamage(context.Amount);
        stats.CurrentHP -= damage;
        invulnerabilityRemaining = invulnerabilitySecondsAfterHit;

        if (stats.CurrentHP > 0f) return;

        stats.CurrentHP = 0f;
        isAlive = false;
        weaponController?.SetAutoFire(false);
        OnDied?.Invoke();
    }

    public void ApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null || stats == null) return;

        stats.BaseDamage += upgrade.FlatDamage;
        stats.FireRate *= (1f + upgrade.FireRatePercent);
        stats.CritChance = Mathf.Clamp01(stats.CritChance + upgrade.CritChanceFlat);

        weaponController?.RebuildFromStats(stats);
    }

    public void ApplySynergyBonus(float fireRateMultiplier, float damageMultiplier, float critChanceBonus)
    {
        if (stats == null) return;

        stats.FireRate *= Mathf.Max(0.1f, fireRateMultiplier);
        stats.BaseDamage *= Mathf.Max(0.1f, damageMultiplier);
        stats.CritChance = Mathf.Clamp01(stats.CritChance + critChanceBonus);

        weaponController?.RebuildFromStats(stats);
    }

    public float GetHealthNormalized()
    {
        if (stats == null || stats.MaxHP <= 0f) return 0f;
        return Mathf.Clamp01(stats.CurrentHP / stats.MaxHP);
    }
}
