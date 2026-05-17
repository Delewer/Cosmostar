using Cosmostar.Core.Design;
using Cosmostar.Core.Models;
using Xunit;

namespace Cosmostar.Core.Tests
{
    public sealed class RunDirectorTests
    {
        [Fact]
        public void RunDirector_TransitionsAndQueuesDraftsAcrossWaves()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var director = new RunDirector(catalog.Waves);

            var tick = director.Advance(70f, false);
            Assert.Equal(RunPhase.Escalation, tick.Phase);
            Assert.True(tick.DraftPending);

            director.ConsumeDraft();
            tick = director.Advance(80f, false);
            Assert.Equal(RunPhase.Elite, tick.Phase);
            Assert.True(tick.DraftPending);

            director.ConsumeDraft();
            tick = director.Advance(80f, false);
            Assert.Equal(RunPhase.Boss, tick.Phase);
        }

        [Fact]
        public void RunDirector_EndsWhenBossIsDefeated()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var director = new RunDirector(catalog.Waves);

            var tick = director.Advance(10f, true);

            Assert.Equal(RunPhase.Results, tick.Phase);
            Assert.Equal(0f, tick.SpawnRatePerSecond);
        }
    }
}
