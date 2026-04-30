using System.Collections.Generic;
using Cosmostar.Core.Design;
using Cosmostar.Core.Models;
using Cosmostar.Core.Systems;
using Xunit;

namespace Cosmostar.Core.Tests
{
    public sealed class UpgradeDraftSystemTests
    {
        [Fact]
        public void GenerateChoices_ExcludesMaxedUpgradesAndDuplicates()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var build = new RunBuildState();
            build.Upgrades.Add(new UpgradeStack { UpgradeId = "plasma_focus", Stacks = 5 });

            var draftSystem = new UpgradeDraftSystem();
            var picks = draftSystem.GenerateChoices(catalog.Upgrades, build, 3, new StubRandomSource(0.1f, 0.4f, 0.8f));

            Assert.Equal(3, picks.Count);
            Assert.DoesNotContain(picks, upgrade => upgrade.Id == "plasma_focus");
            Assert.Equal(picks.Count, new HashSet<string>(picks.ConvertAll(upgrade => upgrade.Id)).Count);
        }

        [Fact]
        public void ApplyUpgrade_AccumulatesExpectedBuildBonuses()
        {
            var upgrade = new UpgradeDef
            {
                Id = "chain_surge_upgrade",
                EffectType = UpgradeEffectType.ChainChance,
                Magnitude = 0.1f,
                AbilityFamily = AbilityFamily.ChainSurge,
                AbilityId = "chain_surge"
            };

            var build = new RunBuildState();
            var system = new UpgradeDraftSystem();
            system.ApplyUpgrade(build, upgrade);

            Assert.Equal(0.1f, build.ChainChance, 3);
            Assert.Single(build.Upgrades);
            Assert.Contains("chain_surge", build.GrantedAbilityIds);
            Assert.Equal(1, system.GetStackCount(build, upgrade.Id));
        }

        [Fact]
        public void GenerateChoices_HidesLockedAbilityUpgradesUntilAbilityIsUnlocked()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var build = new RunBuildState();
            var draftSystem = new UpgradeDraftSystem();

            var lockedOnly = new List<UpgradeDef>
            {
                new UpgradeDef
                {
                    Id = "locked_ability_pick",
                    DisplayName = "Locked Ability Pick",
                    MaxStacks = 1,
                    Weight = 1f,
                    EffectType = UpgradeEffectType.ChainChance,
                    AbilityFamily = AbilityFamily.ChainSurge,
                    AbilityId = "chain_surge"
                }
            };

            var lockedResult = draftSystem.GenerateChoices(lockedOnly, build, new List<string>(), 1, new StubRandomSource(0.2f));
            var unlockedResult = draftSystem.GenerateChoices(lockedOnly, build, new List<string> { "chain_surge" }, 1, new StubRandomSource(0.2f));

            Assert.Empty(lockedResult);
            Assert.Single(unlockedResult);
        }

        private sealed class StubRandomSource : IRandomSource
        {
            private readonly Queue<float> _values;

            public StubRandomSource(params float[] values)
            {
                _values = new Queue<float>(values);
            }

            public float NextFloat()
            {
                return _values.Count > 0 ? _values.Dequeue() : 0.5f;
            }
        }
    }
}
