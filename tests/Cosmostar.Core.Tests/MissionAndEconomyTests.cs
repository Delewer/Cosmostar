using System;
using Cosmostar.Core.Design;
using Cosmostar.Core.Models;
using Cosmostar.Core.Systems;
using Xunit;

namespace Cosmostar.Core.Tests
{
    public sealed class MissionAndEconomyTests
    {
        [Fact]
        public void CompletedMission_AwardsThreeStarsWithoutReviveAndWithShield()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var mission = catalog.Missions[2];
            var summary = new RunSummary
            {
                MissionId = mission.Id,
                BossDefeated = true,
                DurationSeconds = 260f,
                EndingShieldRatio = 0.55f,
                Revived = false
            };

            var system = new MissionSystem();
            var evaluation = system.Evaluate(mission, summary);

            Assert.True(evaluation.Completed);
            Assert.Equal(3, evaluation.StarsEarned);
        }

        [Fact]
        public void FirstUnlockIsAffordableWithinThreeCompletedRuns()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var profile = new SaveProfile();
            ProfileQueries.EnsureDefaultState(profile, catalog);
            var mission = catalog.Missions[0];
            var missionSystem = new MissionSystem();
            var economy = new EconomySystem();
            var meta = new MetaModifiers();
            var daily = missionSystem.GetDailyContract(catalog.Missions, new DateTime(2026, 4, 29));

            for (var runIndex = 0; runIndex < 3; runIndex++)
            {
                var summary = new RunSummary
                {
                    MissionId = mission.Id,
                    DurationSeconds = 245f,
                    EndingShieldRatio = 0.45f,
                    Revived = false,
                    Outcome = RunOutcome.Completed
                };

                var evaluation = missionSystem.Evaluate(mission, summary);
                var reward = economy.CalculateRewards(mission, evaluation, daily, profile, meta);
                economy.ApplyReward(profile, reward, evaluation);
                missionSystem.ApplyMissionProgress(profile, mission, evaluation, summary);
            }

            Assert.True(profile.SoftCurrency >= catalog.Modules[1].UnlockCost);
        }

        [Fact]
        public void CollectedSalvage_AddsSoftCurrencyBonus()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var mission = catalog.Missions[0];
            var profile = new SaveProfile();
            var evaluation = new MissionEvaluation { Completed = true, StarsEarned = 1 };
            var summary = new RunSummary { MissionId = mission.Id, SalvageCollected = 17 };
            var economy = new EconomySystem();

            var reward = economy.CalculateRewards(mission, evaluation, new DailyContract(), profile, new MetaModifiers(), summary);

            Assert.Equal(17, reward.SalvageBonus);
            Assert.Equal(mission.Reward.SoftCurrency + reward.MasteryBonus + reward.SalvageBonus, reward.TotalSoftCurrency);
        }

        [Fact]
        public void ProjectileGrazes_AddSoftCurrencyBonus()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var mission = catalog.Missions[0];
            var profile = new SaveProfile();
            var evaluation = new MissionEvaluation { Completed = true, StarsEarned = 1 };
            var summary = new RunSummary { MissionId = mission.Id, Grazes = 6 };
            var economy = new EconomySystem();

            var reward = economy.CalculateRewards(mission, evaluation, new DailyContract(), profile, new MetaModifiers(), summary);

            Assert.Equal(12, reward.GrazeBonus);
            Assert.Equal(mission.Reward.SoftCurrency + reward.MasteryBonus + reward.GrazeBonus, reward.TotalSoftCurrency);
        }

        [Fact]
        public void AnomalyEvents_AddSoftCurrencyBonus()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var mission = catalog.Missions[0];
            var profile = new SaveProfile();
            var evaluation = new MissionEvaluation { Completed = true, StarsEarned = 1 };
            var summary = new RunSummary { MissionId = mission.Id, AnomalyEventsTriggered = 4 };
            var economy = new EconomySystem();

            var reward = economy.CalculateRewards(mission, evaluation, new DailyContract(), profile, new MetaModifiers(), summary);

            Assert.Equal(12, reward.AnomalyBonus);
            Assert.Equal(mission.Reward.SoftCurrency + reward.MasteryBonus + reward.AnomalyBonus, reward.TotalSoftCurrency);
        }
    }
}
