using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string Id;
    public string DisplayName;
    [TextArea] public string Description;
    public UpgradeRarity Rarity;
    [Min(0.01f)] public float RollWeight = 1f;
    public List<string> Tags = new();

    [Header("Stat Modifiers")]
    public float FlatDamage;
    public float FireRatePercent;
    public float CritChanceFlat;

    [Header("Synergy")]
    public string SynergyKey;
}

public enum UpgradeRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}
