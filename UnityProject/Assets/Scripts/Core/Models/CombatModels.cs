using System;
using System.Collections.Generic;

namespace Cosmostar.Core.Models
{
    public enum WeaponFamily
    {
        Pulse,
        Arc,
        Lance
    }

    public enum AbilityFamily
    {
        None,
        CryoWake,
        ChainSurge,
        DroneWing,
        OverclockPulse
    }

    public enum UpgradeEffectType
    {
        Damage,
        FireRate,
        ProjectileCount,
        MaxShield,
        RestoreShield,
        MoveSpeed,
        PickupRadius,
        Piercing,
        FrostChance,
        ChainChance,
        DroneCompanion,
        OverclockBurst
    }

    public enum EnemyArchetype
    {
        Scout,
        Miner,
        Rammer,
        ShardCaster,
        EliteWarden,
        NullSovereign
    }

    public enum RunOutcome
    {
        Failed,
        Completed
    }

    public enum RunAnomalyKind
    {
        None,
        MeteorShower,
        SolarFlare,
        SalvageBloom
    }

    [Serializable]
    public sealed class ShipDef
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public float BaseHull;
        public float BaseShield;
        public float MoveSpeed;
        public WeaponFamily StartingWeaponFamily;
    }

    [Serializable]
    public sealed class WeaponDef
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public WeaponFamily Family;
        public float FireInterval;
        public float ProjectileDamage;
        public float ProjectileSpeed;
        public int ProjectileCount;
        public float SpreadDegrees;
        public float CritChance;
    }

    [Serializable]
    public sealed class AbilityDef
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public AbilityFamily Family;
        public string Description = string.Empty;
        public float Magnitude;
        public float DurationSeconds;
    }

    [Serializable]
    public sealed class UpgradeDef
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Description = string.Empty;
        public int MaxStacks = 5;
        public float Weight = 1f;
        public UpgradeEffectType EffectType;
        public float Magnitude;
        public WeaponFamily WeaponFamily;
        public AbilityFamily AbilityFamily;
        public string AbilityId = string.Empty;
    }

    [Serializable]
    public sealed class ModuleDef
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Description = string.Empty;
        public ModuleEffectType EffectType;
        public float Magnitude;
        public int MaxLevel = 5;
        public int UnlockCost;
        public int UpgradeCost;
    }

    [Serializable]
    public sealed class EnemyDef
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public EnemyArchetype Archetype;
        public float Hull;
        public float Speed;
        public float ContactDamage;
        public float FireInterval;
        public float TelegraphSeconds;
        public float ScoreValue;
        public bool IsBoss;
    }

    [Serializable]
    public sealed class BossPhaseDef
    {
        public string Id = string.Empty;
        public int PhaseIndex;
        public float TriggerHealthNormalized;
        public float VolleyInterval;
        public int VolleyCount;
        public float ProjectileSpeed;
        public float TelegraphSeconds;
        public float ArenaPulseDamage;
    }

    [Serializable]
    public sealed class WaveDef
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public RunPhase Phase;
        public float StartSecond;
        public float EndSecond;
        public List<EnemyArchetype> SpawnArchetypes = new List<EnemyArchetype>();
        public float SpawnRatePerSecond;
        public bool GrantsUpgradeDraft;
    }

    [Serializable]
    public sealed class MissionDef
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Description = string.Empty;
        public MissionObjectiveKind ObjectiveKind;
        public int TargetValue;
        public float TargetDurationSeconds;
        public float RequiredShieldRatio = 0.4f;
        public string ModifierText = string.Empty;
        public float DifficultyRating;
        public bool DailyEligible = true;
        public RewardTable Reward = new RewardTable();
    }

    [Serializable]
    public sealed class UpgradeStack
    {
        public string UpgradeId = string.Empty;
        public int Stacks;
    }

    [Serializable]
    public sealed class RunBuildState
    {
        public List<UpgradeStack> Upgrades = new List<UpgradeStack>();
        public List<string> GrantedAbilityIds = new List<string>();
        public float DamageMultiplier = 1f;
        public float FireRateMultiplier = 1f;
        public int BonusProjectiles;
        public float BonusShield;
        public float ShieldRestore;
        public float MoveSpeedMultiplier = 1f;
        public float PickupRadiusBonus;
        public int BonusPierce;
        public float FrostChance;
        public float ChainChance;
        public int DroneCompanions;
        public float OverclockBurstDamage;
    }

    [Serializable]
    public sealed class MetaModifiers
    {
        public float BonusHull;
        public float BonusShield;
        public float DamageMultiplier = 1f;
        public float FireRateMultiplier = 1f;
        public float MoveSpeedMultiplier = 1f;
        public float PickupRadiusBonus;
        public int StartingRerolls;
        public int ReviveCharges;
        public float RewardMultiplier = 1f;
    }

    [Serializable]
    public sealed class RunSummary
    {
        public string MissionId = string.Empty;
        public RunOutcome Outcome;
        public float DurationSeconds;
        public int Kills;
        public bool BossDefeated;
        public bool Revived;
        public float EndingShieldRatio;
        public int PickupsCollected;
        public int SalvageCollected;
        public int Grazes;
        public int BestComboCount;
        public int AnomalyEventsTriggered;
    }

    [Serializable]
    public sealed class MissionEvaluation
    {
        public bool Completed;
        public int StarsEarned;
        public bool NoReviveBonus;
        public bool ShieldBonus;
    }

    [Serializable]
    public sealed class MissionRuleSet
    {
        public float GlobalSpawnRateMultiplier = 1f;
        public float StartingShieldMultiplier = 1f;
        public float BossStartTimeOverride = -1f;
        public bool AddRammerToIntroWave;
        public RunAnomalyKind AnomalyKind;
        public string AnomalyLabel = string.Empty;
        public float AnomalyFirstSecond = -1f;
        public float AnomalyIntervalSeconds;
        public float AnomalyTelegraphSeconds;
        public float AnomalyDamage;
        public int AnomalyCount;
    }

    [Serializable]
    public sealed class VerticalSliceCatalog
    {
        public ShipDef Ship = new ShipDef();
        public List<WeaponDef> Weapons = new List<WeaponDef>();
        public List<AbilityDef> Abilities = new List<AbilityDef>();
        public List<UpgradeDef> Upgrades = new List<UpgradeDef>();
        public List<ModuleDef> Modules = new List<ModuleDef>();
        public List<EnemyDef> Enemies = new List<EnemyDef>();
        public List<WaveDef> Waves = new List<WaveDef>();
        public List<BossPhaseDef> BossPhases = new List<BossPhaseDef>();
        public List<MissionDef> Missions = new List<MissionDef>();
        public List<UnlockTrackEntry> UnlockTrack = new List<UnlockTrackEntry>();
    }
}
