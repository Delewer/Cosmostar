using System.Collections.Generic;
using System.Linq;
using Cosmostar.Core.Design;
using Cosmostar.Core.Models;
using Cosmostar.Core.Systems;
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
            var finalBoss = timeline.GetBossesDue(catalog, 599f, 600f, new HashSet<string> { "sky_reaper", "neon_hydra" });

            Assert.Single(firstBoss);
            Assert.Equal("sky_reaper", firstBoss[0].BossID);
            Assert.Single(secondBoss);
            Assert.Equal("neon_hydra", secondBoss[0].BossID);
            Assert.Single(finalBoss);
            Assert.Equal("eclipse_core", finalBoss[0].BossID);
            Assert.True(timeline.IsFinalBossVictory(catalog, "eclipse_core"));
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
