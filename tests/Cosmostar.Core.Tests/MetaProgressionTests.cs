using Cosmostar.Core.Design;
using Cosmostar.Core.Models;
using Cosmostar.Core.Systems;
using Xunit;

namespace Cosmostar.Core.Tests
{
    public sealed class MetaProgressionTests
    {
        [Fact]
        public void CollectNewUnlocks_GrantsTrackRewardsExactlyOnce()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var profile = new SaveProfile
            {
                UnlockTrackXp = 60
            };

            ProfileQueries.EnsureDefaultState(profile, catalog);
            var system = new MetaProgressionSystem();

            var firstPass = system.CollectNewUnlocks(profile, catalog);
            var secondPass = system.CollectNewUnlocks(profile, catalog);

            Assert.Equal(2, firstPass.Count);
            Assert.Empty(secondPass);
            Assert.Contains("cryo_wake", profile.UnlockedAbilityIds);
            Assert.Contains("chain_surge", profile.UnlockedAbilityIds);
        }

        [Fact]
        public void ToggleEquip_RejectsFourthEquippedModule()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var profile = new SaveProfile();
            ProfileQueries.EnsureDefaultState(profile, catalog);

            profile.Modules[1].Unlocked = true;
            profile.Modules[1].Equipped = true;
            profile.Modules[1].Level = 1;
            profile.Modules[2].Unlocked = true;
            profile.Modules[2].Equipped = true;
            profile.Modules[2].Level = 1;
            profile.Modules[3].Unlocked = true;
            profile.Modules[3].Equipped = false;
            profile.Modules[3].Level = 1;

            var system = new MetaProgressionSystem();
            var toggled = system.ToggleEquip(profile, profile.Modules[3].ModuleId);

            Assert.False(toggled);
            Assert.Equal(MetaProgressionSystem.MaxEquippedModules, ProfileQueries.GetEquippedModuleCount(profile));
        }

        [Fact]
        public void UnlockModule_SpendsCreditsAndEquipsWhenSlotAvailable()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var module = catalog.Modules[1];
            var profile = new SaveProfile
            {
                SoftCurrency = module.UnlockCost,
                ModuleShards = 999
            };
            ProfileQueries.EnsureDefaultState(profile, catalog);

            var system = new MetaProgressionSystem();
            var unlocked = system.TryUnlockOrUpgradeModule(profile, module);
            var progress = ProfileQueries.GetModuleProgress(profile, module.Id);

            Assert.True(unlocked);
            Assert.NotNull(progress);
            Assert.True(progress.Unlocked);
            Assert.True(progress.Equipped);
            Assert.Equal(1, progress.Level);
            Assert.Equal(0, profile.SoftCurrency);
            Assert.Equal(999, profile.ModuleShards);
        }

        [Fact]
        public void UpgradeModule_SpendsModuleShardsNotCredits()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var module = catalog.Modules[0];
            var profile = new SaveProfile
            {
                SoftCurrency = 0,
                ModuleShards = module.UpgradeCost
            };
            ProfileQueries.EnsureDefaultState(profile, catalog);

            var progress = ProfileQueries.GetModuleProgress(profile, module.Id);
            Assert.NotNull(progress);
            progress.Unlocked = true;
            progress.Level = 1;

            var system = new MetaProgressionSystem();
            var upgraded = system.TryUnlockOrUpgradeModule(profile, module);

            Assert.True(upgraded);
            Assert.Equal(2, progress.Level);
            Assert.Equal(0, profile.ModuleShards);
            Assert.Equal(0, profile.SoftCurrency);
        }

        [Fact]
        public void UpgradeModule_RejectsWhenShardsAreShort()
        {
            var catalog = VerticalSliceBlueprints.CreateDefaultCatalog();
            var module = catalog.Modules[0];
            var profile = new SaveProfile
            {
                SoftCurrency = 999,
                ModuleShards = module.UpgradeCost - 1
            };
            ProfileQueries.EnsureDefaultState(profile, catalog);

            var progress = ProfileQueries.GetModuleProgress(profile, module.Id);
            Assert.NotNull(progress);
            progress.Unlocked = true;
            progress.Level = 1;

            var system = new MetaProgressionSystem();
            var upgraded = system.TryUnlockOrUpgradeModule(profile, module);

            Assert.False(upgraded);
            Assert.Equal(1, progress.Level);
            Assert.Equal(module.UpgradeCost - 1, profile.ModuleShards);
            Assert.Equal(999, profile.SoftCurrency);
        }
    }
}
