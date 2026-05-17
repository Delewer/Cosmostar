using System;
using System.Collections.Generic;
using Cosmostar.Core.Models;

namespace Cosmostar.Core.Systems
{
    public sealed class MissionSystem
    {
        public MissionEvaluation Evaluate(MissionDef mission, RunSummary summary)
        {
            var evaluation = new MissionEvaluation();

            switch (mission.ObjectiveKind)
            {
                case MissionObjectiveKind.SurviveTime:
                    evaluation.Completed = summary.DurationSeconds >= mission.TargetDurationSeconds;
                    break;
                case MissionObjectiveKind.DefeatEnemies:
                    evaluation.Completed = summary.Kills >= mission.TargetValue;
                    break;
                case MissionObjectiveKind.DefeatBoss:
                    evaluation.Completed = summary.BossDefeated;
                    break;
                case MissionObjectiveKind.PreserveShield:
                    evaluation.Completed = summary.BossDefeated && summary.EndingShieldRatio >= mission.RequiredShieldRatio;
                    break;
            }

            evaluation.StarsEarned = evaluation.Completed ? 1 : 0;
            evaluation.NoReviveBonus = evaluation.Completed && !summary.Revived;
            evaluation.ShieldBonus = evaluation.Completed && summary.EndingShieldRatio >= 0.4f;

            if (evaluation.NoReviveBonus)
            {
                evaluation.StarsEarned += 1;
            }

            if (evaluation.ShieldBonus)
            {
                evaluation.StarsEarned += 1;
            }

            return evaluation;
        }

        public DailyContract GetDailyContract(List<MissionDef> missions, DateTime localDate)
        {
            var eligible = new List<MissionDef>();
            for (var index = 0; index < missions.Count; index++)
            {
                if (missions[index].DailyEligible)
                {
                    eligible.Add(missions[index]);
                }
            }

            var daySeed = localDate.Year * 1000 + localDate.DayOfYear;
            var mission = eligible[daySeed % eligible.Count];
            return new DailyContract
            {
                MissionId = mission.Id,
                Label = "Daily Flux Surge",
                RewardMultiplier = 1.35f
            };
        }

        public void ApplyMissionProgress(SaveProfile profile, MissionDef mission, MissionEvaluation evaluation, RunSummary summary)
        {
            var progress = ProfileQueries.GetMissionProgress(profile, mission.Id);
            if (progress == null)
            {
                progress = new MissionProgress { MissionId = mission.Id };
                profile.Missions.Add(progress);
            }

            progress.Clears += evaluation.Completed ? 1 : 0;
            progress.Completed = progress.Completed || evaluation.Completed;
            if (evaluation.StarsEarned > progress.StarsEarned)
            {
                progress.StarsEarned = evaluation.StarsEarned;
            }

            if (summary.EndingShieldRatio > progress.BestShieldRatio)
            {
                progress.BestShieldRatio = summary.EndingShieldRatio;
            }

            progress.NoReviveClear = progress.NoReviveClear || (evaluation.Completed && !summary.Revived);
        }
    }
}
