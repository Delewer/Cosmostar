#nullable enable annotations

using System.Collections.Generic;
using NeonSkySurvivors.Core.Models;

namespace NeonSkySurvivors.Core.Design
{
    public static class NeonSkySurvivorsBlueprints
    {
        public static NeonSkySurvivorsCatalog CreateMvpCatalog()
        {
            var catalog = new NeonSkySurvivorsCatalog();

            AddEquipment(catalog);
            AddUpgrades(catalog);
            AddEnemies(catalog);
            AddWaves(catalog);
            AddBosses(catalog);

            catalog.StartingEquipmentBySlot[NeonEquipmentSlot.Weapon] = "basic_blaster";
            catalog.StartingEquipmentBySlot[NeonEquipmentSlot.Wings] = "starter_wings";
            catalog.StartingEquipmentBySlot[NeonEquipmentSlot.Engine] = "old_engine";
            catalog.StartingEquipmentBySlot[NeonEquipmentSlot.Hull] = "light_hull";
            catalog.StartingEquipmentBySlot[NeonEquipmentSlot.Core] = "small_battery";
            catalog.StartingEquipmentBySlot[NeonEquipmentSlot.Radar] = "basic_scanner";

            return catalog;
        }

        private static void AddEquipment(NeonSkySurvivorsCatalog catalog)
        {
            catalog.Equipment.Add(Item("basic_blaster", "Basic Blaster", NeonEquipmentSlot.Weapon, Stat(NeonStatType.AttackDamage, 10f), Stat(NeonStatType.FireRate, 1f)));
            catalog.Equipment.Add(Item("twin_cannon", "Twin Cannon", NeonEquipmentSlot.Weapon, Stat(NeonStatType.AttackDamage, 13f), Stat(NeonStatType.FireRate, 1.08f, true)));
            catalog.Equipment.Add(Item("plasma_needle", "Plasma Needle", NeonEquipmentSlot.Weapon, Stat(NeonStatType.AttackDamage, 16f), Stat(NeonStatType.CriticalChance, 0.02f)));
            catalog.Equipment.Add(Item("railgun_nose", "Railgun Nose", NeonEquipmentSlot.Weapon, Stat(NeonStatType.AttackDamage, 24f), Stat(NeonStatType.FireRate, -0.1f, true)));

            catalog.Equipment.Add(Item("starter_wings", "Starter Wings", NeonEquipmentSlot.Wings, Stat(NeonStatType.MovementSpeed, 0.2f)));
            catalog.Equipment.Add(Item("falcon_wings", "Falcon Wings", NeonEquipmentSlot.Wings, Stat(NeonStatType.MovementSpeed, 0.45f), Stat(NeonStatType.FireRate, 1.04f, true)));
            catalog.Equipment.Add(Item("combat_wings", "Combat Wings", NeonEquipmentSlot.Wings, Stat(NeonStatType.FireRate, 1.08f, true), Stat(NeonStatType.CriticalChance, 0.015f)));
            catalog.Equipment.Add(Item("neon_wings", "Neon Wings", NeonEquipmentSlot.Wings, "After dash, gain +20% FireRate for 2 seconds.", Stat(NeonStatType.MovementSpeed, 0.35f), Stat(NeonStatType.CriticalChance, 0.025f)));

            catalog.Equipment.Add(Item("old_engine", "Old Engine", NeonEquipmentSlot.Engine, Stat(NeonStatType.MovementSpeed, 0.25f), Stat(NeonStatType.DashCooldown, -0.1f)));
            catalog.Equipment.Add(Item("turbo_engine", "Turbo Engine", NeonEquipmentSlot.Engine, Stat(NeonStatType.MovementSpeed, 0.55f), Stat(NeonStatType.DashCooldown, -0.3f)));
            catalog.Equipment.Add(Item("ion_engine", "Ion Engine", NeonEquipmentSlot.Engine, Stat(NeonStatType.DashCooldown, -0.45f), Stat(NeonStatType.DashDistance, 0.4f)));
            catalog.Equipment.Add(Item("phantom_engine", "Phantom Engine", NeonEquipmentSlot.Engine, "Dash leaves a damaging trail.", Stat(NeonStatType.MovementSpeed, 0.35f), Stat(NeonStatType.DashCooldown, -0.55f)));

            catalog.Equipment.Add(Item("light_hull", "Light Hull", NeonEquipmentSlot.Hull, Stat(NeonStatType.MaxHP, 12f)));
            catalog.Equipment.Add(Item("steel_hull", "Steel Hull", NeonEquipmentSlot.Hull, Stat(NeonStatType.MaxHP, 28f), Stat(NeonStatType.Armor, 1f)));
            catalog.Equipment.Add(Item("guardian_frame", "Guardian Frame", NeonEquipmentSlot.Hull, "Block the first hit every 30 seconds.", Stat(NeonStatType.MaxHP, 34f), Stat(NeonStatType.Armor, 2f)));
            catalog.Equipment.Add(Item("solar_shield_hull", "Solar Shield Hull", NeonEquipmentSlot.Hull, "Gain shield when HP drops below 30%.", Stat(NeonStatType.MaxHP, 24f), Stat(NeonStatType.Armor, 3f)));

            catalog.Equipment.Add(Item("small_battery", "Small Battery", NeonEquipmentSlot.Core, Stat(NeonStatType.StartingEnergy, 10f), Stat(NeonStatType.SpecialChargeSpeed, 1.02f, true)));
            catalog.Equipment.Add(Item("fusion_core", "Fusion Core", NeonEquipmentSlot.Core, Stat(NeonStatType.StartingEnergy, 18f), Stat(NeonStatType.XPModifier, 1.04f, true)));
            catalog.Equipment.Add(Item("plasma_core", "Plasma Core", NeonEquipmentSlot.Core, Stat(NeonStatType.SpecialChargeSpeed, 1.12f, true), Stat(NeonStatType.XPModifier, 1.06f, true)));
            catalog.Equipment.Add(Item("overdrive_core", "Overdrive Core", NeonEquipmentSlot.Core, "After leveling up, gain a temporary damage boost.", Stat(NeonStatType.StartingEnergy, 25f), Stat(NeonStatType.SpecialChargeSpeed, 1.16f, true)));

            catalog.Equipment.Add(Item("basic_scanner", "Basic Scanner", NeonEquipmentSlot.Radar, Stat(NeonStatType.MagnetRange, 0.35f)));
            catalog.Equipment.Add(Item("magnet_scanner", "Magnet Scanner", NeonEquipmentSlot.Radar, Stat(NeonStatType.MagnetRange, 0.9f), Stat(NeonStatType.XPModifier, 1.03f, true)));
            catalog.Equipment.Add(Item("hunter_radar", "Hunter Radar", NeonEquipmentSlot.Radar, Stat(NeonStatType.CriticalChance, 0.03f), Stat(NeonStatType.CoinBonus, 1.04f, true)));
            catalog.Equipment.Add(Item("quantum_sensor", "Quantum Sensor", NeonEquipmentSlot.Radar, "Boss rewards improved.", Stat(NeonStatType.MagnetRange, 0.7f), Stat(NeonStatType.CoinBonus, 1.1f, true)));
        }

        private static void AddUpgrades(NeonSkySurvivorsCatalog catalog)
        {
            catalog.Upgrades.Add(Upgrade("plasma_blaster", "Plasma Blaster", NeonUpgradeCategory.Weapon, "Unlock and improve the main projectile weapon.", "attack_boost", "plasma_storm"));
            catalog.Upgrades.Add(Upgrade("homing_missiles", "Homing Missiles", NeonUpgradeCategory.Weapon, "Missiles target nearby enemies.", "cooldown_reduction", "rocket_swarm"));
            catalog.Upgrades.Add(Upgrade("laser_wings", "Laser Wings", NeonUpgradeCategory.Weapon, "Side laser beam attacks."));
            catalog.Upgrades.Add(Upgrade("orbit_blades", "Orbit Blades", NeonUpgradeCategory.Weapon, "Energy blades rotate around the plane."));

            catalog.Upgrades.Add(Upgrade("attack_boost", "Attack Boost", NeonUpgradeCategory.Passive, "+10% damage per level.", perLevel: Stat(NeonStatType.AttackDamage, 1.1f, true)));
            catalog.Upgrades.Add(Upgrade("fire_rate_boost", "Fire Rate Boost", NeonUpgradeCategory.Passive, "+10% fire rate per level.", perLevel: Stat(NeonStatType.FireRate, 1.1f, true)));
            catalog.Upgrades.Add(Upgrade("movement_speed_boost", "Movement Speed Boost", NeonUpgradeCategory.Passive, "+8% movement speed per level.", perLevel: Stat(NeonStatType.MovementSpeed, 1.08f, true)));
            catalog.Upgrades.Add(Upgrade("max_hp_boost", "Max HP Boost", NeonUpgradeCategory.Passive, "+15 max HP per level.", perLevel: Stat(NeonStatType.MaxHP, 15f)));
            catalog.Upgrades.Add(Upgrade("armor_boost", "Armor Boost", NeonUpgradeCategory.Passive, "+1 armor per level.", perLevel: Stat(NeonStatType.Armor, 1f)));
            catalog.Upgrades.Add(Upgrade("magnet_boost", "Magnet Boost", NeonUpgradeCategory.Passive, "+15% magnet range per level.", perLevel: Stat(NeonStatType.MagnetRange, 1.15f, true)));
            catalog.Upgrades.Add(Upgrade("critical_chance_boost", "Critical Chance Boost", NeonUpgradeCategory.Passive, "+4% critical chance per level.", perLevel: Stat(NeonStatType.CriticalChance, 0.04f)));
            catalog.Upgrades.Add(Upgrade("cooldown_reduction", "Cooldown Reduction", NeonUpgradeCategory.Passive, "Weapon skills and dash recover faster.", perLevel: Stat(NeonStatType.DashCooldown, -0.12f)));
            catalog.Upgrades.Add(Upgrade("xp_gain_boost", "XP Gain Boost", NeonUpgradeCategory.Passive, "+10% XP gain per level.", perLevel: Stat(NeonStatType.XPModifier, 1.1f, true)));

            catalog.Upgrades.Add(Upgrade("longer_trail", "Longer Trail", NeonUpgradeCategory.Trail, "Dash trail lasts longer."));
            catalog.Upgrades.Add(Upgrade("trail_damage_boost", "Trail Damage Boost", NeonUpgradeCategory.Trail, "Dash trail damage scales higher with AttackDamage."));
            catalog.Upgrades.Add(Upgrade("trail_explosion", "Trail Explosion", NeonUpgradeCategory.Trail, "Dash trail explodes at the end."));
        }

        private static void AddEnemies(NeonSkySurvivorsCatalog catalog)
        {
            catalog.Enemies.Add(new NeonEnemyDef { EnemyID = "chaser_drone", Name = "Chaser Drone", HP = 20f, Damage = 10f, Speed = 2f, XPDrop = 1, CoinDropChance = 0.15f, BehaviorType = NeonEnemyBehaviorType.Chaser });
            catalog.Enemies.Add(new NeonEnemyDef { EnemyID = "fast_wing", Name = "Fast Wing", HP = 12f, Damage = 8f, Speed = 3.5f, XPDrop = 1, CoinDropChance = 0.1f, BehaviorType = NeonEnemyBehaviorType.FastChaser });
            catalog.Enemies.Add(new NeonEnemyDef { EnemyID = "shooter_drone", Name = "Shooter Drone", HP = 30f, Damage = 8f, Speed = 1.5f, XPDrop = 2, CoinDropChance = 0.18f, BehaviorType = NeonEnemyBehaviorType.Shooter, ProjectileType = "simple_bullet" });
            catalog.Enemies.Add(new NeonEnemyDef { EnemyID = "shield_drone", Name = "Shield Drone", HP = 70f, Damage = 12f, Speed = 1.1f, XPDrop = 3, CoinDropChance = 0.22f, BehaviorType = NeonEnemyBehaviorType.Tank });
            catalog.Enemies.Add(new NeonEnemyDef { EnemyID = "mine_carrier", Name = "Mine Carrier", HP = 45f, Damage = 14f, Speed = 0.9f, XPDrop = 3, CoinDropChance = 0.2f, BehaviorType = NeonEnemyBehaviorType.MineCarrier, ProjectileType = "delayed_mine" });
            catalog.Enemies.Add(new NeonEnemyDef { EnemyID = "splitter_orb", Name = "Splitter Orb", HP = 36f, Damage = 9f, Speed = 1.4f, XPDrop = 2, CoinDropChance = 0.18f, BehaviorType = NeonEnemyBehaviorType.Splitter });
            catalog.Enemies.Add(new NeonEnemyDef { EnemyID = "elite_chaser", Name = "Elite Chaser", HP = 95f, Damage = 18f, Speed = 2.35f, XPDrop = 6, CoinDropChance = 0.35f, BehaviorType = NeonEnemyBehaviorType.Chaser, IsElite = true });
            catalog.Enemies.Add(new NeonEnemyDef { EnemyID = "elite_shooter", Name = "Elite Shooter", HP = 110f, Damage = 14f, Speed = 1.4f, XPDrop = 7, CoinDropChance = 0.35f, BehaviorType = NeonEnemyBehaviorType.Shooter, ProjectileType = "heavy_bullet", IsElite = true });
        }

        private static void AddWaves(NeonSkySurvivorsCatalog catalog)
        {
            catalog.Waves.Add(Wave("wave_0_1_teach", 0f, 60f, 0.9f, "", -1f, "chaser_drone", "fast_wing"));
            catalog.Waves.Add(Wave("wave_1_2_learn", 60f, 120f, 1.35f, "", -1f, "chaser_drone", "fast_wing", "shooter_drone"));
            catalog.Waves.Add(Wave("wave_2_3_warning", 120f, 180f, 1.9f, "WARNING: SKY REAPER APPROACHING", 170f, "chaser_drone", "fast_wing", "shooter_drone", "shield_drone"));
            catalog.Waves.Add(Wave("wave_3_4_boss_recovery", 180f, 240f, 1.25f, "", -1f, "shooter_drone", "fast_wing"));
            catalog.Waves.Add(Wave("wave_4_6_pressure", 240f, 360f, 2.55f, "NEON HYDRA APPROACHING", 350f, "shooter_drone", "shield_drone", "mine_carrier", "splitter_orb", "fast_wing"));
            catalog.Waves.Add(Wave("wave_6_7_boss_pressure", 360f, 420f, 1.7f, "", -1f, "shooter_drone", "shield_drone", "splitter_orb"));
            catalog.Waves.Add(Wave("wave_7_7_30_elites", 420f, 450f, 3.0f, "VIPER ACE INCOMING", 442f, "chaser_drone", "fast_wing", "mine_carrier", "elite_chaser"));
            catalog.Waves.Add(Wave("wave_7_30_8_45_chaos", 450f, 525f, 3.25f, "BOMBARDIER PRIME INCOMING", 517f, "fast_wing", "shooter_drone", "mine_carrier", "splitter_orb", "elite_chaser"));
            catalog.Waves.Add(Wave("wave_8_45_9_30_overload", 525f, 570f, 3.5f, "", -1f, "chaser_drone", "fast_wing", "shield_drone", "mine_carrier", "splitter_orb", "elite_chaser", "elite_shooter"));
            catalog.Waves.Add(Wave("wave_9_30_final_surge", 570f, 600f, 4.25f, "FINAL BOSS INCOMING", 590f, "chaser_drone", "fast_wing", "shooter_drone", "shield_drone", "mine_carrier", "splitter_orb", "elite_chaser", "elite_shooter"));
        }

        private static void AddBosses(NeonSkySurvivorsCatalog catalog)
        {
            catalog.Bosses.Add(new NeonBossDef { BossID = "sky_reaper", Name = "Sky Reaper", SpawnSecond = 180f, HP = 950f, ContactDamage = 18f, BulletDamage = 9f, WarningText = "WARNING: SKY REAPER APPROACHING", RewardCoinBonus = 35, RewardRarityHint = nameof(NeonEquipmentRarity.Uncommon), PhaseNotes = new List<string> { "Simple charge cone pressure", "Beatable with basic movement", "Reduced normal wave pressure during first boss" } });
            catalog.Bosses.Add(new NeonBossDef { BossID = "neon_hydra", Name = "Neon Hydra", SpawnSecond = 360f, HP = 2200f, ContactDamage = 24f, BulletDamage = 12f, WarningText = "NEON HYDRA APPROACHING", RewardCoinBonus = 55, RewardRarityHint = nameof(NeonEquipmentRarity.Rare), PhaseNotes = new List<string> { "Harder circular bullets", "Punishes bad movement", "Reduced normal wave pressure during second boss" } });
            catalog.Bosses.Add(new NeonBossDef { BossID = "viper_ace", Name = "Viper Ace", SpawnSecond = 450f, HP = 850f, ContactDamage = 20f, BulletDamage = 10f, WarningText = "VIPER ACE INCOMING", IsMiniBoss = true, RewardCoinBonus = 18, RewardRarityHint = nameof(NeonEquipmentRarity.Uncommon), PhaseNotes = new List<string> { "Fast mini-boss check at 7:30", "Cone shots without full boss durability" } });
            catalog.Bosses.Add(new NeonBossDef { BossID = "bombardier_prime", Name = "Bombardier Prime", SpawnSecond = 525f, HP = 1250f, ContactDamage = 22f, BulletDamage = 11f, WarningText = "BOMBARDIER PRIME INCOMING", IsMiniBoss = true, RewardCoinBonus = 24, RewardRarityHint = nameof(NeonEquipmentRarity.Rare), PhaseNotes = new List<string> { "Second mini-boss at 8:45", "Mine and radial pressure before final surge" } });
            catalog.Bosses.Add(new NeonBossDef { BossID = "eclipse_core", Name = "Eclipse Core", SpawnSecond = 600f, HP = 4800f, ContactDamage = 30f, BulletDamage = 15f, WarningText = "FINAL BOSS INCOMING", RewardCoinBonus = 90, RewardRarityHint = nameof(NeonEquipmentRarity.Rare), PhaseNotes = new List<string> { "Hardest encounter", "Bullet rings and laser arms", "Requires strong build and dodging" } });
        }

        private static NeonEquipmentItemDef Item(string id, string name, NeonEquipmentSlot slot, params NeonStatModifier[] stats)
        {
            return Item(id, name, slot, string.Empty, stats);
        }

        private static NeonEquipmentItemDef Item(string id, string name, NeonEquipmentSlot slot, string specialEffect, params NeonStatModifier[] stats)
        {
            return new NeonEquipmentItemDef
            {
                ItemID = id,
                Name = name,
                SlotType = slot,
                Rarity = NeonEquipmentRarity.Common,
                Level = 1,
                MaxLevel = 20,
                BaseStats = new List<NeonStatModifier>(stats),
                SpecialEffect = specialEffect,
                Icon = slot.ToString().ToLowerInvariant() + "_placeholder",
                UpgradeCoinCost = 20
            };
        }

        private static NeonStatModifier Stat(NeonStatType statType, float value, bool isPercent = false)
        {
            return new NeonStatModifier { StatType = statType, Value = value, IsPercent = isPercent };
        }

        private static NeonUpgradeDef Upgrade(string id, string name, NeonUpgradeCategory category, string description, string requiredPassive = "", string evolution = "", NeonStatModifier? perLevel = null)
        {
            var upgrade = new NeonUpgradeDef
            {
                Id = id,
                Name = name,
                Category = category,
                Description = description,
                RequiredPassiveId = requiredPassive,
                EvolutionId = evolution,
                MaxLevel = 5
            };

            if (perLevel != null)
            {
                upgrade.PerLevelStats.Add(perLevel);
            }

            return upgrade;
        }

        private static NeonWaveSegmentDef Wave(string id, float startSecond, float endSecond, float spawnRate, string warningText, float warningSecond, params string[] enemyIds)
        {
            return new NeonWaveSegmentDef
            {
                Id = id,
                StartSecond = startSecond,
                EndSecond = endSecond,
                SpawnRatePerSecond = spawnRate,
                WarningText = warningText,
                WarningSecond = warningSecond,
                EnemyIDs = new List<string>(enemyIds)
            };
        }
    }
}
