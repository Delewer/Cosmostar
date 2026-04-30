using System;
using System.Collections.Generic;

namespace Cosmostar.Core.Models
{
    public enum ModuleEffectType
    {
        HullPlating,
        ReactorCore,
        ThrusterMesh,
        CapacitorLattice,
        SalvageMagnet,
        TacticalReroll,
        BackupSpark,
        CreditCache
    }

    public enum MissionObjectiveKind
    {
        SurviveTime,
        DefeatEnemies,
        DefeatBoss,
        PreserveShield
    }

    public enum RewardedPlacement
    {
        Revive,
        UpgradeReroll,
        DoubleResults
    }

    [Serializable]
    public sealed class RewardTable
    {
        public int SoftCurrency;
        public int ModuleShards;
        public int UnlockTrackXp;
        public bool DoubleRewardEligible = true;
    }

    [Serializable]
    public sealed class UnlockTrackEntry
    {
        public string Id = string.Empty;
        public int RequiredXp;
        public string RewardLabel = string.Empty;
        public int ModuleShards;
        public string AbilityUnlockId = string.Empty;
    }

    [Serializable]
    public sealed class ModuleProgress
    {
        public string ModuleId = string.Empty;
        public int Level;
        public bool Unlocked;
        public bool Equipped;
    }

    [Serializable]
    public sealed class MissionProgress
    {
        public string MissionId = string.Empty;
        public int StarsEarned;
        public int Clears;
        public bool Completed;
        public float BestShieldRatio;
        public bool NoReviveClear;
    }

    [Serializable]
    public sealed class SaveProfile
    {
        public int Version = 1;
        public int SoftCurrency = 90;
        public int ModuleShards;
        public int UnlockTrackXp;
        public int CurrentStreak;
        public int BestStreak;
        public bool SeenFtue;
        public string EquippedShipId = "starling_mk1";
        public List<ModuleProgress> Modules = new List<ModuleProgress>();
        public List<MissionProgress> Missions = new List<MissionProgress>();
        public List<string> UnlockedAbilityIds = new List<string>();
        public List<string> ClaimedUnlockTrackIds = new List<string>();
    }

    [Serializable]
    public sealed class DailyContract
    {
        public string MissionId = string.Empty;
        public string Label = string.Empty;
        public float RewardMultiplier = 1f;
    }

    [Serializable]
    public sealed class RewardBreakdown
    {
        public RewardTable BaseReward = new RewardTable();
        public int StreakBonus;
        public int MasteryBonus;
        public int DailyBonus;
        public bool Doubled;

        public int TotalSoftCurrency
        {
            get
            {
                var total = BaseReward.SoftCurrency + StreakBonus + MasteryBonus + DailyBonus;
                return Doubled ? total * 2 : total;
            }
        }

        public int TotalModuleShards
        {
            get
            {
                var total = BaseReward.ModuleShards;
                return Doubled ? total * 2 : total;
            }
        }

        public int TotalUnlockTrackXp
        {
            get
            {
                return Doubled ? BaseReward.UnlockTrackXp * 2 : BaseReward.UnlockTrackXp;
            }
        }
    }

    [Serializable]
    public sealed class AnalyticsEvent
    {
        public string EventName = string.Empty;
        public string Screen = string.Empty;
        public long TimestampUnix;
        public string ContextJson = string.Empty;
    }
}
