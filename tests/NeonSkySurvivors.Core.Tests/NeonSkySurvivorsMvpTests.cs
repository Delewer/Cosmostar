using System.Collections.Generic;
using System.Linq;
using NeonSkySurvivors.Core.Design;
using NeonSkySurvivors.Core.Models;
using NeonSkySurvivors.Core.Systems;
using Xunit;

namespace NeonSkySurvivors.Core.Tests
{
    public sealed class NeonSkySurvivorsMvpTests
    {
        [Fact]
        public void Catalog_UsesExactSixMvpEquipmentSlotsAndStartingGear()
        {
            var catalog = NeonSkySurvivorsBlueprints.CreateMvpCatalog();
            var slots = NeonEquipmentSystem.GetMvpSlots();

            Assert.Equal(new[]
            {
                NeonEquipmentSlot.Weapon,
                NeonEquipmentSlot.Wings,
                NeonEquipmentSlot.Engine,
                NeonEquipmentSlot.Hull,
                NeonEquipmentSlot.Core,
                NeonEquipmentSlot.Radar
            }, slots);

            Assert.Equal(24, catalog.Equipment.Count);
            Assert.All(slots, slot => Assert.True(catalog.StartingEquipmentBySlot.ContainsKey(slot)));
            Assert.Equal("basic_blaster", catalog.StartingEquipmentBySlot[NeonEquipmentSlot.Weapon]);
            Assert.Equal("starter_wings", catalog.StartingEquipmentBySlot[NeonEquipmentSlot.Wings]);
            Assert.Equal("old_engine", catalog.StartingEquipmentBySlot[NeonEquipmentSlot.Engine]);
            Assert.Equal("light_hull", catalog.StartingEquipmentBySlot[NeonEquipmentSlot.Hull]);
            Assert.Equal("small_battery", catalog.StartingEquipmentBySlot[NeonEquipmentSlot.Core]);
            Assert.Equal("basic_scanner", catalog.StartingEquipmentBySlot[NeonEquipmentSlot.Radar]);
        }

        [Fact]
        public void StartingProfile_EquipsAircraftPartsAndCalculatesPlaneStats()
        {
            var catalog = NeonSkySurvivorsBlueprints.CreateMvpCatalog();
            var profile = new NeonSaveProfile();
            var system = new NeonEquipmentSystem();

            system.EnsureStartingProfile(profile, catalog);
            var stats = system.CalculateStats(profile, catalog);

            Assert.Equal(6, profile.OwnedEquipmentItems.Count);
            Assert.Equal("basic_blaster", profile.EquippedWeaponItemID);
            Assert.Equal("starter_wings", profile.EquippedWingsItemID);
            Assert.Equal("old_engine", profile.EquippedEngineItemID);
            Assert.Equal("light_hull", profile.EquippedHullItemID);
            Assert.Equal("small_battery", profile.EquippedCoreItemID);
            Assert.Equal("basic_scanner", profile.EquippedRadarItemID);
            Assert.True(stats.AttackDamage > catalog.BasePlayerStats.AttackDamage);
            Assert.True(stats.MaxHP > catalog.BasePlayerStats.MaxHP);
            Assert.True(stats.MovementSpeed > catalog.BasePlayerStats.MovementSpeed);
            Assert.True(stats.MagnetRange > catalog.BasePlayerStats.MagnetRange);
            Assert.True(stats.DashCooldown < catalog.BasePlayerStats.DashCooldown);
        }

        [Fact]
        public void MergeDuplicates_CombinesThreeSameItemsIntoNextRarity()
        {
            var profile = new NeonSaveProfile();
            profile.OwnedEquipmentItems.Add(new NeonOwnedEquipmentItem { InstanceID = "a", ItemID = "basic_blaster", Rarity = NeonEquipmentRarity.Common, Level = 1 });
            profile.OwnedEquipmentItems.Add(new NeonOwnedEquipmentItem { InstanceID = "b", ItemID = "basic_blaster", Rarity = NeonEquipmentRarity.Common, Level = 1 });
            profile.OwnedEquipmentItems.Add(new NeonOwnedEquipmentItem { InstanceID = "c", ItemID = "basic_blaster", Rarity = NeonEquipmentRarity.Common, Level = 1 });

            var merged = new NeonEquipmentSystem().TryMergeDuplicates(profile, "basic_blaster", NeonEquipmentRarity.Common, out var mergedItem);

            Assert.True(merged);
            Assert.NotNull(mergedItem);
            Assert.Single(profile.OwnedEquipmentItems);
            Assert.Equal(NeonEquipmentRarity.Uncommon, mergedItem.Rarity);
            Assert.Equal("basic_blaster", mergedItem.ItemID);
        }

        [Fact]
        public void Timeline_SpawnsBossesAtThreeSixAndTenMinutes()
        {
            var catalog = NeonSkySurvivorsBlueprints.CreateMvpCatalog();
            var timeline = new NeonRunTimelineSystem();

            var firstBoss = timeline.GetBossesDue(catalog, 179f, 180f, new HashSet<string>());
            var secondBoss = timeline.GetBossesDue(catalog, 359f, 360f, new HashSet<string> { "sky_reaper" });
            var firstMiniBoss = timeline.GetBossesDue(catalog, 449f, 450f, new HashSet<string> { "sky_reaper", "neon_hydra" });
            var secondMiniBoss = timeline.GetBossesDue(catalog, 524f, 525f, new HashSet<string> { "sky_reaper", "neon_hydra", "viper_ace" });
            var finalBoss = timeline.GetBossesDue(catalog, 599f, 600f, new HashSet<string> { "sky_reaper", "neon_hydra", "viper_ace", "bombardier_prime" });

            Assert.Single(firstBoss);
            Assert.Equal("sky_reaper", firstBoss[0].BossID);
            Assert.Single(secondBoss);
            Assert.Equal("neon_hydra", secondBoss[0].BossID);
            Assert.Single(firstMiniBoss);
            Assert.Equal("viper_ace", firstMiniBoss[0].BossID);
            Assert.True(firstMiniBoss[0].IsMiniBoss);
            Assert.Single(secondMiniBoss);
            Assert.Equal("bombardier_prime", secondMiniBoss[0].BossID);
            Assert.True(secondMiniBoss[0].IsMiniBoss);
            Assert.Single(finalBoss);
            Assert.Equal("eclipse_core", finalBoss[0].BossID);
            Assert.True(timeline.IsFinalBossVictory(catalog, "eclipse_core"));
        }

        [Fact]
        public void Timeline_UsesBalancedEarlyMidLateWavePacing()
        {
            var catalog = NeonSkySurvivorsBlueprints.CreateMvpCatalog();
            var timeline = new NeonRunTimelineSystem();
            var earlyWave = timeline.GetActiveWave(catalog, 30f)!;
            var warningWave = timeline.GetActiveWave(catalog, 150f)!;
            var bossRecoveryWave = timeline.GetActiveWave(catalog, 185f)!;
            var midWave = timeline.GetActiveWave(catalog, 300f)!;
            var lateWave = timeline.GetActiveWave(catalog, 535f)!;
            var finalSurgeWave = timeline.GetActiveWave(catalog, 575f)!;

            Assert.Equal(10, catalog.Waves.Count);
            Assert.True(earlyWave.SpawnRatePerSecond < warningWave.SpawnRatePerSecond);
            Assert.True(bossRecoveryWave.SpawnRatePerSecond < midWave.SpawnRatePerSecond);
            Assert.True(finalSurgeWave.SpawnRatePerSecond > lateWave.SpawnRatePerSecond);
            Assert.Contains("FINAL BOSS", timeline.GetWarning(catalog, 589f, 590f));
        }

        [Fact]
        public void UpgradeCatalog_IncludesMvpWeaponsPassivesAndTrailCards()
        {
            var catalog = NeonSkySurvivorsBlueprints.CreateMvpCatalog();
            var upgradeIds = catalog.Upgrades.Select(upgrade => upgrade.Id).ToHashSet();

            Assert.Contains("plasma_blaster", upgradeIds);
            Assert.Contains("homing_missiles", upgradeIds);
            Assert.Contains("laser_wings", upgradeIds);
            Assert.Contains("orbit_blades", upgradeIds);
            Assert.Contains("attack_boost", upgradeIds);
            Assert.Contains("cooldown_reduction", upgradeIds);
            Assert.Contains("xp_gain_boost", upgradeIds);
            Assert.Contains("longer_trail", upgradeIds);
            Assert.Contains("trail_damage_boost", upgradeIds);
            Assert.Contains("trail_explosion", upgradeIds);
            Assert.Equal("plasma_storm", catalog.Upgrades.Single(upgrade => upgrade.Id == "plasma_blaster").EvolutionId);
            Assert.Equal("rocket_swarm", catalog.Upgrades.Single(upgrade => upgrade.Id == "homing_missiles").EvolutionId);
        }
    }
}
