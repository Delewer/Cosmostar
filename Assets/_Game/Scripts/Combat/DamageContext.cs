using UnityEngine;

public enum DamageType { Kinetic, Energy, Fire, Ice, Shock, Poison }

public readonly struct DamageContext
{
    public readonly float Amount;
    public readonly DamageType Type;
    public readonly GameObject Source;
    public readonly bool CanCrit;
    public readonly float CritMultiplier;

    public DamageContext(float amount, DamageType type, GameObject source, bool canCrit = false, float critMultiplier = 1.5f)
    {
        Amount = amount;
        Type = type;
        Source = source;
        CanCrit = canCrit;
        CritMultiplier = critMultiplier;
    }
}
