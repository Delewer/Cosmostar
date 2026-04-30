using Cosmostar.Core.Design;
using Cosmostar.Core.Models;
using Cosmostar.Core.Systems;
using Xunit;

namespace Cosmostar.Core.Tests
{
    public sealed class MissionRuleSystemTests
    {
        [Fact]
        public void BossClear_MovesBossEarlierThanBaseTimeline()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var mission = catalog.Missions[2];
            var system = new MissionRuleSystem();

            var rules = system.Resolve(mission);
            var modifiedWaves = system.CreateModifiedWaves(catalog.Waves, rules);

            Assert.Equal(185f, rules.BossStartTimeOverride, 3);
            Assert.Equal(185f, modifiedWaves[3].StartSecond, 3);
            Assert.Equal(185f, modifiedWaves[2].EndSecond, 3);
        }

        [Fact]
        public void HardSurvival_AddsRammersToIntroWave()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var mission = catalog.Missions[5];
            var system = new MissionRuleSystem();

            var rules = system.Resolve(mission);
            var modifiedWaves = system.CreateModifiedWaves(catalog.Waves, rules);

            Assert.True(rules.AddRammerToIntroWave);
            Assert.Contains(EnemyArchetype.Rammer, modifiedWaves[0].SpawnArchetypes);
        }
    }
}
