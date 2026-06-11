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

        // Allocation-free enemy lookup: built once per catalog so high-spawn-rate late
        // waves do not allocate a LINQ closure + linear scan on every spawn (GC pressure).
        private readonly Dictionary<string, NeonEnemyDef> _enemyLookup = new Dictionary<string, NeonEnemyDef>();
        private NeonSkySurvivorsCatalog? _enemyLookupSource;

        public NeonRunGameplaySystem(int seed = 1337)
        {
            _random = new Random(seed);
        }

        public NeonRunState StartRun(NeonSaveProfile profile, NeonSkySurvivorsCatalog catalog)
        {
            var equipmentSystem = new NeonEquipmentSystem();
            equipmentSystem.EnsureStartingProfile(profile, catalog);

            var run = new NeonRunState();
            run.RerollsRemaining = 2;
            run.BanishesRemaining = 1;
            run.Player.Stats = equipmentSystem.CalculateStats(profile, catalog);
            run.Player.Stats.CurrentHP = run.Player.Stats.MaxHP;
            run.Player.SpecialCharge = Math.Min(run.Player.Stats.StartingEnergy, run.Player.SpecialChargeMax);
            run.Player.Position = NeonVector2.Zero;
            run.Player.MovementTarget = NeonVector2.Zero;
            run.Player.LastMoveDirection = NeonVector2.Up;
            PopulateEquipmentEffects(run, profile);
            return run;
        }

        private static void PopulateEquipmentEffects(NeonRunState run, NeonSaveProfile profile)
        {
            // Map equipped item IDs to in-run special-effect keys the simulation understands.
            AddEffectIfEquipped(run, profile, "guardian_frame", "guardian_block");
            AddEffectIfEquipped(run, profile, "solar_shield_hull", "solar_shield");
            AddEffectIfEquipped(run, profile, "neon_wings", "dash_firerate");
            AddEffectIfEquipped(run, profile, "overdrive_core", "levelup_damage");
            AddEffectIfEquipped(run, profile, "quantum_sensor", "boss_reward_boost");
        }

        private static void AddEffectIfEquipped(NeonRunState run, NeonSaveProfile profile, string itemId, string effectKey)
        {
            if (profile.EquippedWeaponItemID == itemId
                || profile.EquippedWingsItemID == itemId
                || profile.EquippedEngineItemID == itemId
                || profile.EquippedHullItemID == itemId
                || profile.EquippedCoreItemID == itemId
                || profile.EquippedRadarItemID == itemId)
            {
                run.ActiveEquipmentEffects.Add(effectKey);
            }
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
            if (run.ActiveEquipmentEffects.Contains("dash_firerate"))
            {
                // Neon Wings: +20% fire rate for 2s after a dash.
                run.Player.FireRateBuffRemaining = 2f;
            }
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
            TickSpecialCharge(run, deltaTime);
            TickMovement(run, deltaTime);
            TickTimeline(run, catalog, previousElapsedSeconds);
            TickAutoFire(run, deltaTime);
            TickHomingMissiles(run);
            TickLaserWings(run);
            TickOrbitBlades(run, deltaTime);
            TickProjectiles(run, deltaTime);
            TickEnemyMovementAndContact(run, deltaTime);
            TickDashTrails(run, deltaTime);
            TickXpCollection(run, catalog, deltaTime);
            TickEvolutionChests(run, catalog, deltaTime);
            CleanupDefeatedEnemies(run, catalog);
            TrySpawnWaveEnemies(run, catalog, deltaTime);
        }

        public bool ApplyUpgradeChoice(NeonRunState run, NeonSkySurvivorsCatalog catalog, NeonUpgradeDef upgrade)
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
            CheckEvolutions(run, catalog);
            run.DraftChoices.Clear();
            run.Status = NeonRunStatus.Running;
            return true;
        }

        public bool TryActivateSpecial(NeonRunState run)
        {
            if (run.Status != NeonRunStatus.Running || run.Player.SpecialCharge < run.Player.SpecialChargeMax)
            {
                return false;
            }

            run.Player.SpecialCharge = 0f;

            // Neon Nova: arena-wide burst, clears enemy fire, and briefly shields the player.
            var novaDamage = run.Player.Stats.AttackDamage * 6f;
            foreach (var enemy in run.Enemies)
            {
                enemy.HP -= novaDamage;
            }

            for (var index = run.Projectiles.Count - 1; index >= 0; index--)
            {
                if (!run.Projectiles[index].FromPlayer)
                {
                    run.Projectiles.RemoveAt(index);
                }
            }

            run.Player.InvulnerabilityRemaining = Math.Max(run.Player.InvulnerabilityRemaining, 1f);
            return true;
        }

        private void TickSpecialCharge(NeonRunState run, float deltaTime)
        {
            if (run.Player.SpecialCharge >= run.Player.SpecialChargeMax)
            {
                return;
            }

            var perSecond = 7f * Math.Max(0.1f, run.Player.Stats.SpecialChargeSpeed);
            run.Player.SpecialCharge = Math.Min(run.Player.SpecialChargeMax, run.Player.SpecialCharge + perSecond * deltaTime);
        }

        private void TickTimers(NeonRunState run, float deltaTime)
        {
            run.Player.DashCooldownRemaining = Math.Max(0f, run.Player.DashCooldownRemaining - deltaTime);
            run.Player.InvulnerabilityRemaining = Math.Max(0f, run.Player.InvulnerabilityRemaining - deltaTime);
            run.Player.WeaponCooldownRemaining = Math.Max(0f, run.Player.WeaponCooldownRemaining - deltaTime);
            run.Player.MissileCooldownRemaining = Math.Max(0f, run.Player.MissileCooldownRemaining - deltaTime);
            run.Player.LaserCooldownRemaining = Math.Max(0f, run.Player.LaserCooldownRemaining - deltaTime);
            run.Player.GuardianBlockCooldown = Math.Max(0f, run.Player.GuardianBlockCooldown - deltaTime);
            run.Player.FireRateBuffRemaining = Math.Max(0f, run.Player.FireRateBuffRemaining - deltaTime);
            run.Player.DamageBoostRemaining = Math.Max(0f, run.Player.DamageBoostRemaining - deltaTime);

            // Re-arm the Solar Shield once the pilot recovers above 40% HP.
            if (!run.Player.SolarShieldArmed && run.Player.Stats.CurrentHP > run.Player.Stats.MaxHP * 0.4f)
            {
                run.Player.SolarShieldArmed = true;
            }
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
            var damage = run.Player.Stats.AttackDamage * (isCritical ? run.Player.Stats.CriticalDamage : 1f) * DamageMultiplier(run);

            if (HasEvolution(run, "plasma_storm"))
            {
                // Plasma Storm: a hard-hitting, piercing 3-way spread.
                for (var shot = -1; shot <= 1; shot++)
                {
                    run.Projectiles.Add(new NeonRunProjectileState
                    {
                        Position = run.Player.Position,
                        Velocity = Rotate(direction, shot * 0.18f) * 1.9f,
                        Damage = damage * 1.4f,
                        Radius = isCritical ? 0.26f : 0.2f,
                        RemainingPierce = 2,
                        FromPlayer = true
                    });
                }
            }
            else
            {
                run.Projectiles.Add(new NeonRunProjectileState
                {
                    Position = run.Player.Position,
                    Velocity = direction * 1.8f,
                    Damage = damage,
                    Radius = isCritical ? 0.24f : 0.18f,
                    RemainingPierce = HasUpgrade(run, "plasma_blaster") && run.Build.GetLevel("plasma_blaster") >= 5 ? 1 : 0,
                    FromPlayer = true
                });
            }

            var effectiveFireRate = run.Player.Stats.FireRate * (run.Player.FireRateBuffRemaining > 0f ? 1.2f : 1f);
            run.Player.WeaponCooldownRemaining = 1f / Math.Max(0.1f, effectiveFireRate);
        }

        private void TickHomingMissiles(NeonRunState run)
        {
            var level = run.Build.GetLevel("homing_missiles");
            if (level <= 0 || run.Player.MissileCooldownRemaining > 0f || run.Enemies.Count == 0)
            {
                return;
            }

            var count = level >= 2 ? 2 : 1;
            var explosionRadius = level >= 4 ? 0.32f : 0.18f;
            var splits = level >= 5;
            var damage = run.Player.Stats.AttackDamage * 0.9f * DamageMultiplier(run);
            var cooldown = level >= 3 ? 1.5f : 2.2f;

            if (HasEvolution(run, "rocket_swarm"))
            {
                // Rocket Swarm: more missiles, always-splitting, larger blasts, rapid cadence.
                count += 2;
                explosionRadius = Math.Max(explosionRadius, 0.34f);
                splits = true;
                damage *= 1.2f;
                cooldown *= 0.6f;
            }

            for (var index = 0; index < count; index++)
            {
                var target = FindNearestEnemy(run, run.Player.Position);
                var direction = target != null ? (target.Position - run.Player.Position).Normalized : run.Player.LastMoveDirection.Normalized;
                if (direction.SqrMagnitude <= 0.0001f)
                {
                    direction = NeonVector2.Up;
                }

                // Fan multiple missiles out slightly so they do not perfectly overlap.
                var spread = (index - (count - 1) * 0.5f) * 0.25f;
                run.Projectiles.Add(new NeonRunProjectileState
                {
                    Position = run.Player.Position,
                    Velocity = Rotate(direction, spread) * 1.3f,
                    Damage = damage,
                    Radius = 0.16f,
                    RemainingLife = 3.5f,
                    FromPlayer = true,
                    IsHoming = true,
                    ExplosionRadius = explosionRadius,
                    SplitsOnHit = splits
                });
            }

            run.Player.MissileCooldownRemaining = cooldown;
        }

        private void TickLaserWings(NeonRunState run)
        {
            var level = run.Build.GetLevel("laser_wings");
            if (level <= 0 || run.Player.LaserCooldownRemaining > 0f)
            {
                return;
            }

            var facing = run.Player.LastMoveDirection.SqrMagnitude > 0.0001f ? run.Player.LastMoveDirection.Normalized : NeonVector2.Up;
            var isSolarSplitter = HasEvolution(run, "solar_splitter");

            // Solar Splitter: 50% crit bonus and triple beams (6 total) with extended reach.
            var critBonus = isSolarSplitter ? 0.5f : 0f;
            var isCritical = _random.NextDouble() <= Math.Min(1.0, run.Player.Stats.CriticalChance + critBonus);
            var critMult = isCritical ? run.Player.Stats.CriticalDamage : 1f;
            var damage = run.Player.Stats.AttackDamage * (0.5f + 0.1f * level) * DamageMultiplier(run) * critMult;
            var speed = isSolarSplitter ? 3.2f : (level >= 3 ? 2.6f : 2.0f);
            var life  = isSolarSplitter ? 1.4f : (level >= 3 ? 1.1f : 0.85f);

            const float halfPi = 1.5707964f;
            if (isSolarSplitter)
            {
                // Six beams: perpendicular pair + two angled pairs (120° spread total)
                for (var beam = 0; beam < 3; beam++)
                {
                    var spread = (beam - 1) * 0.42f;
                    FireLaserBolt(run, Rotate(facing,  halfPi + spread), damage, speed, life);
                    FireLaserBolt(run, Rotate(facing, -halfPi - spread), damage, speed, life);
                }
            }
            else
            {
                FireLaserBolt(run, Rotate(facing,  halfPi), damage, speed, life);
                FireLaserBolt(run, Rotate(facing, -halfPi), damage, speed, life);
                if (level >= 5)
                {
                    FireLaserBolt(run, Rotate(facing,  halfPi + 0.35f), damage, speed, life);
                    FireLaserBolt(run, Rotate(facing, -halfPi - 0.35f), damage, speed, life);
                }
            }

            run.Player.LaserCooldownRemaining = isSolarSplitter ? 0.6f : (level >= 4 ? 0.8f : 1.2f);
        }

        private void TickOrbitBlades(NeonRunState run, float deltaTime)
        {
            run.OrbitBlades.Clear();
            var level = run.Build.GetLevel("orbit_blades");
            if (level <= 0)
            {
                return;
            }

            var isNeonBarrier = HasEvolution(run, "neon_barrier");
            var count         = isNeonBarrier ? Math.Max(2, level >= 2 ? 2 : 1) : (level >= 2 ? 2 : 1);
            var radius        = isNeonBarrier ? 0.55f : (level >= 3 ? 0.34f : 0.26f);
            var rotationSpeed = isNeonBarrier ? 4.5f  : (level >= 4 ? 3.5f  : 2.2f);
            var knockback     = level >= 5 || isNeonBarrier;

            // Neon Barrier: blades act as a shield — deal reflective AoE damage in a wider radius
            var bladeHitRadius  = isNeonBarrier ? 0.28f : 0.14f;
            var damagePerSecond = run.Player.Stats.AttackDamage * (isNeonBarrier ? 4.0f : 2.5f) * DamageMultiplier(run);

            run.Player.OrbitAngle += rotationSpeed * deltaTime;
            const float twoPi = 6.2831855f;
            for (var index = 0; index < count; index++)
            {
                var angle = run.Player.OrbitAngle + index * (twoPi / count);
                var bladePosition = run.Player.Position + new NeonVector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                run.OrbitBlades.Add(bladePosition);

                foreach (var enemy in run.Enemies)
                {
                    if (NeonVector2.Distance(enemy.Position, bladePosition) > bladeHitRadius)
                    {
                        continue;
                    }

                    enemy.HP -= damagePerSecond * deltaTime;
                    if (knockback)
                    {
                        var push = (enemy.Position - run.Player.Position).Normalized * (isNeonBarrier ? 0.04f : 0.02f);
                        enemy.Position = ClampToArena(enemy.Position + push);
                    }
                }

                // Neon Barrier: also block incoming projectiles that touch the blade orbit radius
                if (isNeonBarrier)
                {
                    for (var pIndex = run.Projectiles.Count - 1; pIndex >= 0; pIndex--)
                    {
                        var proj = run.Projectiles[pIndex];
                        if (proj.FromPlayer) continue;
                        if (NeonVector2.Distance(proj.Position, bladePosition) <= bladeHitRadius)
                        {
                            run.Projectiles.RemoveAt(pIndex);
                        }
                    }
                }
            }
        }

        private void FireLaserBolt(NeonRunState run, NeonVector2 direction, float damage, float speed, float life)
        {
            run.Projectiles.Add(new NeonRunProjectileState
            {
                Position = run.Player.Position,
                Velocity = direction * speed,
                Damage = damage,
                Radius = 0.14f,
                RemainingLife = life,
                RemainingPierce = 3,
                FromPlayer = true
            });
        }

        private void TickProjectiles(NeonRunState run, float deltaTime)
        {
            for (var projectileIndex = run.Projectiles.Count - 1; projectileIndex >= 0; projectileIndex--)
            {
                var projectile = run.Projectiles[projectileIndex];

                if (projectile.IsHoming)
                {
                    var target = FindNearestEnemy(run, projectile.Position);
                    if (target != null)
                    {
                        var speed = projectile.Velocity.Magnitude;
                        if (speed <= 0.0001f)
                        {
                            speed = 1.3f;
                        }

                        projectile.Velocity = (target.Position - projectile.Position).Normalized * speed;
                    }
                }

                projectile.Position += projectile.Velocity * deltaTime;
                projectile.RemainingLife -= deltaTime;

                if (projectile.FromPlayer)
                {
                    if (ResolvePlayerProjectileHits(run, projectile, projectileIndex))
                    {
                        continue;
                    }
                }
                else if (ResolveEnemyProjectileHits(run, projectile, projectileIndex))
                {
                    continue;
                }

                if (projectile.RemainingLife <= 0f || Math.Abs(projectile.Position.X) > 1.2f || Math.Abs(projectile.Position.Y) > 1.2f)
                {
                    // Mines that time out still detonate in a small radius around the player.
                    if (projectile.IsMine && NeonVector2.Distance(projectile.Position, run.Player.Position) <= 0.3f)
                    {
                        DamagePlayer(run, projectile.Damage);
                    }

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

                if (projectile.ExplosionRadius > 0f)
                {
                    DetonateProjectile(run, projectile, enemy.Position);
                    run.Projectiles.RemoveAt(projectileIndex);
                    return true;
                }

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

        private void DetonateProjectile(NeonRunState run, NeonRunProjectileState projectile, NeonVector2 center)
        {
            // Area damage to every enemy within the blast radius.
            foreach (var enemy in run.Enemies)
            {
                if (NeonVector2.Distance(enemy.Position, center) <= projectile.ExplosionRadius)
                {
                    enemy.HP -= projectile.Damage * 0.75f;
                }
            }

            if (!projectile.SplitsOnHit)
            {
                return;
            }

            // Level 5 missiles split into two non-homing fragments on impact.
            for (var index = 0; index < 2; index++)
            {
                var direction = Rotate(NeonVector2.Up, index == 0 ? 0.6f : -0.6f);
                run.Projectiles.Add(new NeonRunProjectileState
                {
                    Position = center,
                    Velocity = direction * 1.6f,
                    Damage = projectile.Damage * 0.5f,
                    Radius = 0.12f,
                    RemainingLife = 1.2f,
                    RemainingPierce = 1,
                    FromPlayer = true
                });
            }
        }

        private bool ResolveEnemyProjectileHits(NeonRunState run, NeonRunProjectileState projectile, int projectileIndex)
        {
            var triggerRange = projectile.IsMine ? 0.2f : 0.14f;
            if (NeonVector2.Distance(projectile.Position, run.Player.Position) > triggerRange)
            {
                return false;
            }

            DamagePlayer(run, projectile.Damage);
            run.Projectiles.RemoveAt(projectileIndex);
            return true;
        }

        private void TickEnemyMovementAndContact(NeonRunState run, float deltaTime)
        {
            foreach (var enemy in run.Enemies)
            {
                enemy.AttackCooldownRemaining = Math.Max(0f, enemy.AttackCooldownRemaining - deltaTime);

                var toPlayer = run.Player.Position - enemy.Position;
                var distance = toPlayer.Magnitude;
                var direction = toPlayer.Normalized;
                var step = (enemy.Speed / 10f) * deltaTime;

                if (!enemy.IsBoss && enemy.Behavior == NeonEnemyBehaviorType.Shooter)
                {
                    // Keep distance and fire simple bullets at the player.
                    const float preferredRange = 0.55f;
                    if (distance > preferredRange + 0.08f)
                    {
                        enemy.Position += direction * step;
                    }
                    else if (distance < preferredRange - 0.08f)
                    {
                        enemy.Position -= direction * step;
                    }

                    if (enemy.AttackCooldownRemaining <= 0f)
                    {
                        FireEnemyBullet(run, enemy, direction);
                        enemy.AttackCooldownRemaining = 1.7f;
                    }
                }
                else if (!enemy.IsBoss && enemy.Behavior == NeonEnemyBehaviorType.MineCarrier)
                {
                    // Drift slowly toward the player while dropping timed mines.
                    enemy.Position += direction * step;
                    if (enemy.AttackCooldownRemaining <= 0f)
                    {
                        DropMine(run, enemy);
                        enemy.AttackCooldownRemaining = 2.6f;
                    }
                }
                else
                {
                    // Chasers, fast chasers, tanks, splitters, and bosses move toward the player.
                    enemy.Position += direction * step;
                }

                if (distance <= 0.16f)
                {
                    DamagePlayer(run, enemy.ContactDamage);
                }
            }
        }

        private void FireEnemyBullet(NeonRunState run, NeonRunEnemyState enemy, NeonVector2 direction)
        {
            if (direction.SqrMagnitude <= 0.0001f)
            {
                direction = NeonVector2.Up;
            }

            run.Projectiles.Add(new NeonRunProjectileState
            {
                Position = enemy.Position,
                Velocity = direction * 1.15f,
                Damage = enemy.ContactDamage,
                Radius = 0.14f,
                RemainingLife = 3f,
                FromPlayer = false
            });
        }

        private void DropMine(NeonRunState run, NeonRunEnemyState enemy)
        {
            run.Projectiles.Add(new NeonRunProjectileState
            {
                Position = enemy.Position,
                Velocity = NeonVector2.Zero,
                Damage = enemy.ContactDamage * 1.4f,
                Radius = 0.18f,
                RemainingLife = 2.6f,
                FromPlayer = false,
                IsMine = true
            });
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

        private void TickEvolutionChests(NeonRunState run, NeonSkySurvivorsCatalog catalog, float deltaTime)
        {
            for (var chestIndex = run.EvolutionChests.Count - 1; chestIndex >= 0; chestIndex--)
            {
                var chest = run.EvolutionChests[chestIndex];
                chest.RemainingLife -= deltaTime;
                if (chest.RemainingLife <= 0f)
                {
                    run.EvolutionChests.RemoveAt(chestIndex);
                    continue;
                }

                var pickupRange = Math.Max(0.12f, run.Player.Stats.MagnetRange / 20f);
                if (NeonVector2.Distance(chest.Position, run.Player.Position) > pickupRange)
                {
                    continue;
                }

                run.EvolutionChests.RemoveAt(chestIndex);
                OpenEvolutionChest(run, catalog);
            }
        }

        /// <summary>
        /// Boss-chest evolution path (Section 15): the chest evolves a max-level weapon even
        /// if its required passive was never picked. If nothing is eligible the chest falls
        /// back to a heal plus special charge so it is never a dead pickup.
        /// </summary>
        public bool OpenEvolutionChest(NeonRunState run, NeonSkySurvivorsCatalog catalog)
        {
            foreach (var upgrade in catalog.Upgrades)
            {
                if (string.IsNullOrWhiteSpace(upgrade.EvolutionId))
                {
                    continue;
                }

                if (run.Build.GetLevel(upgrade.Id) < upgrade.MaxLevel || run.Build.EvolvedWeapons.Contains(upgrade.EvolutionId))
                {
                    continue;
                }

                run.Build.EvolvedWeapons.Add(upgrade.EvolutionId);
                return true;
            }

            run.Player.Stats.CurrentHP = Math.Min(run.Player.Stats.MaxHP, run.Player.Stats.CurrentHP + run.Player.Stats.MaxHP * 0.15f);
            run.Player.SpecialCharge = Math.Min(run.Player.SpecialChargeMax, run.Player.SpecialCharge + 25f);
            return false;
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
                run.Player.SpecialCharge = Math.Min(run.Player.SpecialChargeMax, run.Player.SpecialCharge + (enemy.IsBoss ? 20f : 1.5f));
                if (_random.NextDouble() <= 0.2f * run.Player.Stats.CoinBonus)
                {
                    run.Player.CoinsCollected += 1;
                }

                if (enemy.CanSplit)
                {
                    SpawnSplitChildren(run, enemy);
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

                // Defeated bosses drop an evolution chest (Section 15: the chest/boss path).
                run.EvolutionChests.Add(new NeonEvolutionChestState { Position = enemy.Position });

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

        private NeonEnemyDef? GetEnemyDefinition(NeonSkySurvivorsCatalog catalog, string enemyId)
        {
            if (!ReferenceEquals(_enemyLookupSource, catalog))
            {
                _enemyLookup.Clear();
                foreach (var enemy in catalog.Enemies)
                {
                    _enemyLookup[enemy.EnemyID] = enemy;
                }
                _enemyLookupSource = catalog;
            }

            return _enemyLookup.TryGetValue(enemyId, out var definition) ? definition : null;
        }

        private void SpawnEnemy(NeonRunState run, NeonSkySurvivorsCatalog catalog, string enemyId)
        {
            var definition = GetEnemyDefinition(catalog, enemyId);
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
                IsBoss = false,
                Behavior = definition.BehaviorType,
                CanSplit = definition.BehaviorType == NeonEnemyBehaviorType.Splitter,
                // Stagger ranged/mine attacks so they do not all fire on the same frame.
                AttackCooldownRemaining = 0.6f + (float)_random.NextDouble() * 1.4f
            });
        }

        private void SpawnSplitChildren(NeonRunState run, NeonRunEnemyState parent)
        {
            var childHp = Math.Max(6f, parent.MaxHP * 0.4f);
            for (var index = 0; index < 2; index++)
            {
                var offset = index == 0 ? new NeonVector2(0.06f, 0.04f) : new NeonVector2(-0.06f, -0.04f);
                run.Enemies.Add(new NeonRunEnemyState
                {
                    EnemyID = parent.EnemyID,
                    Position = ClampToArena(parent.Position + offset),
                    HP = childHp,
                    MaxHP = childHp,
                    ContactDamage = parent.ContactDamage * 0.7f,
                    Speed = parent.Speed + 0.6f,
                    XPDrop = 1,
                    IsBoss = false,
                    Behavior = NeonEnemyBehaviorType.FastChaser,
                    CanSplit = false,
                    AttackCooldownRemaining = 0f
                });
            }
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

            // Guardian Frame: fully block one hit every 30 seconds.
            if (run.ActiveEquipmentEffects.Contains("guardian_block") && run.Player.GuardianBlockCooldown <= 0f)
            {
                run.Player.GuardianBlockCooldown = 30f;
                run.Player.InvulnerabilityRemaining = 0.3f;
                return;
            }

            var mitigated = Math.Max(1f, damage - run.Player.Stats.Armor);
            run.Player.Stats.CurrentHP -= mitigated;
            run.Player.InvulnerabilityRemaining = 0.3f;

            // Solar Shield Hull: once HP dips below 30%, grant a brief protective shield.
            if (run.ActiveEquipmentEffects.Contains("solar_shield")
                && run.Player.SolarShieldArmed
                && run.Player.Stats.CurrentHP > 0f
                && run.Player.Stats.CurrentHP < run.Player.Stats.MaxHP * 0.3f)
            {
                run.Player.SolarShieldArmed = false;
                run.Player.InvulnerabilityRemaining = Math.Max(run.Player.InvulnerabilityRemaining, 1.5f);
            }

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
            if (run.ActiveEquipmentEffects.Contains("levelup_damage"))
            {
                // Overdrive Core: temporary damage boost after each level-up.
                run.Player.DamageBoostRemaining = 6f;
            }

            PopulateDraft(run, catalog);
        }

        /// <summary>Fills DraftChoices with up to 3 eligible (un-maxed, un-banned) upgrades.</summary>
        private void PopulateDraft(NeonRunState run, NeonSkySurvivorsCatalog catalog)
        {
            run.DraftChoices.Clear();
            var eligible = catalog.Upgrades
                .Where(upgrade => !run.BannedUpgradeIds.Contains(upgrade.Id)
                                  && run.Build.GetLevel(upgrade.Id) < upgrade.MaxLevel)
                .OrderBy(_ => _random.Next())
                .Take(3)
                .ToList();

            run.DraftChoices.AddRange(eligible);
            run.Status = run.DraftChoices.Count > 0 ? NeonRunStatus.LevelUpDraft : NeonRunStatus.Running;
        }

        /// <summary>Spend a reroll to redraw the whole draft (respects banned upgrades).</summary>
        public bool RerollDraft(NeonRunState run, NeonSkySurvivorsCatalog catalog)
        {
            if (run.Status != NeonRunStatus.LevelUpDraft || run.RerollsRemaining <= 0)
            {
                return false;
            }

            run.RerollsRemaining -= 1;
            PopulateDraft(run, catalog);
            return true;
        }

        /// <summary>Spend a banish to remove an upgrade from this run forever; refill its slot.</summary>
        public bool BanishUpgrade(NeonRunState run, NeonSkySurvivorsCatalog catalog, NeonUpgradeDef upgrade)
        {
            if (run.Status != NeonRunStatus.LevelUpDraft || run.BanishesRemaining <= 0
                || upgrade == null || !run.DraftChoices.Contains(upgrade))
            {
                return false;
            }

            run.BanishesRemaining -= 1;
            run.BannedUpgradeIds.Add(upgrade.Id);
            run.DraftChoices.Remove(upgrade);

            var replacement = catalog.Upgrades
                .Where(u => !run.BannedUpgradeIds.Contains(u.Id)
                            && run.Build.GetLevel(u.Id) < u.MaxLevel
                            && !run.DraftChoices.Contains(u))
                .OrderBy(_ => _random.Next())
                .FirstOrDefault();
            if (replacement != null)
            {
                run.DraftChoices.Add(replacement);
            }
            return true;
        }

        private void ApplyUpgradeStats(NeonRunState run, NeonUpgradeDef upgrade)
        {
            foreach (var stat in upgrade.PerLevelStats)
            {
                ApplyModifier(run.Player.Stats, stat);
            }
        }

        private void CheckEvolutions(NeonRunState run, NeonSkySurvivorsCatalog catalog)
        {
            // Scan every weapon upgrade so an evolution triggers regardless of whether the
            // weapon or its required passive was the most recently picked card.
            foreach (var upgrade in catalog.Upgrades)
            {
                if (string.IsNullOrWhiteSpace(upgrade.EvolutionId) || string.IsNullOrWhiteSpace(upgrade.RequiredPassiveId))
                {
                    continue;
                }

                if (run.Build.GetLevel(upgrade.Id) < upgrade.MaxLevel || run.Build.GetLevel(upgrade.RequiredPassiveId) < 1)
                {
                    continue;
                }

                if (!run.Build.EvolvedWeapons.Contains(upgrade.EvolutionId))
                {
                    run.Build.EvolvedWeapons.Add(upgrade.EvolutionId);
                }
            }
        }

        private static bool HasEvolution(NeonRunState run, string evolutionId)
        {
            return run.Build.EvolvedWeapons.Contains(evolutionId);
        }

        private static float DamageMultiplier(NeonRunState run)
        {
            // Overdrive Core grants a temporary post-level-up damage boost.
            return run.Player.DamageBoostRemaining > 0f ? 1.3f : 1f;
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

        private static NeonVector2 Rotate(NeonVector2 value, float radians)
        {
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);
            return new NeonVector2(value.X * cos - value.Y * sin, value.X * sin + value.Y * cos);
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
