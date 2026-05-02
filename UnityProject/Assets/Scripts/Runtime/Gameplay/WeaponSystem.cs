using Cosmostar.Runtime.Gameplay;
using UnityEngine;

namespace Cosmostar.Runtime.Systems
{
    public sealed class WeaponSystem
    {
        public void TickPlayerFire(RunSession session, float deltaTime)
        {
            session.WeaponCooldown -= deltaTime;
            session.DroneCooldown -= deltaTime;

            var comboFireRateMultiplier = 1f + Mathf.Min(0.4f, Mathf.Max(0f, session.ComboCount - 1) * 0.03f);
            var fireInterval = session.Weapon.FireInterval / (session.Meta.FireRateMultiplier * session.Build.FireRateMultiplier * comboFireRateMultiplier);
            if (session.WeaponCooldown <= 0f)
            {
                session.WeaponCooldown = fireInterval;
                SpawnPlayerVolley(session, session.Player.Position, session.Weapon.ProjectileCount + session.Build.BonusProjectiles, session.Weapon.SpreadDegrees);
            }

            if (session.Build.DroneCompanions > 0 && session.DroneCooldown <= 0f)
            {
                session.DroneCooldown = 0.55f;
                for (var droneIndex = 0; droneIndex < session.Build.DroneCompanions; droneIndex++)
                {
                    var xOffset = droneIndex == 0 ? -0.05f : 0.05f;
                    SpawnDroneShot(session, new Vector2(session.Player.Position.x + xOffset, session.Player.Position.y + 0.02f));
                }
            }
        }

        public void TickProjectiles(RunSession session, float deltaTime)
        {
            for (var index = session.Projectiles.Count - 1; index >= 0; index--)
            {
                var projectile = session.Projectiles[index];
                if (projectile.FromPlayer && projectile.Homes)
                {
                    ApplyHoming(session, projectile, deltaTime);
                }

                projectile.Position += projectile.Velocity * deltaTime;
                projectile.RemainingLife -= deltaTime;

                if (projectile.Position.y < -0.1f || projectile.Position.y > 1.1f || projectile.Position.x < -0.1f || projectile.Position.x > 1.1f || projectile.RemainingLife <= 0f)
                {
                    session.Projectiles.RemoveAt(index);
                }
            }
        }

        private static void ApplyHoming(RunSession session, ProjectileState projectile, float deltaTime)
        {
            EnemyState target = null;
            var bestDistance = float.MaxValue;
            for (var index = 0; index < session.Enemies.Count; index++)
            {
                var enemy = session.Enemies[index];
                if (enemy.Hull <= 0f)
                {
                    continue;
                }

                var offset = enemy.Position - projectile.Position;
                if (offset.y < -0.05f)
                {
                    continue;
                }

                var distance = offset.sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    target = enemy;
                }
            }

            if (target == null)
            {
                return;
            }

            var speed = projectile.Velocity.magnitude;
            if (speed <= 0.0001f)
            {
                return;
            }

            var desiredDirection = (target.Position - projectile.Position).normalized;
            var currentDirection = projectile.Velocity / speed;
            var nextDirection = Vector2.Lerp(currentDirection, desiredDirection, Mathf.Clamp01(projectile.HomingStrength * deltaTime));
            projectile.Velocity = nextDirection.normalized * speed;
        }

        public void SpawnEnemyShot(RunSession session, EnemyState enemy, Vector2 direction, float speedScale)
        {
            SpawnEnemyShot(session, enemy.Position, enemy.Def.ContactDamage * 0.8f, enemy.Def.IsBoss, direction, speedScale);
        }

        public void SpawnEnemyShot(RunSession session, Vector2 origin, float damage, bool isBossShot, Vector2 direction, float speedScale)
        {
            session.Projectiles.Add(new ProjectileState
            {
                Position = origin,
                Velocity = direction.normalized * speedScale,
                Damage = damage,
                Radius = isBossShot ? 12f : 8f,
                RemainingLife = 6f,
                FromPlayer = false,
                Color = new Color(1f, 0.28f, 0.52f, 0.95f)
            });
        }

        private void SpawnPlayerVolley(RunSession session, Vector2 origin, int projectileCount, float spreadDegrees)
        {
            var clampedCount = Mathf.Max(1, projectileCount);
            var spread = clampedCount == 1 ? 0f : spreadDegrees;
            var baseDamage = session.Weapon.ProjectileDamage * session.Meta.DamageMultiplier * session.Build.DamageMultiplier;
            var projectileSpeed = session.Weapon.ProjectileSpeed;

            for (var projectileIndex = 0; projectileIndex < clampedCount; projectileIndex++)
            {
                var angleStep = clampedCount == 1 ? 0f : spread / (clampedCount - 1);
                var angle = -spread * 0.5f + angleStep * projectileIndex;
                var direction = Quaternion.Euler(0f, 0f, angle) * Vector2.up;
                var isCritical = Random.value <= session.Weapon.CritChance;
                var projectileColor = ResolvePlayerProjectileColor(session.Weapon.Family, isCritical);
                var projectileRadius = session.Weapon.Family == Cosmostar.Core.Models.WeaponFamily.Lance ? 10f : 7f;

                session.Projectiles.Add(new ProjectileState
                {
                    Position = origin + Vector2.up * 0.03f,
                    Velocity = direction.normalized * projectileSpeed,
                    Damage = isCritical ? baseDamage * 1.8f : baseDamage,
                    Radius = isCritical ? projectileRadius + 3f : projectileRadius,
                    RemainingLife = 3.2f,
                    FromPlayer = true,
                    IsCritical = isCritical,
                    RemainingPierce = session.Build.BonusPierce,
                    AppliesSlow = session.Build.FrostChance > 0f,
                    CanChain = session.Build.ChainChance > 0f,
                    Homes = session.Weapon.Family == Cosmostar.Core.Models.WeaponFamily.Arc,
                    HomingStrength = session.Weapon.Family == Cosmostar.Core.Models.WeaponFamily.Arc ? 6.5f : 0f,
                    Color = projectileColor
                });
            }
        }

        private void SpawnDroneShot(RunSession session, Vector2 origin)
        {
            session.Projectiles.Add(new ProjectileState
            {
                Position = origin,
                Velocity = new Vector2(0f, 1.55f),
                Damage = session.Weapon.ProjectileDamage * 0.45f * session.Meta.DamageMultiplier * session.Build.DamageMultiplier,
                Radius = 6f,
                RemainingLife = 2.8f,
                FromPlayer = true,
                Homes = true,
                HomingStrength = 5.5f,
                Color = new Color(1f, 0.95f, 0.3f, 0.95f)
            });
        }

        private static Color ResolvePlayerProjectileColor(Cosmostar.Core.Models.WeaponFamily family, bool isCritical)
        {
            if (isCritical)
            {
                return new Color(1f, 0.95f, 0.24f, 0.98f);
            }

            return family == Cosmostar.Core.Models.WeaponFamily.Arc ? new Color(0.2f, 0.95f, 1f) : new Color(0.26f, 1f, 0.42f);
        }
    }
}
