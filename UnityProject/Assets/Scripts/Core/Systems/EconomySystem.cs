#nullable disable

using Cosmostar.Core.Models;

namespace Cosmostar.Core.Systems
{
    public sealed class EconomySystem
    {
        public RewardBreakdown CalculateRewards(MissionDef mission, MissionEvaluation evaluation, DailyContract dailyContract, SaveProfile profile, MetaModifiers modifiers, RunSummary summary = null)
        {
            var breakdown = new RewardBreakdown();
            breakdown.BaseReward = new RewardTable
            {
                SoftCurrency = mission.Reward.SoftCurrency,
                ModuleShards = mission.Reward.ModuleShards,
                UnlockTrackXp = mission.Reward.UnlockTrackXp,
                DoubleRewardEligible = mission.Reward.DoubleRewardEligible
            };
            breakdown.SalvageBonus = summary == null ? 0 : summary.SalvageCollected;
            breakdown.GrazeBonus = summary == null ? 0 : summary.Grazes * 2;
            breakdown.AnomalyBonus = summary == null ? 0 : summary.AnomalyEventsTriggered * 3;

            if (!evaluation.Completed)
            {
                breakdown.BaseReward.SoftCurrency = mission.Reward.SoftCurrency / 2;
                breakdown.BaseReward.ModuleShards = mission.Reward.ModuleShards / 2;
                breakdown.BaseReward.UnlockTrackXp = mission.Reward.UnlockTrackXp / 2;
                return breakdown;
            }

            breakdown.StreakBonus = profile.CurrentStreak * 4;
            breakdown.MasteryBonus = evaluation.StarsEarned * 6;
            if (dailyContract != null && dailyContract.MissionId == mission.Id)
            {
                breakdown.DailyBonus = (int)(mission.Reward.SoftCurrency * (dailyContract.RewardMultiplier - 1f));
            }

            breakdown.BaseReward.SoftCurrency += (int)(mission.Reward.SoftCurrency * (modifiers.RewardMultiplier - 1f));
            return breakdown;
        }

        public void ApplyReward(SaveProfile profile, RewardBreakdown breakdown, MissionEvaluation evaluation)
        {
            profile.SoftCurrency += breakdown.TotalSoftCurrency;
            profile.ModuleShards += breakdown.TotalModuleShards;
            profile.UnlockTrackXp += breakdown.TotalUnlockTrackXp;

            if (evaluation.Completed)
            {
                profile.CurrentStreak += 1;
                if (profile.CurrentStreak > profile.BestStreak)
                {
                    profile.BestStreak = profile.CurrentStreak;
                }
            }
            else
            {
                profile.CurrentStreak = 0;
            }
        }
    }
}
