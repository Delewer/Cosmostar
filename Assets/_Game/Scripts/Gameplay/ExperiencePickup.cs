using System;
using UnityEngine;

public class ExperiencePickup : MonoBehaviour
{
    [SerializeField] private int amount = 1;
    [SerializeField] private float lifetimeSeconds = 12f;

    private float lifetimeRemaining;

    public int Amount => amount;

    public event Action<ExperiencePickup> OnExpired;

    public void Initialize(int value)
    {
        amount = Mathf.Max(1, value);
        lifetimeRemaining = lifetimeSeconds;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        lifetimeRemaining -= Time.deltaTime;
        if (lifetimeRemaining <= 0f)
        {
            Expire();
        }
    }

    public void Collect()
    {
        Expire();
    }

    private void Expire()
    {
        OnExpired?.Invoke(this);
    }
}
