using System.Collections.Generic;
using Cosmostar.Core.Models;

namespace Cosmostar.Core.Systems
{
    public enum CatalogValidationSeverity
    {
        Warning,
        Error
    }

    public sealed class CatalogValidationIssue
    {
        public CatalogValidationSeverity Severity;
        public string Code = string.Empty;
        public string Message = string.Empty;
    }

    public sealed class CatalogValidationReport
    {
        public List<CatalogValidationIssue> Issues = new List<CatalogValidationIssue>();

        public bool IsValid
        {
            get
            {
                for (var index = 0; index < Issues.Count; index++)
                {
                    if (Issues[index].Severity == CatalogValidationSeverity.Error)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }

    public sealed class CatalogValidationSystem
    {
        public CatalogValidationReport Validate(VerticalSliceCatalog catalog)
        {
            var report = new CatalogValidationReport();
            if (catalog == null)
            {
                AddError(report, "catalog.null", "Catalog is missing.");
                return report;
            }

            ValidateShip(catalog.Ship, report);
            ValidateWeapons(catalog, report);
            ValidateAbilities(catalog, report);
            ValidateUpgrades(catalog, report);
            ValidateModules(catalog, report);
            ValidateEnemies(catalog, report);
            ValidateWaves(catalog, report);
            ValidateBossPhases(catalog, report);
            ValidateMissions(catalog, report);
            ValidateUnlockTrack(catalog, report);

            return report;
        }

        private static void ValidateShip(ShipDef ship, CatalogValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(ship.Id))
            {
                AddError(report, "ship.id", "Ship must have an id.");
            }

            if (ship.BaseHull <= 0f)
            {
                AddError(report, "ship.hull", "Ship base hull must be greater than zero.");
            }

            if (ship.BaseShield < 0f)
            {
                AddError(report, "ship.shield", "Ship base shield cannot be negative.");
            }

            if (ship.MoveSpeed <= 0f)
            {
                AddError(report, "ship.speed", "Ship move speed must be greater than zero.");
            }
        }

        private static void ValidateWeapons(VerticalSliceCatalog catalog, CatalogValidationReport report)
        {
            if (catalog.Weapons.Count == 0)
            {
                AddError(report, "weapons.empty", "At least one weapon is required.");
                return;
            }

            var ids = new HashSet<string>();
            for (var index = 0; index < catalog.Weapons.Count; index++)
            {
                var weapon = catalog.Weapons[index];
                ValidateId("weapon", weapon.Id, ids, report);

                if (weapon.FireInterval <= 0f)
                {
                    AddError(report, "weapon.fire_interval", "Weapon '" + weapon.Id + "' fire interval must be greater than zero.");
                }

                if (weapon.ProjectileDamage <= 0f)
                {
                    AddError(report, "weapon.damage", "Weapon '" + weapon.Id + "' projectile damage must be greater than zero.");
                }

                if (weapon.ProjectileSpeed <= 0f)
                {
                    AddError(report, "weapon.speed", "Weapon '" + weapon.Id + "' projectile speed must be greater than zero.");
                }

                if (weapon.ProjectileCount <= 0)
                {
                    AddError(report, "weapon.projectile_count", "Weapon '" + weapon.Id + "' projectile count must be greater than zero.");
                }

                if (weapon.CritChance < 0f || weapon.CritChance > 1f)
                {
                    AddError(report, "weapon.crit", "Weapon '" + weapon.Id + "' crit chance must be between 0 and 1.");
                }
            }
        }

        private static void ValidateAbilities(VerticalSliceCatalog catalog, CatalogValidationReport report)
        {
            var ids = new HashSet<string>();
            for (var index = 0; index < catalog.Abilities.Count; index++)
            {
                var ability = catalog.Abilities[index];
                ValidateId("ability", ability.Id, ids, report);

                if (ability.Family == AbilityFamily.None)
                {
                    AddError(report, "ability.family", "Ability '" + ability.Id + "' must specify a family.");
                }
            }
        }

        private static void ValidateUpgrades(VerticalSliceCatalog catalog, CatalogValidationReport report)
        {
            var ids = new HashSet<string>();
            var abilityIds = BuildIdSet(catalog.Abilities);

            for (var index = 0; index < catalog.Upgrades.Count; index++)
            {
                var upgrade = catalog.Upgrades[index];
                ValidateId("upgrade", upgrade.Id, ids, report);

                if (upgrade.MaxStacks <= 0)
                {
                    AddError(report, "upgrade.max_stacks", "Upgrade '" + upgrade.Id + "' max stacks must be greater than zero.");
                }

                if (upgrade.Weight <= 0f)
                {
                    AddError(report, "upgrade.weight", "Upgrade '" + upgrade.Id + "' weight must be greater than zero.");
                }

                if (!string.IsNullOrWhiteSpace(upgrade.AbilityId) && !abilityIds.Contains(upgrade.AbilityId))
                {
                    AddError(report, "upgrade.ability_ref", "Upgrade '" + upgrade.Id + "' references missing ability '" + upgrade.AbilityId + "'.");
                }
            }
        }

        private static void ValidateModules(VerticalSliceCatalog catalog, CatalogValidationReport report)
        {
            var ids = new HashSet<string>();
            for (var index = 0; index < catalog.Modules.Count; index++)
            {
                var module = catalog.Modules[index];
                ValidateId("module", module.Id, ids, report);

                if (module.MaxLevel <= 0)
                {
                    AddError(report, "module.max_level", "Module '" + module.Id + "' max level must be greater than zero.");
                }

                if (module.UnlockCost < 0 || module.UpgradeCost < 0)
                {
                    AddError(report, "module.cost", "Module '" + module.Id + "' costs cannot be negative.");
                }

                if (module.MaxLevel > 1 && module.UpgradeCost <= 0)
                {
                    AddError(report, "module.upgrade_cost", "Module '" + module.Id + "' upgrade cost must be greater than zero when it has multiple levels.");
                }
            }
        }

        private static void ValidateEnemies(VerticalSliceCatalog catalog, CatalogValidationReport report)
        {
            if (catalog.Enemies.Count == 0)
            {
                AddError(report, "enemies.empty", "At least one enemy is required.");
                return;
            }

            var ids = new HashSet<string>();
            var archetypes = new HashSet<EnemyArchetype>();
            var hasBoss = false;

            for (var index = 0; index < catalog.Enemies.Count; index++)
            {
                var enemy = catalog.Enemies[index];
                ValidateId("enemy", enemy.Id, ids, report);
                archetypes.Add(enemy.Archetype);

                if (enemy.Hull <= 0f)
                {
                    AddError(report, "enemy.hull", "Enemy '" + enemy.Id + "' hull must be greater than zero.");
                }

                if (enemy.Speed < 0f)
                {
                    AddError(report, "enemy.speed", "Enemy '" + enemy.Id + "' speed cannot be negative.");
                }

                if (enemy.ContactDamage < 0f)
                {
                    AddError(report, "enemy.contact_damage", "Enemy '" + enemy.Id + "' contact damage cannot be negative.");
                }

                hasBoss = hasBoss || enemy.IsBoss;
            }

            if (!hasBoss)
            {
                AddWarning(report, "enemy.boss_missing", "Catalog has no boss enemy.");
            }
        }

        private static void ValidateWaves(VerticalSliceCatalog catalog, CatalogValidationReport report)
        {
            if (catalog.Waves.Count == 0)
            {
                AddError(report, "waves.empty", "At least one wave is required.");
                return;
            }

            var ids = new HashSet<string>();
            var archetypes = BuildArchetypeSet(catalog.Enemies);
            var previousEnd = 0f;
            var hasBossWave = false;

            for (var index = 0; index < catalog.Waves.Count; index++)
            {
                var wave = catalog.Waves[index];
                ValidateId("wave", wave.Id, ids, report);

                if (wave.StartSecond < 0f)
                {
                    AddError(report, "wave.start", "Wave '" + wave.Id + "' cannot start before zero.");
                }

                if (wave.EndSecond <= wave.StartSecond)
                {
                    AddError(report, "wave.duration", "Wave '" + wave.Id + "' must end after it starts.");
                }

                if (index > 0 && wave.StartSecond < previousEnd)
                {
                    AddError(report, "wave.overlap", "Wave '" + wave.Id + "' overlaps the previous wave.");
                }

                if (wave.SpawnRatePerSecond < 0f)
                {
                    AddError(report, "wave.spawn_rate", "Wave '" + wave.Id + "' spawn rate cannot be negative.");
                }

                if (wave.SpawnArchetypes.Count == 0)
                {
                    AddError(report, "wave.archetypes_empty", "Wave '" + wave.Id + "' must spawn at least one archetype.");
                }

                for (var archetypeIndex = 0; archetypeIndex < wave.SpawnArchetypes.Count; archetypeIndex++)
                {
                    var archetype = wave.SpawnArchetypes[archetypeIndex];
                    if (!archetypes.Contains(archetype))
                    {
                        AddError(report, "wave.enemy_ref", "Wave '" + wave.Id + "' references missing enemy archetype '" + archetype + "'.");
                    }
                }

                previousEnd = wave.EndSecond;
                hasBossWave = hasBossWave || wave.Phase == RunPhase.Boss;
            }

            if (!hasBossWave)
            {
                AddWarning(report, "wave.boss_missing", "Catalog has no boss wave.");
            }
        }

        private static void ValidateBossPhases(VerticalSliceCatalog catalog, CatalogValidationReport report)
        {
            if (catalog.BossPhases.Count == 0)
            {
                AddError(report, "boss_phase.empty", "At least one boss phase is required.");
                return;
            }

            var ids = new HashSet<string>();
            var phaseIndexes = new HashSet<int>();

            for (var index = 0; index < catalog.BossPhases.Count; index++)
            {
                var phase = catalog.BossPhases[index];
                ValidateId("boss_phase", phase.Id, ids, report);

                if (!phaseIndexes.Add(phase.PhaseIndex))
                {
                    AddError(report, "boss_phase.duplicate_index", "Boss phase index '" + phase.PhaseIndex + "' is duplicated.");
                }

                if (phase.PhaseIndex <= 0)
                {
                    AddError(report, "boss_phase.index", "Boss phase '" + phase.Id + "' index must be greater than zero.");
                }

                if (phase.TriggerHealthNormalized <= 0f || phase.TriggerHealthNormalized > 1f)
                {
                    AddError(report, "boss_phase.trigger", "Boss phase '" + phase.Id + "' trigger health must be between 0 and 1.");
                }

                if (phase.VolleyInterval <= 0f || phase.VolleyCount <= 0 || phase.ProjectileSpeed <= 0f)
                {
                    AddError(report, "boss_phase.volley", "Boss phase '" + phase.Id + "' volley values must be greater than zero.");
                }
            }
        }

        private static void ValidateMissions(VerticalSliceCatalog catalog, CatalogValidationReport report)
        {
            if (catalog.Missions.Count == 0)
            {
                AddError(report, "missions.empty", "At least one mission is required.");
                return;
            }

            var ids = new HashSet<string>();
            var hasDaily = false;
            for (var index = 0; index < catalog.Missions.Count; index++)
            {
                var mission = catalog.Missions[index];
                ValidateId("mission", mission.Id, ids, report);
                hasDaily = hasDaily || mission.DailyEligible;

                if (mission.DifficultyRating <= 0f)
                {
                    AddError(report, "mission.difficulty", "Mission '" + mission.Id + "' difficulty must be greater than zero.");
                }

                if (mission.TargetDurationSeconds <= 0f)
                {
                    AddError(report, "mission.duration", "Mission '" + mission.Id + "' target duration must be greater than zero.");
                }

                if ((mission.ObjectiveKind == MissionObjectiveKind.DefeatEnemies || mission.ObjectiveKind == MissionObjectiveKind.DefeatBoss || mission.ObjectiveKind == MissionObjectiveKind.PreserveShield) && mission.TargetValue <= 0)
                {
                    AddError(report, "mission.target", "Mission '" + mission.Id + "' target value must be greater than zero.");
                }

                if (mission.RequiredShieldRatio < 0f || mission.RequiredShieldRatio > 1f)
                {
                    AddError(report, "mission.shield_ratio", "Mission '" + mission.Id + "' shield ratio must be between 0 and 1.");
                }
            }

            if (!hasDaily)
            {
                AddError(report, "missions.daily_missing", "At least one mission must be daily eligible.");
            }
        }

        private static void ValidateUnlockTrack(VerticalSliceCatalog catalog, CatalogValidationReport report)
        {
            var ids = new HashSet<string>();
            var abilityIds = BuildIdSet(catalog.Abilities);
            var previousXp = -1;

            for (var index = 0; index < catalog.UnlockTrack.Count; index++)
            {
                var entry = catalog.UnlockTrack[index];
                ValidateId("unlock_track", entry.Id, ids, report);

                if (entry.RequiredXp <= previousXp)
                {
                    AddError(report, "unlock_track.order", "Unlock track entry '" + entry.Id + "' must require more XP than the previous entry.");
                }

                if (entry.ModuleShards < 0)
                {
                    AddError(report, "unlock_track.shards", "Unlock track entry '" + entry.Id + "' module shards cannot be negative.");
                }

                if (!string.IsNullOrWhiteSpace(entry.AbilityUnlockId) && !abilityIds.Contains(entry.AbilityUnlockId))
                {
                    AddError(report, "unlock_track.ability_ref", "Unlock track entry '" + entry.Id + "' references missing ability '" + entry.AbilityUnlockId + "'.");
                }

                previousXp = entry.RequiredXp;
            }
        }

        private static void ValidateId(string contentType, string id, HashSet<string> ids, CatalogValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                AddError(report, contentType + ".id", contentType + " id is required.");
                return;
            }

            if (!ids.Add(id))
            {
                AddError(report, contentType + ".duplicate_id", contentType + " id '" + id + "' is duplicated.");
            }
        }

        private static HashSet<string> BuildIdSet(List<AbilityDef> abilities)
        {
            var ids = new HashSet<string>();
            for (var index = 0; index < abilities.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(abilities[index].Id))
                {
                    ids.Add(abilities[index].Id);
                }
            }

            return ids;
        }

        private static HashSet<EnemyArchetype> BuildArchetypeSet(List<EnemyDef> enemies)
        {
            var archetypes = new HashSet<EnemyArchetype>();
            for (var index = 0; index < enemies.Count; index++)
            {
                archetypes.Add(enemies[index].Archetype);
            }

            return archetypes;
        }

        private static void AddError(CatalogValidationReport report, string code, string message)
        {
            AddIssue(report, CatalogValidationSeverity.Error, code, message);
        }

        private static void AddWarning(CatalogValidationReport report, string code, string message)
        {
            AddIssue(report, CatalogValidationSeverity.Warning, code, message);
        }

        private static void AddIssue(CatalogValidationReport report, CatalogValidationSeverity severity, string code, string message)
        {
            report.Issues.Add(new CatalogValidationIssue
            {
                Severity = severity,
                Code = code,
                Message = message
            });
        }
    }
}
