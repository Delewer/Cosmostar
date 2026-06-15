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

        [Test]
        public void StartingProfile_DoesNotResetPlayerChosenLoadout()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            profile.EquippedRadarItemID = "quantum_sensor";

            // Called again on every app launch and run start — must keep the player's pick.
            _equipment.EnsureStartingProfile(profile, _catalog);
            Assert.AreEqual("quantum_sensor", profile.EquippedRadarItemID,
                "EnsureStartingProfile must only fill empty slots, never overwrite a loadout.");
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

        // ── Evolution chests (boss drop path) ────────────────────────────

        [Test]
        public void BossDeath_DropsEvolutionChest()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            var run = _gameplay.StartRun(profile, _catalog);

            var firstBoss = _catalog.Bosses.OrderBy(b => b.SpawnSecond).First();
            run.Enemies.Add(new NeonRunEnemyState
            {
                EnemyID = firstBoss.BossID,
                Position = new NeonVector2(3f, 3f),
                HP = 0f,
                MaxHP = firstBoss.HP,
                IsBoss = true
            });

            _gameplay.Tick(run, _catalog, 0.001f);

            Assert.AreEqual(1, run.BossesKilled);
            Assert.AreEqual(1, run.EvolutionChests.Count, "Boss death must drop an evolution chest.");
        }

        [Test]
        public void EvolutionChest_EvolvesMaxedWeaponWithoutPassive()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            var run = _gameplay.StartRun(profile, _catalog);

            var weapon = _catalog.Upgrades.First(u => u.Id == "plasma_blaster");
            run.Build.UpgradeLevels[weapon.Id] = weapon.MaxLevel;

            Assert.IsTrue(_gameplay.OpenEvolutionChest(run, _catalog),
                "Chest must evolve a maxed weapon even without the required passive.");
            Assert.IsTrue(run.Build.EvolvedWeapons.Contains("plasma_storm"));
        }

        [Test]
        public void EvolutionChest_FallsBackToHealAndCharge()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            var run = _gameplay.StartRun(profile, _catalog);
            run.Player.Stats.CurrentHP = run.Player.Stats.MaxHP * 0.5f;
            var chargeBefore = run.Player.SpecialCharge;

            Assert.IsFalse(_gameplay.OpenEvolutionChest(run, _catalog), "No maxed weapon → no evolution.");
            Assert.Greater(run.Player.Stats.CurrentHP, run.Player.Stats.MaxHP * 0.5f, "Fallback heals.");
            Assert.Greater(run.Player.SpecialCharge, chargeBefore, "Fallback grants special charge.");
        }

        [Test]
        public void EvolutionChest_PickedUpAtPlayerPosition()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            var run = _gameplay.StartRun(profile, _catalog);
            run.EvolutionChests.Add(new NeonEvolutionChestState { Position = run.Player.Position });

            _gameplay.Tick(run, _catalog, 0.001f);

            Assert.AreEqual(0, run.EvolutionChests.Count, "Chest under the player is collected.");
        }

        [Test]
        public void EvolutionChest_ExpiresWhenIgnored()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            var run = _gameplay.StartRun(profile, _catalog);
            run.EvolutionChests.Add(new NeonEvolutionChestState
            {
                Position = new NeonVector2(4f, 4f),
                RemainingLife = 0.0005f
            });

            _gameplay.Tick(run, _catalog, 0.001f);

            Assert.AreEqual(0, run.EvolutionChests.Count, "Expired chest despawns.");
        }

        // ── Tesla Arc / Nova Mortar weapons ──────────────────────────────

        [Test]
        public void Catalog_HasSixWeaponsAllWithEvolutions()
        {
            var weapons = _catalog.Upgrades.Where(u => u.Category == NeonUpgradeCategory.Weapon).ToList();
            Assert.GreaterOrEqual(weapons.Count, 6, "4 MVP weapons + Tesla Arc + Nova Mortar.");
            foreach (var weapon in weapons)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(weapon.EvolutionId), weapon.Id + " missing evolution.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(weapon.RequiredPassiveId), weapon.Id + " missing required passive.");
            }

            Assert.IsTrue(weapons.Any(w => w.Id == "tesla_arc" && w.EvolutionId == "storm_cage"));
            Assert.IsTrue(weapons.Any(w => w.Id == "nova_mortar" && w.EvolutionId == "supernova"));
        }

        [Test]
        public void TeslaArc_ChainsAcrossNearbyEnemiesAndSetsCooldown()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            var run = _gameplay.StartRun(profile, _catalog);
            run.Build.UpgradeLevels["tesla_arc"] = 3; // zap + 2 chains

            var def = _catalog.Enemies.First();
            var positions = new[] { new NeonVector2(0.3f, 0f), new NeonVector2(0.5f, 0.1f), new NeonVector2(0.6f, -0.1f) };
            foreach (var position in positions)
            {
                run.Enemies.Add(new NeonRunEnemyState
                {
                    EnemyID = def.EnemyID,
                    Position = position,
                    HP = 1000f,
                    MaxHP = 1000f,
                    Behavior = def.BehaviorType
                });
            }

            _gameplay.Tick(run, _catalog, 0.02f);

            Assert.GreaterOrEqual(run.TeslaZaps.Count, 2, "Tesla Arc must chain to at least 2 enemies here.");
            Assert.Greater(run.Player.TeslaCooldownRemaining, 0f, "Tesla Arc goes on cooldown after firing.");
            Assert.Less(run.Enemies[0].HP, 1000f, "Nearest enemy takes chain-lightning damage.");
        }

        [Test]
        public void NovaMortar_FiresExplosiveShellOnCooldown()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            var run = _gameplay.StartRun(profile, _catalog);
            run.Build.UpgradeLevels["nova_mortar"] = 1;

            var def = _catalog.Enemies.First();
            run.Enemies.Add(new NeonRunEnemyState
            {
                EnemyID = def.EnemyID,
                Position = new NeonVector2(0.6f, 0.6f),
                HP = 1000f,
                MaxHP = 1000f,
                Behavior = def.BehaviorType
            });

            _gameplay.Tick(run, _catalog, 0.02f);

            Assert.IsTrue(run.Projectiles.Any(p => p.FromPlayer && p.ExplosionRadius > 0f && !p.IsHoming),
                "Nova Mortar must launch a non-homing explosive shell.");
            Assert.Greater(run.Player.MortarCooldownRemaining, 0f);
        }

        [Test]
        public void EvolutionChest_EvolvesTeslaArcIntoStormCage()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            var run = _gameplay.StartRun(profile, _catalog);
            var tesla = _catalog.Upgrades.First(u => u.Id == "tesla_arc");
            run.Build.UpgradeLevels[tesla.Id] = tesla.MaxLevel;

            Assert.IsTrue(_gameplay.OpenEvolutionChest(run, _catalog));
            Assert.IsTrue(run.Build.EvolvedWeapons.Contains("storm_cage"));
        }

        // ── Sector difficulty tiers ──────────────────────────────────────

        [Test]
        public void Sector_ScalesAreOneAtBaseAndGrowPerTier()
        {
            Assert.AreEqual(1f, NeonRunGameplaySystem.SectorEnemyHpScale(1), 0.0001f);
            Assert.AreEqual(1f, NeonRunGameplaySystem.SectorEnemyDamageScale(1), 0.0001f);
            Assert.AreEqual(1f, NeonRunGameplaySystem.SectorSpawnRateScale(1), 0.0001f);
            Assert.AreEqual(1f, NeonRunGameplaySystem.SectorRewardScale(1), 0.0001f);

            Assert.AreEqual(1.7f, NeonRunGameplaySystem.SectorEnemyHpScale(3), 0.0001f);
            Assert.AreEqual(1.5f, NeonRunGameplaySystem.SectorEnemyDamageScale(3), 0.0001f);
            Assert.AreEqual(1.6f, NeonRunGameplaySystem.SectorRewardScale(3), 0.0001f);
        }

        [Test]
        public void Sector_StartRunClampsToValidRange()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);

            Assert.AreEqual(1, _gameplay.StartRun(profile, _catalog).Sector, "Default is sector 1.");
            Assert.AreEqual(1, _gameplay.StartRun(profile, _catalog, 0).Sector, "Below range clamps up.");
            Assert.AreEqual(NeonRunGameplaySystem.MaxSector, _gameplay.StartRun(profile, _catalog, 99).Sector, "Above range clamps down.");
            Assert.AreEqual(4, _gameplay.StartRun(profile, _catalog, 4).Sector);
        }

        [Test]
        public void Sector_ScalesSpawnedEnemiesAndBosses()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            var run = _gameplay.StartRun(profile, _catalog, 3);

            // Keep the plane effectively unkillable so the sim reaches the first boss.
            run.Player.Stats.MaxHP = 1_000_000f;
            run.Player.Stats.CurrentHP = 1_000_000f;

            var firstBossSecond = _catalog.Bosses.Min(b => b.SpawnSecond);
            var safety = 0;
            while (run.ElapsedSeconds <= firstBossSecond + 1f && safety++ < 5000)
            {
                if (run.Status == NeonRunStatus.LevelUpDraft)
                {
                    _gameplay.ApplyUpgradeChoice(run, _catalog, run.DraftChoices[0]);
                    continue;
                }

                _gameplay.Tick(run, _catalog, 0.5f);
            }

            var hpScale = NeonRunGameplaySystem.SectorEnemyHpScale(3);
            var enemyDefs = _catalog.Enemies.ToDictionary(e => e.EnemyID);

            // At least one wave enemy must carry exactly def.HP × sector scale
            // (split children deliberately use derived HP and are skipped).
            var scaledEnemy = run.Enemies.Any(e => !e.IsBoss
                && enemyDefs.TryGetValue(e.EnemyID, out var def)
                && System.Math.Abs(e.MaxHP - def.HP * hpScale) < 0.01f);
            Assert.IsTrue(scaledEnemy, "Wave enemies must spawn with sector-scaled HP.");

            var boss = run.Enemies.FirstOrDefault(e => e.IsBoss && !e.IsMiniBoss);
            Assert.IsNotNull(boss, "First boss should have spawned by " + firstBossSecond + "s.");
            var bossDef = _catalog.Bosses.First(b => b.BossID == boss!.EnemyID);
            Assert.AreEqual(bossDef.HP * hpScale, boss!.MaxHP, 0.01f, "Boss HP must be sector-scaled.");
        }

        // ── Equipment effects ────────────────────────────────────────────

        [Test]
        public void QuantumSensor_GrantsBossRewardBoostEffect()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            profile.EquippedRadarItemID = "quantum_sensor";

            var run = _gameplay.StartRun(profile, _catalog);
            Assert.IsTrue(run.ActiveEquipmentEffects.Contains("boss_reward_boost"));
        }

        [Test]
        public void VoidEngine_DashKillReset_ClearsRemainingCooldown()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            profile.EquippedEngineItemID = "void_engine";

            var run = _gameplay.StartRun(profile, _catalog);
            Assert.IsTrue(run.ActiveEquipmentEffects.Contains("dash_kill_reset"));

            run.Player.DashCooldownRemaining = 5f;
            var def = _catalog.Enemies.First();
            run.Enemies.Add(new NeonRunEnemyState
            {
                EnemyID = def.EnemyID,
                Position = new NeonVector2(2f, 2f),
                HP = 0f,
                MaxHP = def.HP,
                Behavior = def.BehaviorType
            });

            _gameplay.Tick(run, _catalog, 0.001f);

            Assert.AreEqual(0f, run.Player.DashCooldownRemaining,
                "Void Engine: killing an enemy must reset dash cooldown to zero.");
        }

        [Test]
        public void StormReactor_DoubleNova_DealsTwiceBaseDamage()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);

            var runBase = _gameplay.StartRun(profile, _catalog);

            profile.EquippedCoreItemID = "storm_reactor";
            var runDouble = _gameplay.StartRun(profile, _catalog);
            Assert.IsTrue(runDouble.ActiveEquipmentEffects.Contains("double_nova"));

            var def = _catalog.Enemies.First();
            foreach (var run in new[] { runBase, runDouble })
            {
                run.Enemies.Add(new NeonRunEnemyState
                {
                    EnemyID = def.EnemyID,
                    Position = new NeonVector2(0.3f, 0.3f),
                    HP = 1_000_000f,
                    MaxHP = 1_000_000f,
                    Behavior = def.BehaviorType
                });
                run.Player.SpecialCharge = run.Player.SpecialChargeMax;
            }

            _gameplay.TryActivateSpecial(runBase);
            _gameplay.TryActivateSpecial(runDouble);

            var baseDamage   = 1_000_000f - runBase.Enemies[0].HP;
            var doubleDamage = 1_000_000f - runDouble.Enemies[0].HP;
            Assert.AreEqual(baseDamage * 2f, doubleDamage, 0.01f,
                "Storm Reactor double_nova must deal exactly twice the base Nova damage.");
        }

        [Test]
        public void StormReactor_AutoChargeSpecial_FiresWhenFull()
        {
            var profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(profile, _catalog);
            profile.EquippedCoreItemID = "storm_reactor";

            var run = _gameplay.StartRun(profile, _catalog);
            Assert.IsTrue(run.ActiveEquipmentEffects.Contains("auto_charge_special"));

            run.Player.SpecialCharge = run.Player.SpecialChargeMax;

            _gameplay.Tick(run, _catalog, 0.001f);

            Assert.Less(run.Player.SpecialCharge, run.Player.SpecialChargeMax,
                "Storm Reactor: full special must auto-fire during Tick, resetting the charge meter.");
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

        // ── Pilot milestone reward track ─────────────────────────────────────────

        [Test]
        public void MetaProgress_HasClaimedMilestoneLevelsList()
        {
            var meta = new NeonMetaProgress();
            Assert.IsNotNull(meta.ClaimedMilestoneLevels, "ClaimedMilestoneLevels should initialise to empty list.");
            Assert.AreEqual(0, meta.ClaimedMilestoneLevels.Count);
        }

        [Test]
        public void MetaProgress_ClaimedMilestoneLevels_PersistsAfterAddition()
        {
            var meta = new NeonMetaProgress();
            meta.ClaimedMilestoneLevels.Add(2);
            meta.ClaimedMilestoneLevels.Add(5);
            Assert.AreEqual(2, meta.ClaimedMilestoneLevels.Count);
            Assert.IsTrue(meta.ClaimedMilestoneLevels.Contains(2));
            Assert.IsTrue(meta.ClaimedMilestoneLevels.Contains(5));
        }

        [Test]
        public void MetaProgress_LevelOneHasNoMilestoneClaimed()
        {
            var meta = new NeonMetaProgress { AccountLevel = 1 };
            Assert.IsFalse(meta.ClaimedMilestoneLevels.Contains(1),
                "Level 1 has no milestone — nothing should be auto-claimed.");
        }

        [Test]
        public void MetaProgress_AccountXpThresholdGrowsWithLevel()
        {
            // threshold = 100 * level
            Assert.AreEqual(100, 100 * 1, "Lv1→Lv2 threshold");
            Assert.AreEqual(500, 100 * 5, "Lv5→Lv6 threshold");
            Assert.AreEqual(1000, 100 * 10, "Lv10→Lv11 threshold");
        }

        // ── Weekly mission model ──────────────────────────────────────────────────

        [Test]
        public void MetaProgress_WeeklyMissionInitialisesEmpty()
        {
            var meta = new NeonMetaProgress();
            Assert.IsNotNull(meta.WeeklyMission);
            Assert.IsTrue(string.IsNullOrEmpty(meta.WeeklyMission.Id), "WeeklyMission.Id should be empty on a new profile.");
            Assert.AreEqual(string.Empty, meta.WeeklyMissionDate);
        }

        [Test]
        public void MetaProgress_WeeklyMission_AssignAndRead()
        {
            var meta = new NeonMetaProgress();
            meta.WeeklyMission = new NeonMissionState
            {
                Id = "w_boss10", Name = "Boss Slayer", Metric = "bosses_total", Target = 10,
                RewardCoins = 250, RewardAccountXP = 100
            };
            meta.WeeklyMissionDate = "2026-W24";
            Assert.AreEqual("w_boss10", meta.WeeklyMission.Id);
            Assert.AreEqual(10, meta.WeeklyMission.Target);
            Assert.AreEqual("2026-W24", meta.WeeklyMissionDate);
        }

        [Test]
        public void MetaProgress_WeeklyMission_ProgressAccumulates()
        {
            var mission = new NeonMissionState { Id = "w_boss10", Target = 10, Metric = "bosses_total" };
            mission.Progress += 3;
            mission.Progress += 4;
            Assert.AreEqual(7, mission.Progress, "Weekly mission progress should accumulate across runs.");
            Assert.IsFalse(mission.Claimed);
            Assert.IsFalse(mission.Progress >= mission.Target);
        }

        // ── Lifetime stats & achievements model ──────────────────────────────────

        [Test]
        public void SaveProfile_LifetimeStats_InitialiseToZero()
        {
            var profile = new NeonSaveProfile();
            Assert.AreEqual(0, profile.LifetimeEnemiesKilled);
            Assert.AreEqual(0, profile.LifetimeBossesKilled);
            Assert.AreEqual(0f, profile.LifetimeTimePlayed, 0.001f);
        }

        [Test]
        public void SaveProfile_UnlockedAchievements_InitialisesEmpty()
        {
            var profile = new NeonSaveProfile();
            Assert.IsNotNull(profile.UnlockedAchievements);
            Assert.AreEqual(0, profile.UnlockedAchievements.Count);
        }

        [Test]
        public void SaveProfile_LifetimeStats_Accumulate()
        {
            var profile = new NeonSaveProfile();
            profile.LifetimeEnemiesKilled += 120;
            profile.LifetimeEnemiesKilled += 80;
            Assert.AreEqual(200, profile.LifetimeEnemiesKilled);
            profile.LifetimeBossesKilled += 3;
            Assert.AreEqual(3, profile.LifetimeBossesKilled);
        }

        [Test]
        public void SaveProfile_UnlockedAchievements_AddAndQuery()
        {
            var profile = new NeonSaveProfile();
            profile.UnlockedAchievements.Add("first_run");
            profile.UnlockedAchievements.Add("kill_1000");
            Assert.IsTrue(profile.UnlockedAchievements.Contains("first_run"));
            Assert.IsTrue(profile.UnlockedAchievements.Contains("kill_1000"));
            Assert.IsFalse(profile.UnlockedAchievements.Contains("sector_8"));
            Assert.AreEqual(2, profile.UnlockedAchievements.Count);
        }

        // ── Localization string table ─────────────────────────────────────────────

        [Test]
        public void NeonStrings_DefaultTable_HasExpectedKeys()
        {
            NeonStrings.ResetToDefault();
            Assert.AreEqual("NEON SKY SURVIVORS", NeonStrings.Get("menu.title"));
            Assert.AreEqual("SETTINGS", NeonStrings.Get("settings.title"));
            Assert.AreEqual("PAUSED", NeonStrings.Get("pause.title"));
            Assert.AreEqual("ACHIEVEMENTS", NeonStrings.Get("achievements.title"));
        }

        [Test]
        public void NeonStrings_MissingKey_ReturnsSelf()
        {
            NeonStrings.ResetToDefault();
            const string key = "nonexistent.key";
            Assert.AreEqual(key, NeonStrings.Get(key),
                "Unknown key should fall through to the key string itself.");
        }

        [Test]
        public void NeonStrings_Load_OverridesTable()
        {
            NeonStrings.Load(new System.Collections.Generic.Dictionary<string, string>
            {
                { "menu.title", "NEON SKY OVERRIDDEN" },
            });
            Assert.AreEqual("NEON SKY OVERRIDDEN", NeonStrings.Get("menu.title"));
            Assert.AreEqual("settings.title", NeonStrings.Get("settings.title"),
                "Keys not in override table should fall back to the key itself.");
            NeonStrings.ResetToDefault(); // restore for other tests
        }

        [Test]
        public void NeonStrings_ResetToDefault_RestoresEnglish()
        {
            NeonStrings.Load(new System.Collections.Generic.Dictionary<string, string>());
            NeonStrings.ResetToDefault();
            Assert.AreEqual("VICTORY", NeonStrings.Get("results.victory"));
        }
    }
}
