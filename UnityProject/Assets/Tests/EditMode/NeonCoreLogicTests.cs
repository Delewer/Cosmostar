using System.Collections.Generic;
using System.Linq;
using NeonSkySurvivors.Core.Design;
using NeonSkySurvivors.Core.Models;
using NeonSkySurvivors.Core.Systems;
using NUnit.Framework;

namespace NeonSkySurvivors.Tests
{
    /// <summary>
    /// EditMode tests for the engine-independent Core (models / systems / blueprints).
    /// These exercise the gameplay logic that cannot be visually verified, and act as a
    /// safety net in CI for balance math, equipment rules, and run setup.
    /// </summary>
    public class NeonCoreLogicTests
    {
        private NeonSkySurvivorsCatalog _catalog = null!;
        private NeonEquipmentSystem _equipment = null!;
        private NeonRunGameplaySystem _gameplay = null!;

        [SetUp]
        public void SetUp()
        {
            _catalog = NeonSkySurvivorsBlueprints.CreateMvpCatalog();
            _equipment = new NeonEquipmentSystem();
            _gameplay = new NeonRunGameplaySystem(1234);
        }

        // ── Catalog integrity ────────────────────────────────────────────

        [Test]
        public void Catalog_HasAtLeastMvpContent()
        {
            Assert.GreaterOrEqual(_catalog.Equipment.Count, 24, "Spec asks for ~24 equipment items.");
            Assert.GreaterOrEqual(_catalog.Upgrades.Count, 4, "At least the 4 MVP weapons.");
            Assert.IsNotEmpty(_catalog.Enemies);
            Assert.IsNotEmpty(_catalog.Waves);
            Assert.IsNotEmpty(_catalog.Bosses);
        }

        [Test]
        public void Catalog_EveryItemHasIdAndName()
        {
            foreach (var item in _catalog.Equipment)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(item.ItemID), "Item missing ItemID.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(item.Name), $"{item.ItemID} missing Name.");
            }
        }

        [Test]
        public void Catalog_ItemIdsAreUnique()
        {
            var ids = _catalog.Equipment.Select(i => i.ItemID).ToList();
            Assert.AreEqual(ids.Count, ids.Distinct().Count(), "Duplicate equipment ItemIDs.");
        }

        [Test]
        public void Catalog_CoversAllSixSlots()
        {
            foreach (var slot in NeonEquipmentSystem.GetMvpSlots())
            {
                Assert.IsTrue(_catalog.Equipment.Any(i => i.SlotType == slot), $"No item for slot {slot}.");
            }
        }

        [Test]
        public void Catalog_UpgradeCostMatchesRarityTier()
        {
            var expected = new Dictionary<NeonEquipmentRarity, int>
            {
                { NeonEquipmentRarity.Common, 20 },
                { NeonEquipmentRarity.Uncommon, 30 },
                { NeonEquipmentRarity.Rare, 50 },
                { NeonEquipmentRarity.Epic, 80 },
                { NeonEquipmentRarity.Legendary, 130 },
                { NeonEquipmentRarity.Mythic, 220 },
            };
            foreach (var item in _catalog.Equipment)
            {
                Assert.AreEqual(expected[item.Rarity], item.UpgradeCoinCost,
                    $"{item.ItemID} ({item.Rarity}) has wrong base upgrade cost.");
            }
        }

        [Test]
        public void Catalog_HasMythicTierItems()
        {
            var mythics = _catalog.Equipment.Where(i => i.Rarity == NeonEquipmentRarity.Mythic).ToList();
            Assert.GreaterOrEqual(mythics.Count, 2, "Spec adds 2 Mythic items (Void Engine, Storm Reactor).");
        }

        // ── Starting profile / loadout ───────────────────────────────────

        [Test]
        public void StartingProfile_EquipsAllSixSlots()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);

            Assert.IsNotEmpty(profile.EquippedWeaponItemID);
            Assert.IsNotEmpty(profile.EquippedWingsItemID);
            Assert.IsNotEmpty(profile.EquippedEngineItemID);
            Assert.IsNotEmpty(profile.EquippedHullItemID);
            Assert.IsNotEmpty(profile.EquippedCoreItemID);
            Assert.IsNotEmpty(profile.EquippedRadarItemID);
            Assert.GreaterOrEqual(profile.OwnedEquipmentItems.Count, 6);
        }

        // ── Stat calculation ─────────────────────────────────────────────

        [Test]
        public void CalculateStats_ProducesPlayablePlane()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            var stats = _equipment.CalculateStats(profile, _catalog);

            Assert.Greater(stats.MaxHP, 0f);
            Assert.AreEqual(stats.MaxHP, stats.CurrentHP, 0.001f, "Run should start at full HP.");
            Assert.Greater(stats.AttackDamage, 0f);
            Assert.Greater(stats.MovementSpeed, 0f);
        }

        // ── Upgrade cost formula ─────────────────────────────────────────

        [Test]
        public void UpgradeCost_FollowsBaseLevelRarityFormula()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);

            var owned = profile.OwnedEquipmentItems[0];
            var def = _catalog.Equipment.First(i => i.ItemID == owned.ItemID);

            Assert.IsTrue(_equipment.TryGetUpgradeCost(profile, _catalog, owned.InstanceID, out var cost));
            var expected = def.UpgradeCoinCost + owned.Level * 10 + (int)owned.Rarity * 25;
            Assert.AreEqual(expected, cost);
        }

        // ── Merge rules ──────────────────────────────────────────────────

        [Test]
        public void Merge_ThreeCommonsBecomeOneUncommon()
        {
            var profile = new NeonSaveProfile();
            AddDuplicates(profile, "basic_blaster", NeonEquipmentRarity.Common, 3);

            Assert.IsTrue(_equipment.TryMergeDuplicates(profile, "basic_blaster", NeonEquipmentRarity.Common, out var merged));
            Assert.IsNotNull(merged);
            Assert.AreEqual(NeonEquipmentRarity.Uncommon, merged!.Rarity);
            Assert.AreEqual(1, profile.OwnedEquipmentItems.Count, "3 consumed, 1 produced.");
        }

        [Test]
        public void Merge_LegendaryBecomesMythic()
        {
            var profile = new NeonSaveProfile();
            AddDuplicates(profile, "overdrive_core", NeonEquipmentRarity.Legendary, 3);

            Assert.IsTrue(_equipment.TryMergeDuplicates(profile, "overdrive_core", NeonEquipmentRarity.Legendary, out var merged));
            Assert.AreEqual(NeonEquipmentRarity.Mythic, merged!.Rarity);
        }

        [Test]
        public void Merge_MythicCannotMergeFurther()
        {
            var profile = new NeonSaveProfile();
            AddDuplicates(profile, "void_engine", NeonEquipmentRarity.Mythic, 3);

            Assert.IsFalse(_equipment.TryMergeDuplicates(profile, "void_engine", NeonEquipmentRarity.Mythic, out _));
        }

        [Test]
        public void Merge_RequiresThreeDuplicates()
        {
            var profile = new NeonSaveProfile();
            AddDuplicates(profile, "basic_blaster", NeonEquipmentRarity.Common, 2);

            Assert.IsFalse(_equipment.TryMergeDuplicates(profile, "basic_blaster", NeonEquipmentRarity.Common, out _));
            Assert.AreEqual(2, profile.OwnedEquipmentItems.Count, "Nothing consumed on a failed merge.");
        }

        // ── Run setup ────────────────────────────────────────────────────

        [Test]
        public void StartRun_BeginsRunningAtFullHpAndZeroTime()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            var run = new NeonRunGameplaySystem(1337).StartRun(profile, _catalog);

            Assert.AreEqual(NeonRunStatus.Running, run.Status);
            Assert.AreEqual(0f, run.ElapsedSeconds, 0.001f);
            Assert.Greater(run.Player.Stats.MaxHP, 0f);
            Assert.AreEqual(run.Player.Stats.MaxHP, run.Player.Stats.CurrentHP, 0.001f);
        }

        [Test]
        public void StartRun_IsDeterministicForFixedSeed()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);

            var a = new NeonRunGameplaySystem(99).StartRun(profile, _catalog);
            var b = new NeonRunGameplaySystem(99).StartRun(profile, _catalog);
            Assert.AreEqual(a.Player.Stats.MaxHP, b.Player.Stats.MaxHP, 0.001f);
            Assert.AreEqual(a.Player.Stats.AttackDamage, b.Player.Stats.AttackDamage, 0.001f);
        }

        // ── Level-up draft: reroll / banish ──────────────────────────────

        [Test]
        public void StartRun_SeedsRerollAndBanishCharges()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            var run = _gameplay.StartRun(profile, _catalog);
            Assert.AreEqual(2, run.RerollsRemaining);
            Assert.AreEqual(1, run.BanishesRemaining);
        }

        [Test]
        public void Reroll_DecrementsAndRedrawsDraft()
        {
            var run = DraftRun();
            run.RerollsRemaining = 2;
            Assert.IsTrue(_gameplay.RerollDraft(run, _catalog));
            Assert.AreEqual(1, run.RerollsRemaining);
            Assert.Greater(run.DraftChoices.Count, 0);
            Assert.LessOrEqual(run.DraftChoices.Count, 3);
        }

        [Test]
        public void Reroll_FailsWithNoChargesOrWhenNotDrafting()
        {
            var run = DraftRun();
            run.RerollsRemaining = 0;
            Assert.IsFalse(_gameplay.RerollDraft(run, _catalog));

            run.RerollsRemaining = 1;
            run.Status = NeonRunStatus.Running;
            Assert.IsFalse(_gameplay.RerollDraft(run, _catalog));
        }

        [Test]
        public void Banish_RemovesUpgradeAndSpendsCharge()
        {
            var run = DraftRun();
            run.BanishesRemaining = 1;
            var target = run.DraftChoices[0];

            Assert.IsTrue(_gameplay.BanishUpgrade(run, _catalog, target));
            Assert.AreEqual(0, run.BanishesRemaining);
            Assert.IsTrue(run.BannedUpgradeIds.Contains(target.Id));
            Assert.IsFalse(run.DraftChoices.Contains(target), "Banished card removed from the draft.");
        }

        [Test]
        public void Banish_BannedUpgradeNeverReappearsAfterRerolls()
        {
            var run = DraftRun();
            run.BanishesRemaining = 1;
            var target = run.DraftChoices[0];
            _gameplay.BanishUpgrade(run, _catalog, target);

            run.RerollsRemaining = 50;
            for (var i = 0; i < 30; i++)
            {
                _gameplay.RerollDraft(run, _catalog);
                Assert.IsFalse(run.DraftChoices.Any(c => c.Id == target.Id),
                    "A banished upgrade must never be drafted again.");
            }
        }

        [Test]
        public void Banish_FailsWithNoChargesOrUnknownCard()
        {
            var run = DraftRun();
            run.BanishesRemaining = 0;
            Assert.IsFalse(_gameplay.BanishUpgrade(run, _catalog, run.DraftChoices[0]));

            run.BanishesRemaining = 1;
            var notShown = _catalog.Upgrades.First(u => !run.DraftChoices.Contains(u));
            Assert.IsFalse(_gameplay.BanishUpgrade(run, _catalog, notShown));
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private NeonRunState DraftRun(int choices = 3)
        {
            var run = new NeonRunState { Status = NeonRunStatus.LevelUpDraft };
            run.DraftChoices.AddRange(_catalog.Upgrades.Take(choices));
            return run;
        }

        private static void AddDuplicates(NeonSaveProfile profile, string itemId, NeonEquipmentRarity rarity, int count)
        {
            for (var i = 0; i < count; i++)
            {
                profile.OwnedEquipmentItems.Add(new NeonOwnedEquipmentItem
                {
                    InstanceID = itemId + "_" + i,
                    ItemID = itemId,
                    Rarity = rarity,
                    Level = 1
                });
            }
        }
    }
}
