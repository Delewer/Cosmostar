#nullable enable annotations

using System;
using System.Collections.Generic;
using System.Linq;
using NeonSkySurvivors.Core.Models;

namespace NeonSkySurvivors.Core.Systems
{
    public sealed class NeonRunGameplaySystem
    {
        private readonly NeonRunTimelineSystem _timeline = new NeonRunTimelineSystem();
        private readonly Random _random;
        private float _spawnAccumulator;

        public NeonRunGameplaySystem(int seed = 1337)
        {
            _random = new Random(seed);
        }

        public NeonRunState StartRun(NeonSaveProfile profile, NeonSkySurvivorsCatalog catalog)
        {
            var equipmentSystem = new NeonEquipmentSystem();
            equipmentSystem.EnsureStartingProfile(profile, catalog);

            var run = new NeonRunState();
            run.Player.Stats = equipmentSystem.CalculateStats(profile, catalog);
            run.Player.Stats.CurrentHP = run.Player.Stats.MaxHP;
            run.Player.Position = NeonVector2.Zero;
            run.Player.MovementTarget = NeonVector2.Zero;
            run.Player.LastMoveDirection = NeonVector2.Up;
            return run;
        }

        public void SetMovementTarget(NeonRunState run, NeonVector2 target)
        {
            run.Player.MovementTarget = ClampToArena(target);
        }

        public bool TryDash(NeonRunState run)
        {
            if (run.Status != NeonRunStatus.Running || run.Player.DashCooldownRemaining > 0f)
            {
                return false;
            }

            var direction = run.Player.LastMoveDirection.SqrMagnitude > 0.0001f ? run.Player.LastMoveDirection.Normalized : NeonVector2.Up;
            var start = run.Player.Position;
            var distance = run.Player.Stats.DashDistance / 10f;
            run.Player.Position = ClampToArena(run.Player.Position + direction * distance);
            run.Player.DashCooldownRemaining = Math.Max(0.6f, run.Player.Stats.DashCooldown);
            run.Player.InvulnerabilityRemaining = Math.Max(run.Player.InvulnerabilityRemaining, 0.28f);
            run.DashTrails.Add(new NeonDashTrailState
            {
                Start = start,
                End = run.Player.Position,
                DamagePerSecond = run.Player.Stats.AttackDamage * GetTrailDamageMultiplier(run),
                RemainingLifetime = HasUpgrade(run, "longer_trail") ? 2.2f : 1.5f,
                ExplodesOnExpire = HasUpgrade(run, "trail_explosion")
            });

            return true;
        }

        public void Tick(NeonRunState run, NeonSkySurvivorsCatalog catalog, float deltaTime)
        {
            if (run.Status != NeonRunStatus.Running)
            {
                return;
            }

            var previousElapsedSeconds = run.ElapsedSeconds;
            run.ElapsedSeconds += Math.Max(0f, deltaTime);
            TickTimers(run, deltaTime);
            TickMovement(run, deltaTime);
            TickTimeline(run, catalog, previousElapsedSeconds);
            TickAutoFire(run, deltaTime);
            TickProjectiles(run, deltaTime);
            TickEnemyMovementAndContact(run, deltaTime);
            TickDashTrails(run, deltaTime);
            TickXpCollection(run, catalog, deltaTime);
            CleanupDefeatedEnemies(run, catalog);
            TrySpawnWaveEnemies(run, catalog, deltaTime);
        }

        public bool ApplyUpgradeChoice(NeonRunState run, NeonUpgradeDef upgrade)
        {
            if (run.Status != NeonRunStatus.LevelUpDraft || upgrade == null || !run.DraftChoices.Contains(upgrade))
            {
                return false;
            }

            var currentLevel = run.Build.GetLevel(upgrade.Id);
            if (currentLevel >= upgrade.MaxLevel)
            {
                return false;
            }

            run.Build.UpgradeLevels[upgrade.Id] = currentLevel + 1;
            ApplyUpgradeStats(run, upgrade);
            TryEvolve(run, upgrade);
            run.DraftChoices.Clear();
            run.Status = NeonRunStatus.Running;
            return true;
        }

        private void TickTimers(NeonRunState run, float deltaTime)
        {
            run.Player.DashCooldownRemaining = Math.Max(0f, run.Player.DashCooldownRemaining - deltaTime);
            run.Player.InvulnerabilityRemaining = Math.Max(0f, run.Player.InvulnerabilityRemaining - deltaTime);
            run.Player.WeaponCooldownRemaining = Math.Max(0f, run.Player.WeaponCooldownRemaining - deltaTime);
        }

        private void TickMovement(NeonRunState run, float deltaTime)
        {
            var delta = run.Player.MovementTarget - run.Player.Position;
            if (delta.SqrMagnitude > 0.0001f)
            {
                run.Player.LastMoveDirection = delta.Normalized;
            }

            var unitsPerSecond = run.Player.Stats.MovementSpeed / 10f;
            run.Player.Position = NeonVector2.MoveTowards(run.Player.Position, run.Player.MovementTarget, unitsPerSecond * deltaTime);
        }

        private void TickTimeline(NeonRunState run, NeonSkySurvivorsCatalog catalog, float previousElapsedSeconds)
        {
            var warning = _timeline.GetWarning(catalog, previousElapsedSeconds, run.ElapsedSeconds);
            if (!string.IsNullOrWhiteSpace(warning))
            {
                run.LastWarning = warning;
            }

            foreach (var boss in _timeline.GetBossesDue(catalog, previousElapsedSeconds, run.ElapsedSeconds, run.SpawnedBossIDs))
            {
                SpawnBoss(run, boss);
                run.SpawnedBossIDs.Add(boss.BossID);
                run.LastWarning = boss.WarningText;
            }
        }

        private void TickAutoFire(NeonRunState run, float deltaTime)
        {
            if (run.Player.WeaponCooldownRemaining > 0f || run.Enemies.Count == 0)
            {
                return;
            }

            var target = FindNearestEnemy(run, run.Player.Position);
            if (target == null)
            {
                return;
            }

            var direction = (target.Position - run.Player.Position).Normalized;
            var isCritical = _random.NextDouble() <= run.Player.Stats.CriticalChance;
            var damage = run.Player.Stats.AttackDamage * (isCritical ? run.Player.Stats.CriticalDamage : 1f);
            run.Projectiles.Add(new NeonRunProjectileState
            {
                Position = run.Player.Position,
                Velocity = direction * 1.8f,
                Damage = damage,
                Radius = isCritical ? 0.24f : 0.18f,
                RemainingPierce = HasUpgrade(run, "plasma_blaster") && run.Build.GetLevel("plasma_blaster") >= 5 ? 1 : 0,
                FromPlayer = true
            });

            run.Player.WeaponCooldownRemaining = 1f / Math.Max(0.1f, run.Player.Stats.FireRate);
        }

        private void TickProjectiles(NeonRunState run, float deltaTime)
        {
            for (var projectileIndex = run.Projectiles.Count - 1; projectileIndex >= 0; projectileIndex--)
            {
                var projectile = run.Projectiles[projectileIndex];
                projectile.Position += projectile.Velocity * deltaTime;
                projectile.RemainingLife -= deltaTime;

                if (projectile.FromPlayer && ResolvePlayerProjectileHits(run, projectile, projectileIndex))
                {
                    continue;
                }

                if (projectile.RemainingLife <= 0f || Math.Abs(projectile.Position.X) > 1.2f || Math.Abs(projectile.Position.Y) > 1.2f)
                {
                    run.Projectiles.RemoveAt(projectileIndex);
                }
            }
        }

        private bool ResolvePlayerProjectileHits(NeonRunState run, NeonRunProjectileState projectile, int projectileIndex)
        {
            for (var enemyIndex = run.Enemies.Count - 1; enemyIndex >= 0; enemyIndex--)
            {
                var enemy = run.Enemies[enemyIndex];
                if (NeonVector2.Distance(projectile.Position, enemy.Position) > projectile.Radius + 0.12f)
                {
                    continue;
                }

                enemy.HP -= projectile.Damage;
                if (projectile.RemainingPierce > 0)
                {
                    projectile.RemainingPierce -= 1;
                    continue;
                }

                run.Projectiles.RemoveAt(projectileIndex);
                return true;
            }

            return false;
        }

        private void TickEnemyMovementAndContact(NeonRunState run, float deltaTime)
        {
            foreach (var enemy in run.Enemies)
            {
                var direction = (run.Player.Position - enemy.Position).Normalized;
                enemy.Position += direction * (enemy.Speed / 10f) * deltaTime;
                if (NeonVector2.Distance(enemy.Position, run.Player.Position) <= 0.16f)
                {
                    DamagePlayer(run, enemy.ContactDamage);
                }
            }
        }

        private void TickDashTrails(NeonRunState run, float deltaTime)
        {
            for (var trailIndex = run.DashTrails.Count - 1; trailIndex >= 0; trailIndex--)
            {
                var trail = run.DashTrails[trailIndex];
                foreach (var enemy in run.Enemies)
                {
                    if (DistanceToSegment(enemy.Position, trail.Start, trail.End) <= 0.13f)
                    {
                        enemy.HP -= trail.DamagePerSecond * deltaTime;
                    }
                }

                trail.RemainingLifetime -= deltaTime;
                if (trail.RemainingLifetime > 0f)
                {
                    continue;
                }

                if (trail.ExplodesOnExpire)
                {
                    foreach (var enemy in run.Enemies)
                    {
                        if (NeonVector2.Distance(enemy.Position, trail.End) <= 0.28f)
                        {
                            enemy.HP -= run.Player.Stats.AttackDamage * 1.5f;
                        }
                    }
                }

                run.DashTrails.RemoveAt(trailIndex);
            }
        }

        private void TickXpCollection(NeonRunState run, NeonSkySurvivorsCatalog catalog, float deltaTime)
        {
            for (var shardIndex = run.XpShards.Count - 1; shardIndex >= 0; shardIndex--)
            {
                var shard = run.XpShards[shardIndex];
                var pickupRange = Math.Max(0.08f, run.Player.Stats.MagnetRange / 20f);
                if (NeonVector2.Distance(shard.Position, run.Player.Position) > pickupRange)
                {
                    var direction = (run.Player.Position - shard.Position).Normalized;
                    shard.Position += direction * pickupRange * deltaTime;
                    continue;
                }

                run.Player.XP += shard.XPValue * run.Player.Stats.XPModifier;
                run.XpShards.RemoveAt(shardIndex);
                if (run.Player.XP >= run.Player.XPToNextLevel)
                {
                    OpenLevelUpDraft(run, catalog);
                    return;
                }
            }
        }

        private void CleanupDefeatedEnemies(NeonRunState run, NeonSkySurvivorsCatalog catalog)
        {
            for (var enemyIndex = run.Enemies.Count - 1; enemyIndex >= 0; enemyIndex--)
            {
                var enemy = run.Enemies[enemyIndex];
                if (enemy.HP > 0f)
                {
                    continue;
                }

                run.Enemies.RemoveAt(enemyIndex);
                run.EnemiesKilled += 1;
                run.XpShards.Add(new NeonXpShardState { Position = enemy.Position, XPValue = enemy.XPDrop });
                if (_random.NextDouble() <= 0.2f * run.Player.Stats.CoinBonus)
                {
                    run.Player.CoinsCollected += 1;
                }

                if (!enemy.IsBoss)
                {
                    continue;
                }

                if (enemy.IsMiniBoss)
                {
                    run.MiniBossesKilled += 1;
                }
                else
                {
                    run.BossesKilled += 1;
                }

                if (catalog.Bosses.OrderByDescending(boss => boss.SpawnSecond).First().BossID == enemy.EnemyID)
                {
                    run.Status = NeonRunStatus.Victory;
                }
            }
        }

        private void TrySpawnWaveEnemies(NeonRunState run, NeonSkySurvivorsCatalog catalog, float deltaTime)
        {
            var wave = _timeline.GetActiveWave(catalog, run.ElapsedSeconds);
            if (wave == null || wave.EnemyIDs.Count == 0)
            {
                return;
            }

            _spawnAccumulator += wave.SpawnRatePerSecond * deltaTime;
            while (_spawnAccumulator >= 1f)
            {
                _spawnAccumulator -= 1f;
                var enemyId = wave.EnemyIDs[_random.Next(wave.EnemyIDs.Count)];
                SpawnEnemy(run, catalog, enemyId);
            }
        }

        private void SpawnEnemy(NeonRunState run, NeonSkySurvivorsCatalog catalog, string enemyId)
        {
            var definition = catalog.Enemies.FirstOrDefault(enemy => enemy.EnemyID == enemyId);
            if (definition == null)
            {
                return;
            }

            var angle = _random.NextDouble() * Math.PI * 2d;
            var spawnRadius = 1.05f;
            var position = new NeonVector2((float)Math.Cos(angle) * spawnRadius, (float)Math.Sin(angle) * spawnRadius);
            run.Enemies.Add(new NeonRunEnemyState
            {
                EnemyID = definition.EnemyID,
                Position = position,
                HP = definition.HP,
                MaxHP = definition.HP,
                ContactDamage = definition.Damage,
                Speed = definition.Speed,
                XPDrop = definition.XPDrop,
                IsBoss = false
            });
        }

        private void SpawnBoss(NeonRunState run, NeonBossDef boss)
        {
            run.Enemies.Add(new NeonRunEnemyState
            {
                EnemyID = boss.BossID,
                Position = new NeonVector2(0f, 0.82f),
                HP = boss.HP,
                MaxHP = boss.HP,
                ContactDamage = boss.ContactDamage,
                Speed = 0.5f,
                XPDrop = 20,
                IsBoss = true,
                IsMiniBoss = boss.IsMiniBoss
            });
        }

        private void DamagePlayer(NeonRunState run, float damage)
        {
            if (run.Player.InvulnerabilityRemaining > 0f || run.Status != NeonRunStatus.Running)
            {
                return;
            }

            var mitigated = Math.Max(1f, damage - run.Player.Stats.Armor);
            run.Player.Stats.CurrentHP -= mitigated;
            run.Player.InvulnerabilityRemaining = 0.3f;
            if (run.Player.Stats.CurrentHP <= 0f)
            {
                run.Player.Stats.CurrentHP = 0f;
                run.Status = NeonRunStatus.GameOver;
            }
        }

        private void OpenLevelUpDraft(NeonRunState run, NeonSkySurvivorsCatalog catalog)
        {
            run.Player.Level += 1;
            run.Player.XP -= run.Player.XPToNextLevel;
            run.Player.XPToNextLevel = (float)Math.Ceiling(run.Player.XPToNextLevel * 1.35f + 2f);
            run.DraftChoices.Clear();

            var eligible = catalog.Upgrades
                .Where(upgrade => run.Build.GetLevel(upgrade.Id) < upgrade.MaxLevel)
                .OrderBy(_ => _random.Next())
                .Take(3)
                .ToList();

            run.DraftChoices.AddRange(eligible);
            run.Status = run.DraftChoices.Count > 0 ? NeonRunStatus.LevelUpDraft : NeonRunStatus.Running;
        }

        private void ApplyUpgradeStats(NeonRunState run, NeonUpgradeDef upgrade)
        {
            foreach (var stat in upgrade.PerLevelStats)
            {
                ApplyModifier(run.Player.Stats, stat);
            }
        }

        private void TryEvolve(NeonRunState run, NeonUpgradeDef upgrade)
        {
            if (string.IsNullOrWhiteSpace(upgrade.EvolutionId) || run.Build.GetLevel(upgrade.Id) < upgrade.MaxLevel || string.IsNullOrWhiteSpace(upgrade.RequiredPassiveId))
            {
                return;
            }

            if (run.Build.GetLevel(upgrade.RequiredPassiveId) >= 1 && !run.Build.EvolvedWeapons.Contains(upgrade.EvolutionId))
            {
                run.Build.EvolvedWeapons.Add(upgrade.EvolutionId);
            }
        }

        private static bool HasUpgrade(NeonRunState run, string upgradeId)
        {
            return run.Build.GetLevel(upgradeId) > 0;
        }

        private static float GetTrailDamageMultiplier(NeonRunState run)
        {
            return HasUpgrade(run, "trail_damage_boost") ? 1.8f : 1f;
        }

        private static NeonRunEnemyState? FindNearestEnemy(NeonRunState run, NeonVector2 position)
        {
            NeonRunEnemyState? best = null;
            var bestDistance = float.MaxValue;
            foreach (var enemy in run.Enemies)
            {
                var distance = (enemy.Position - position).SqrMagnitude;
                if (distance >= bestDistance)
                {
                    continue;
                }

                best = enemy;
                bestDistance = distance;
            }

            return best;
        }

        private static void ApplyModifier(NeonPlayerStats stats, NeonStatModifier modifier)
        {
            switch (modifier.StatType)
            {
                case NeonStatType.AttackDamage:
                    stats.AttackDamage = Apply(stats.AttackDamage, modifier);
                    break;
                case NeonStatType.FireRate:
                    stats.FireRate = Apply(stats.FireRate, modifier);
                    break;
                case NeonStatType.MovementSpeed:
                    stats.MovementSpeed = Apply(stats.MovementSpeed, modifier);
                    break;
                case NeonStatType.MaxHP:
                    stats.MaxHP = Apply(stats.MaxHP, modifier);
                    stats.CurrentHP += modifier.IsPercent ? 0f : modifier.Value;
                    break;
                case NeonStatType.Armor:
                    stats.Armor = Apply(stats.Armor, modifier);
                    break;
                case NeonStatType.CriticalChance:
                    stats.CriticalChance = Apply(stats.CriticalChance, modifier);
                    break;
                case NeonStatType.MagnetRange:
                    stats.MagnetRange = Apply(stats.MagnetRange, modifier);
                    break;
                case NeonStatType.DashCooldown:
                    stats.DashCooldown = Math.Max(0.6f, Apply(stats.DashCooldown, modifier));
                    break;
                case NeonStatType.XPModifier:
                    stats.XPModifier = Apply(stats.XPModifier, modifier);
                    break;
            }
        }

        private static float Apply(float current, NeonStatModifier modifier)
        {
            return modifier.IsPercent ? current * modifier.Value : current + modifier.Value;
        }

        private static NeonVector2 ClampToArena(NeonVector2 position)
        {
            return new NeonVector2(Math.Max(-1f, Math.Min(1f, position.X)), Math.Max(-1f, Math.Min(1f, position.Y)));
        }

        private static float DistanceToSegment(NeonVector2 point, NeonVector2 start, NeonVector2 end)
        {
            var segment = end - start;
            var lengthSquared = segment.SqrMagnitude;
            if (lengthSquared <= 0.0001f)
            {
                return NeonVector2.Distance(point, start);
            }

            var t = ((point.X - start.X) * segment.X + (point.Y - start.Y) * segment.Y) / lengthSquared;
            t = Math.Max(0f, Math.Min(1f, t));
            var projection = start + segment * t;
            return NeonVector2.Distance(point, projection);
        }
    }
}
