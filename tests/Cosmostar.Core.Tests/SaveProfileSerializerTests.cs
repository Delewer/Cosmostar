using Cosmostar.Core.Models;
using Cosmostar.Core.Systems;
using Xunit;

namespace Cosmostar.Core.Tests
{
    public sealed class SaveProfileSerializerTests
    {
        [Fact]
        public void SaveProfile_RoundTripsWithoutDroppingProgress()
        {
            var profile = new SaveProfile();
            profile.SoftCurrency = 222;
            profile.CurrentStreak = 3;
            profile.Modules.Add(new ModuleProgress { ModuleId = "reactor_core", Level = 2, Unlocked = true, Equipped = true });
            profile.UnlockedAbilityIds.Add("chain_surge");

            var json = SaveProfileSerializer.Serialize(profile);
            var restored = SaveProfileSerializer.Deserialize(json);

            Assert.Equal(profile.SoftCurrency, restored.SoftCurrency);
            Assert.Equal(profile.CurrentStreak, restored.CurrentStreak);
            Assert.Single(restored.Modules);
            Assert.Contains("chain_surge", restored.UnlockedAbilityIds);
        }
    }
}
