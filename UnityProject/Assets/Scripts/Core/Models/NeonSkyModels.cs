using System;
using System.Collections.Generic;
using System.Linq;

namespace NeonSkySurvivors.Core.Models
{
    public enum NeonEquipmentSlot
    {
        Weapon,
        Wings,
        Engine,
        Hull,
        Core,
        Radar
    }

    public enum NeonEquipmentRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic
    }

    public enum NeonStatType
    {
        AttackDamage,
        FireRate,
        MovementSpeed,
        MaxHP,
        CurrentHP,
        Armor,
        CriticalChance,
        CriticalDamage,
        MagnetRange,
        StartingEnergy,
        DashCooldown,
        DashDistance,
        SpecialChargeSpeed,
        XPModifier,
        CoinBonus
    }

    public enum NeonUpgradeCategory
    {
        Weapon,
        Passive,
        Trail,
        Defense,
        Special
    }

    public enum NeonEnemyBehaviorType
    {
        Chaser,
        FastChaser,
        Shooter,
        Tank,
        MineCarrier,
        Splitter,
        Boss
    }

    [Serializable]
    public sealed class NeonStatModifier
    {
        public NeonStatType StatType;
        public float Value;
        public bool IsPercent;
    }

    [Serializable]
    public sealed class NeonPlayerStats
    {
        public float AttackDamage = 10f;
        public float FireRate = 1f;
        public float MovementSpeed = 5f;
        public float MaxHP = 100f;
        public float CurrentHP = 100f;
        public float Armor;
        public float CriticalChance = 0.05f;
        public float CriticalDamage = 2f;
        public float MagnetRange = 2.5f;
        public float StartingEnergy;
        public float DashCooldown = 4f;
        public float DashDistance = 4f;
        public float SpecialChargeSpeed = 1f;
        public float XPModifier = 1f;
        public float CoinBonus = 1f;

        public NeonPlayerStats Clone()
        {
            return (NeonPlayerStats)MemberwiseClone();
        }
    }

    [Serializable]
    public sealed class NeonEquipmentItemDef
    {
        public string ItemID = string.Empty;
        public string Name = string.Empty;
        public NeonEquipmentSlot SlotType;
        public NeonEquipmentRarity Rarity;
        public int Level = 1;
        public int MaxLevel = 20;
        public List<NeonStatModifier> BaseStats = new List<NeonStatModifier>();
        public string SpecialEffect = string.Empty;
        public string Icon = string.Empty;
        public int UpgradeCoinCost = 20;
    }

    [Serializable]
    public sealed class NeonOwnedEquipmentItem
    {
        public string InstanceID = string.Empty;
        public string ItemID = string.Empty;
        public NeonEquipmentRarity Rarity;
        public int Level = 1;
    }

    [Serializable]
    public sealed class NeonSaveProfile
    {
        public int PlayerCoins = 120;
        public int PlayerMaterials;
        public List<NeonOwnedEquipmentItem> OwnedEquipmentItems = new List<NeonOwnedEquipmentItem>();
        public string EquippedWeaponItemID = string.Empty;
        public string EquippedWingsItemID = string.Empty;
        public string EquippedEngineItemID = string.Empty;
        public string EquippedHullItemID = string.Empty;
        public string EquippedCoreItemID = string.Empty;
        public string EquippedRadarItemID = string.Empty;
        public List<string> UnlockedWeapons = new List<string>();
        public int CompletedRuns;
        public float BestSurvivalTime;
        public int BossesDefeated;
        public float MusicVolume = 0.8f;
        public float SfxVolume = 1.0f;
        public bool VibrationEnabled = true;
        public int AccountLevel = 1;
        public int AccountXP = 0;
        public Dictionary<string, string> Settings = new Dictionary<string, string>();
    }

    [Serializable]
    public sealed class NeonUpgradeDef
    {
        public string Id = string.Empty;
        public string Name = string.Empty;
        public NeonUpgradeCategory Category;
        public int MaxLevel = 5;
        public string Description = string.Empty;
        public string RequiredPassiveId = string.Empty;
        public string EvolutionId = string.Empty;
        public List<NeonStatModifier> PerLevelStats = new List<NeonStatModifier>();
    }

    [Serializable]
    public sealed class NeonEnemyDef
    {
        public string EnemyID = string.Empty;
        public string Name = string.Empty;
        public float HP;
        public float Damage;
        public float Speed;
        public int XPDrop;
        public float CoinDropChance;
        public NeonEnemyBehaviorType BehaviorType;
        public string ProjectileType = string.Empty;
        public bool IsElite;
    }

    [Serializable]
    public sealed class NeonWaveSegmentDef
    {
        public string Id = string.Empty;
        public float StartSecond;
        public float EndSecond;
        public float SpawnRatePerSecond;
        public string WarningText = string.Empty;
        public float WarningSecond = -1f;
        public List<string> EnemyIDs = new List<string>();
    }

    [Serializable]
    public sealed class NeonBossDef
    {
        public string BossID = string.Empty;
        public string Name = string.Empty;
        public float SpawnSecond;
        public float HP;
        public float ContactDamage;
        public float BulletDamage;
        public string WarningText = string.Empty;
        public bool IsMiniBoss;
        public int RewardCoinBonus = 40;
        public string RewardRarityHint = nameof(NeonEquipmentRarity.Common);
        public List<string> PhaseNotes = new List<string>();
    }

    [Serializable]
    public sealed class NeonRewardConfig
    {
        public int BaseCoins = 20;
        public int CoinPerKill = 1;
        public int BossCoinBonus = 40;
        public int MiniBossCoinBonus = 18;
        public int SurvivalMinuteCoins = 3;
        public string FinalBossGuaranteedRarity = nameof(NeonEquipmentRarity.Rare);
    }

    [Serializable]
    public sealed class NeonSkySurvivorsCatalog
    {
        public string WorkingTitle = "Neon Sky Survivors";
        public float MissionDurationSeconds = 600f;
        public NeonPlayerStats BasePlayerStats = new NeonPlayerStats();
        public List<NeonEquipmentItemDef> Equipment = new List<NeonEquipmentItemDef>();
        public List<NeonUpgradeDef> Upgrades = new List<NeonUpgradeDef>();
        public List<NeonEnemyDef> Enemies = new List<NeonEnemyDef>();
        public List<NeonWaveSegmentDef> Waves = new List<NeonWaveSegmentDef>();
        public List<NeonBossDef> Bosses = new List<NeonBossDef>();
        public NeonRewardConfig Rewards = new NeonRewardConfig();
        public Dictionary<NeonEquipmentSlot, string> StartingEquipmentBySlot = Enum.GetValues(typeof(NeonEquipmentSlot))
            .Cast<NeonEquipmentSlot>()
            .ToDictionary(slot => slot, _ => string.Empty);
    }
}
