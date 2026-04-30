using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    public float MaxHP = 100f;
    public float CurrentHP = 100f;
    public float Armor = 0f;
    public float MoveSpeed = 7f;
    public float FireRate = 4f;
    public float BaseDamage = 10f;
    public float CritChance = 0.1f;
    public float CritMultiplier = 1.5f;

    public float GetMitigatedDamage(float rawDamage)
    {
        return rawDamage * (100f / (100f + Mathf.Max(0f, Armor)));
    }
}
