using System.Collections.Generic;
using Cosmostar.Core.Models;

namespace Cosmostar.Core.Design
{
    public static class VerticalSliceBlueprints
    {
        public static VerticalSliceCatalog CreateDefaultCatalog()
        {
            var catalog = new VerticalSliceCatalog();

            catalog.Ship = new ShipDef
            {
                Id = "starling_mk1",
                DisplayName = "Starling Mk-I",
                BaseHull = 90f,
                BaseShield = 70f,
                MoveSpeed = 0.65f,
                StartingWeaponFamily = WeaponFamily.Pulse
            };

            catalog.Weapons.Add(new WeaponDef
            {
                Id = "pulse_array",
                DisplayName = "Pulse Array",
                Family = WeaponFamily.Pulse,
                FireInterval = 0.18f,
                ProjectileDamage = 16f,
                ProjectileSpeed = 1.4f,
                ProjectileCount = 1,
                SpreadDegrees = 8f,
                CritChance = 0.08f
            });
            catalog.Weapons.Add(new WeaponDef
            {
                Id = "arc_splitter",
                DisplayName = "Arc Splitter",
                Family = WeaponFamily.Arc,
                FireInterval = 0.32f,
                ProjectileDamage = 22f,
                ProjectileSpeed = 1.15f,
                ProjectileCount = 2,
                SpreadDegrees = 22f,
                CritChance = 0.04f
            });
            catalog.Weapons.Add(new WeaponDef
            {
                Id = "lance_rail",
                DisplayName = "Lance Rail",
                Family = WeaponFamily.Lance,
                FireInterval = 0.55f,
                ProjectileDamage = 38f,
                ProjectileSpeed = 1.9f,
                ProjectileCount = 1,
                SpreadDegrees = 0f,
                CritChance = 0.12f
            });

            catalog.Abilities.Add(new AbilityDef
            {
                Id = "cryo_wake",
                DisplayName = "Cryo Wake",
                Family = AbilityFamily.CryoWake,
                Description = "Shots can freeze enemies and slow the lane.",
                Magnitude = 0.18f,
                DurationSeconds = 1.5f
            });
            catalog.Abilities.Add(new AbilityDef
            {
                Id = "chain_surge",
                DisplayName = "Chain Surge",
                Family = AbilityFamily.ChainSurge,
                Description = "Critical hits arc to nearby targets.",
                Magnitude = 0.22f,
                DurationSeconds = 0f
            });
            catalog.Abilities.Add(new AbilityDef
            {
                Id = "drone_wing",
                DisplayName = "Drone Wing",
                Family = AbilityFamily.DroneWing,
                Description = "Adds autonomous flanking drones.",
                Magnitude = 1f,
                DurationSeconds = 0f
            });
            catalog.Abilities.Add(new AbilityDef
            {
                Id = "overclock_pulse",
                DisplayName = "Overclock Pulse",
                Family = AbilityFamily.OverclockPulse,
                Description = "Periodic shockwave burst when your reactor peaks.",
                Magnitude = 26f,
                DurationSeconds = 4.5f
            });

            catalog.Upgrades.Add(new UpgradeDef { Id = "plasma_focus", DisplayName = "Plasma Focus", Description = "Increase shot damage.", MaxStacks = 5, Weight = 1.4f, EffectType = UpgradeEffectType.Damage, Magnitude = 0.14f });
            catalog.Upgrades.Add(new UpgradeDef { Id = "rapid_cycling", DisplayName = "Rapid Cycling", Description = "Increase fire rate.", MaxStacks = 5, Weight = 1.4f, EffectType = UpgradeEffectType.FireRate, Magnitude = 0.12f });
            catalog.Upgrades.Add(new UpgradeDef { Id = "split_vector", DisplayName = "Split Vector", Description = "Add one more projectile to each volley.", MaxStacks = 3, Weight = 1f, EffectType = UpgradeEffectType.ProjectileCount, Magnitude = 1f });
            catalog.Upgrades.Add(new UpgradeDef { Id = "shield_weave", DisplayName = "Shield Weave", Description = "Increase max shield.", MaxStacks = 4, Weight = 1.1f, EffectType = UpgradeEffectType.MaxShield, Magnitude = 12f });
            catalog.Upgrades.Add(new UpgradeDef { Id = "emergency_patch", DisplayName = "Emergency Patch", Description = "Restore shield immediately.", MaxStacks = 3, Weight = 0.8f, EffectType = UpgradeEffectType.RestoreShield, Magnitude = 18f });
            catalog.Upgrades.Add(new UpgradeDef { Id = "afterburner", DisplayName = "Afterburner", Description = "Increase move speed.", MaxStacks = 4, Weight = 1f, EffectType = UpgradeEffectType.MoveSpeed, Magnitude = 0.08f });
            catalog.Upgrades.Add(new UpgradeDef { Id = "magnetic_sweep", DisplayName = "Magnetic Sweep", Description = "Extend pickup radius.", MaxStacks = 4, Weight = 0.9f, EffectType = UpgradeEffectType.PickupRadius, Magnitude = 0.05f });
            catalog.Upgrades.Add(new UpgradeDef { Id = "piercing_array", DisplayName = "Piercing Array", Description = "Shots pass through additional targets.", MaxStacks = 2, Weight = 0.7f, EffectType = UpgradeEffectType.Piercing, Magnitude = 1f });
            catalog.Upgrades.Add(new UpgradeDef { Id = "cryo_wake_upgrade", DisplayName = "Cryo Wake", Description = "Freeze chance on hit.", MaxStacks = 3, Weight = 0.7f, EffectType = UpgradeEffectType.FrostChance, Magnitude = 0.1f, AbilityFamily = AbilityFamily.CryoWake, AbilityId = "cryo_wake" });
            catalog.Upgrades.Add(new UpgradeDef { Id = "chain_surge_upgrade", DisplayName = "Chain Surge", Description = "Chain chance on critical burst.", MaxStacks = 3, Weight = 0.7f, EffectType = UpgradeEffectType.ChainChance, Magnitude = 0.1f, AbilityFamily = AbilityFamily.ChainSurge, AbilityId = "chain_surge" });
            catalog.Upgrades.Add(new UpgradeDef { Id = "drone_wing_upgrade", DisplayName = "Drone Wing", Description = "Add a drone companion.", MaxStacks = 2, Weight = 0.6f, EffectType = UpgradeEffectType.DroneCompanion, Magnitude = 1f, AbilityFamily = AbilityFamily.DroneWing, AbilityId = "drone_wing" });
            catalog.Upgrades.Add(new UpgradeDef { Id = "overclock_pulse_upgrade", DisplayName = "Overclock Pulse", Description = "Emit periodic reactor bursts.", MaxStacks = 2, Weight = 0.5f, EffectType = UpgradeEffectType.OverclockBurst, Magnitude = 24f, AbilityFamily = AbilityFamily.OverclockPulse, AbilityId = "overclock_pulse" });

            catalog.Modules.Add(new ModuleDef { Id = "hull_plating", DisplayName = "Hull Plating", Description = "Adds hull and shield buffer.", EffectType = ModuleEffectType.HullPlating, Magnitude = 12f, MaxLevel = 5, UnlockCost = 0, UpgradeCost = 45 });
            catalog.Modules.Add(new ModuleDef { Id = "reactor_core", DisplayName = "Reactor Core", Description = "Improves baseline damage.", EffectType = ModuleEffectType.ReactorCore, Magnitude = 0.08f, MaxLevel = 5, UnlockCost = 160, UpgradeCost = 65 });
            catalog.Modules.Add(new ModuleDef { Id = "thruster_mesh", DisplayName = "Thruster Mesh", Description = "Improves move speed.", EffectType = ModuleEffectType.ThrusterMesh, Magnitude = 0.07f, MaxLevel = 5, UnlockCost = 180, UpgradeCost = 70 });
            catalog.Modules.Add(new ModuleDef { Id = "capacitor_lattice", DisplayName = "Capacitor Lattice", Description = "Improves fire rate.", EffectType = ModuleEffectType.CapacitorLattice, Magnitude = 0.06f, MaxLevel = 5, UnlockCost = 210, UpgradeCost = 80 });
            catalog.Modules.Add(new ModuleDef { Id = "salvage_magnet", DisplayName = "Salvage Magnet", Description = "Improves pickup radius.", EffectType = ModuleEffectType.SalvageMagnet, Magnitude = 0.05f, MaxLevel = 5, UnlockCost = 190, UpgradeCost = 75 });
            catalog.Modules.Add(new ModuleDef { Id = "tactical_reroll", DisplayName = "Tactical Reroll", Description = "Adds a free reroll each run.", EffectType = ModuleEffectType.TacticalReroll, Magnitude = 1f, MaxLevel = 3, UnlockCost = 220, UpgradeCost = 120 });
            catalog.Modules.Add(new ModuleDef { Id = "backup_spark", DisplayName = "Backup Spark", Description = "Adds a revive charge.", EffectType = ModuleEffectType.BackupSpark, Magnitude = 1f, MaxLevel = 2, UnlockCost = 260, UpgradeCost = 140 });
            catalog.Modules.Add(new ModuleDef { Id = "credit_cache", DisplayName = "Credit Cache", Description = "Improves payout after each run.", EffectType = ModuleEffectType.CreditCache, Magnitude = 0.08f, MaxLevel = 5, UnlockCost = 200, UpgradeCost = 85 });

            catalog.Enemies.Add(new EnemyDef { Id = "scout", DisplayName = "Scout", Archetype = EnemyArchetype.Scout, Hull = 26f, Speed = 0.14f, ContactDamage = 9f, FireInterval = 0f, ScoreValue = 1f });
            catalog.Enemies.Add(new EnemyDef { Id = "miner", DisplayName = "Miner", Archetype = EnemyArchetype.Miner, Hull = 34f, Speed = 0.11f, ContactDamage = 11f, FireInterval = 0f, ScoreValue = 1.3f });
            catalog.Enemies.Add(new EnemyDef { Id = "rammer", DisplayName = "Rammer", Archetype = EnemyArchetype.Rammer, Hull = 48f, Speed = 0.19f, ContactDamage = 16f, FireInterval = 0f, ScoreValue = 1.6f });
            catalog.Enemies.Add(new EnemyDef { Id = "shard_caster", DisplayName = "Shard Caster", Archetype = EnemyArchetype.ShardCaster, Hull = 40f, Speed = 0.09f, ContactDamage = 10f, FireInterval = 2.4f, ScoreValue = 1.8f });
            catalog.Enemies.Add(new EnemyDef { Id = "elite_warden", DisplayName = "Elite Warden", Archetype = EnemyArchetype.EliteWarden, Hull = 220f, Speed = 0.1f, ContactDamage = 20f, FireInterval = 1.9f, ScoreValue = 8f });
            catalog.Enemies.Add(new EnemyDef { Id = "null_sovereign", DisplayName = "Null Sovereign", Archetype = EnemyArchetype.NullSovereign, Hull = 780f, Speed = 0.05f, ContactDamage = 26f, FireInterval = 1.4f, ScoreValue = 30f, IsBoss = true });

            catalog.Waves.Add(new WaveDef { Id = "intro", DisplayName = "Breach Corridor", Phase = RunPhase.Intro, StartSecond = 0f, EndSecond = 65f, SpawnRatePerSecond = 1.4f, GrantsUpgradeDraft = true, SpawnArchetypes = new List<EnemyArchetype> { EnemyArchetype.Scout, EnemyArchetype.Miner } });
            catalog.Waves.Add(new WaveDef { Id = "escalation", DisplayName = "Flare Net", Phase = RunPhase.Escalation, StartSecond = 65f, EndSecond = 145f, SpawnRatePerSecond = 1.8f, GrantsUpgradeDraft = true, SpawnArchetypes = new List<EnemyArchetype> { EnemyArchetype.Scout, EnemyArchetype.Rammer, EnemyArchetype.ShardCaster } });
            catalog.Waves.Add(new WaveDef { Id = "elite", DisplayName = "Warden Surge", Phase = RunPhase.Elite, StartSecond = 145f, EndSecond = 215f, SpawnRatePerSecond = 1.5f, GrantsUpgradeDraft = true, SpawnArchetypes = new List<EnemyArchetype> { EnemyArchetype.Rammer, EnemyArchetype.ShardCaster, EnemyArchetype.EliteWarden } });
            catalog.Waves.Add(new WaveDef { Id = "boss", DisplayName = "Sovereign Gate", Phase = RunPhase.Boss, StartSecond = 215f, EndSecond = 320f, SpawnRatePerSecond = 0.2f, GrantsUpgradeDraft = false, SpawnArchetypes = new List<EnemyArchetype> { EnemyArchetype.NullSovereign } });

            catalog.BossPhases.Add(new BossPhaseDef { Id = "boss_phase_1", PhaseIndex = 1, TriggerHealthNormalized = 1f, VolleyInterval = 1.3f, VolleyCount = 3, ProjectileSpeed = 0.18f, ArenaPulseDamage = 8f });
            catalog.BossPhases.Add(new BossPhaseDef { Id = "boss_phase_2", PhaseIndex = 2, TriggerHealthNormalized = 0.67f, VolleyInterval = 1f, VolleyCount = 4, ProjectileSpeed = 0.22f, ArenaPulseDamage = 10f });
            catalog.BossPhases.Add(new BossPhaseDef { Id = "boss_phase_3", PhaseIndex = 3, TriggerHealthNormalized = 0.34f, VolleyInterval = 0.75f, VolleyCount = 6, ProjectileSpeed = 0.26f, ArenaPulseDamage = 12f });

            catalog.Missions.Add(new MissionDef { Id = "survive_four", DisplayName = "Break the Screen", Description = "Survive the full corridor for four minutes.", ObjectiveKind = MissionObjectiveKind.SurviveTime, TargetValue = 0, TargetDurationSeconds = 240f, ModifierText = "Dense scout traffic", DifficultyRating = 1f, Reward = new RewardTable { SoftCurrency = 55, ModuleShards = 8, UnlockTrackXp = 18 } });
            catalog.Missions.Add(new MissionDef { Id = "kill_eighty", DisplayName = "Clean Sweep", Description = "Destroy 80 drones before extraction.", ObjectiveKind = MissionObjectiveKind.DefeatEnemies, TargetValue = 80, TargetDurationSeconds = 240f, ModifierText = "Bonus salvage on scouts", DifficultyRating = 1.1f, Reward = new RewardTable { SoftCurrency = 60, ModuleShards = 9, UnlockTrackXp = 20 } });
            catalog.Missions.Add(new MissionDef { Id = "boss_clear", DisplayName = "Open the Gate", Description = "Reach and destroy the Null Sovereign.", ObjectiveKind = MissionObjectiveKind.DefeatBoss, TargetValue = 1, TargetDurationSeconds = 300f, ModifierText = "Boss appears earlier", DifficultyRating = 1.4f, Reward = new RewardTable { SoftCurrency = 72, ModuleShards = 12, UnlockTrackXp = 28 } });
            catalog.Missions.Add(new MissionDef { Id = "shield_clear", DisplayName = "Glass Pilot", Description = "Beat the boss with at least 40% shield remaining.", ObjectiveKind = MissionObjectiveKind.PreserveShield, TargetValue = 1, TargetDurationSeconds = 300f, RequiredShieldRatio = 0.4f, ModifierText = "Shield pickups reduced", DifficultyRating = 1.55f, Reward = new RewardTable { SoftCurrency = 82, ModuleShards = 14, UnlockTrackXp = 32 } });
            catalog.Missions.Add(new MissionDef { Id = "kill_one_twenty", DisplayName = "Neon Harvest", Description = "Destroy 120 targets under pressure.", ObjectiveKind = MissionObjectiveKind.DefeatEnemies, TargetValue = 120, TargetDurationSeconds = 270f, ModifierText = "Faster enemy spawns", DifficultyRating = 1.45f, Reward = new RewardTable { SoftCurrency = 78, ModuleShards = 13, UnlockTrackXp = 30 } });
            catalog.Missions.Add(new MissionDef { Id = "survive_hard", DisplayName = "Hard Vacuum", Description = "Last three minutes with no revive safety net.", ObjectiveKind = MissionObjectiveKind.SurviveTime, TargetValue = 0, TargetDurationSeconds = 180f, ModifierText = "Rammers spawn early", DifficultyRating = 1.25f, Reward = new RewardTable { SoftCurrency = 66, ModuleShards = 10, UnlockTrackXp = 24 } });

            catalog.UnlockTrack.Add(new UnlockTrackEntry { Id = "track_1", RequiredXp = 20, RewardLabel = "Cryo Wake unlocked", AbilityUnlockId = "cryo_wake", ModuleShards = 6 });
            catalog.UnlockTrack.Add(new UnlockTrackEntry { Id = "track_2", RequiredXp = 50, RewardLabel = "Chain Surge unlocked", AbilityUnlockId = "chain_surge", ModuleShards = 8 });
            catalog.UnlockTrack.Add(new UnlockTrackEntry { Id = "track_3", RequiredXp = 90, RewardLabel = "Drone Wing unlocked", AbilityUnlockId = "drone_wing", ModuleShards = 10 });
            catalog.UnlockTrack.Add(new UnlockTrackEntry { Id = "track_4", RequiredXp = 140, RewardLabel = "Overclock Pulse unlocked", AbilityUnlockId = "overclock_pulse", ModuleShards = 14 });

            return catalog;
        }
    }
}
