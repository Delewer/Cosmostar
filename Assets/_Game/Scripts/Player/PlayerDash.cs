using System;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [SerializeField] private float dashDistance = 2.5f;
    [SerializeField] private float dashCooldown = 4f;

    private float cooldownRemaining;

    public float CooldownRemaining => cooldownRemaining;
    public bool IsReady => cooldownRemaining <= 0f;

    public event Action<float> OnCooldownChanged;

    private void Update()
    {
        if (cooldownRemaining <= 0f) return;

        cooldownRemaining -= Time.deltaTime;
        if (cooldownRemaining < 0f) cooldownRemaining = 0f;
        OnCooldownChanged?.Invoke(cooldownRemaining);
    }

    public bool TryDash(Vector2 direction)
    {
        if (!IsReady || direction.sqrMagnitude <= 0.0001f) return false;

        Vector3 delta = ((Vector3)direction.normalized) * dashDistance;
        transform.position += delta;
        cooldownRemaining = dashCooldown;
        OnCooldownChanged?.Invoke(cooldownRemaining);
        return true;
    }
}
