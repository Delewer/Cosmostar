using Cosmostar.Core.Design;
using Cosmostar.Core.Models;
using Cosmostar.Core.Systems;
using Xunit;

namespace Cosmostar.Core.Tests
{
    public sealed class CatalogValidationSystemTests
    {
        [Fact]
        public void DefaultCatalog_IsValid()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var system = new CatalogValidationSystem();

            var report = system.Validate(catalog);

            Assert.True(report.IsValid);
            Assert.Empty(report.Issues);
        }

        [Fact]
        public void Validate_ReportsStructuralContentErrors()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            catalog.Weapons[1].Id = catalog.Weapons[0].Id;
            catalog.Upgrades.Add(new UpgradeDef
            {
                Id = "broken_ability_ref",
                MaxStacks = 1,
                Weight = 1f,
                EffectType = UpgradeEffectType.ChainChance,
                AbilityId = "missing_ability"
            });
            catalog.Waves[1].StartSecond = 10f;
            catalog.UnlockTrack[1].RequiredXp = catalog.UnlockTrack[0].RequiredXp;

            for (var index = 0; index < catalog.Missions.Count; index++)
            {
                catalog.Missions[index].DailyEligible = false;
            }

            var system = new CatalogValidationSystem();
            var report = system.Validate(catalog);

            Assert.False(report.IsValid);
            Assert.Contains(report.Issues, issue => issue.Code == "weapon.duplicate_id");
            Assert.Contains(report.Issues, issue => issue.Code == "upgrade.ability_ref");
            Assert.Contains(report.Issues, issue => issue.Code == "wave.overlap");
            Assert.Contains(report.Issues, issue => issue.Code == "unlock_track.order");
            Assert.Contains(report.Issues, issue => issue.Code == "missions.daily_missing");
        }

        [Fact]
        public void Validate_ReportsNegativeTelegraphDurations()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            catalog.Enemies[3].TelegraphSeconds = -0.1f;
            catalog.BossPhases[0].TelegraphSeconds = -0.1f;

            var system = new CatalogValidationSystem();
            var report = system.Validate(catalog);

            Assert.False(report.IsValid);
            Assert.Contains(report.Issues, issue => issue.Code == "enemy.telegraph_seconds");
            Assert.Contains(report.Issues, issue => issue.Code == "boss_phase.telegraph_seconds");
        }
    }
}
