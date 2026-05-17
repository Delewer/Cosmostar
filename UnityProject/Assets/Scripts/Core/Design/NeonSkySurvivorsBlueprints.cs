using System.Collections.Generic;
using Cosmostar.Core.Models;

namespace Cosmostar.Core.Design
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
            catalog.Waves.Add(Wave("wave_0_1", 0f, 60f, 1.2f, "", -1f, "chaser_drone", "fast_wing"));
            catalog.Waves.Add(Wave("wave_1_2", 60f, 120f, 1.8f, "", -1f, "chaser_drone", "fast_wing", "shooter_drone"));
            catalog.Waves.Add(Wave("wave_2_3", 120f, 180f, 2.2f, "WARNING: SKY REAPER APPROACHING", 170f, "chaser_drone", "fast_wing", "shooter_drone", "shield_drone"));
            catalog.Waves.Add(Wave("wave_3_6", 180f, 360f, 2.6f, "", -1f, "shooter_drone", "shield_drone", "mine_carrier", "splitter_orb", "fast_wing"));
            catalog.Waves.Add(Wave("wave_6_10", 360f, 600f, 3.4f, "FINAL BOSS INCOMING", 590f, "chaser_drone", "fast_wing", "shooter_drone", "shield_drone", "mine_carrier", "splitter_orb", "elite_chaser", "elite_shooter"));
        }

        private static void AddBosses(NeonSkySurvivorsCatalog catalog)
        {
            catalog.Bosses.Add(new NeonBossDef { BossID = "sky_reaper", Name = "Sky Reaper", SpawnSecond = 180f, HP = 2500f, ContactDamage = 20f, BulletDamage = 10f, WarningText = "WARNING: SKY REAPER APPROACHING", PhaseNotes = new List<string> { "Charge toward player", "Shoot 5 bullets in a cone", "At 50% HP summon drones and shoot faster" } });
            catalog.Bosses.Add(new NeonBossDef { BossID = "neon_hydra", Name = "Neon Hydra", SpawnSecond = 360f, HP = 6000f, ContactDamage = 25f, BulletDamage = 12f, WarningText = "NEON HYDRA APPROACHING", PhaseNotes = new List<string> { "Shoot circular bullets", "Summon enemies", "At 50% HP split or increase attack speed" } });
            catalog.Bosses.Add(new NeonBossDef { BossID = "eclipse_core", Name = "Eclipse Core", SpawnSecond = 600f, HP = 12000f, ContactDamage = 30f, BulletDamage = 15f, WarningText = "FINAL BOSS INCOMING", PhaseNotes = new List<string> { "Fire bullet rings", "At 50% HP add rotating laser arms", "At 25% HP rage with faster attacks" } });
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

        private static NeonUpgradeDef Upgrade(string id, string name, NeonUpgradeCategory category, string description, string requiredPassive = "", string evolution = "", NeonStatModifier perLevel = null)
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
