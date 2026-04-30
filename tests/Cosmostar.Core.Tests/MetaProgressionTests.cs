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
    }
}
