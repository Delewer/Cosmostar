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

            var fireInterval = session.Weapon.FireInterval / (session.Meta.FireRateMultiplier * session.Build.FireRateMultiplier);
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
                projectile.Position += projectile.Velocity * deltaTime;
                projectile.RemainingLife -= deltaTime;

                if (projectile.Position.y < -0.1f || projectile.Position.y > 1.1f || projectile.Position.x < -0.1f || projectile.Position.x > 1.1f || projectile.RemainingLife <= 0f)
                {
                    session.Projectiles.RemoveAt(index);
                }
            }
        }

        public void SpawnEnemyShot(RunSession session, EnemyState enemy, Vector2 direction, float speedScale)
        {
            session.Projectiles.Add(new ProjectileState
            {
                Position = enemy.Position,
                Velocity = direction.normalized * speedScale,
                Damage = enemy.Def.ContactDamage * 0.8f,
                Radius = enemy.Def.IsBoss ? 12f : 8f,
                RemainingLife = 6f,
                FromPlayer = false,
                Color = new Color(1f, 0.28f, 0.52f, 0.95f)
            });
        }

        private void SpawnPlayerVolley(RunSession session, Vector2 origin, int projectileCount, float spreadDegrees)
        {
            var clampedCount = Mathf.Max(1, projectileCount);
            var spread = clampedCount == 1 ? 0f : spreadDegrees;
            var damage = session.Weapon.ProjectileDamage * session.Meta.DamageMultiplier * session.Build.DamageMultiplier;
            var projectileSpeed = session.Weapon.ProjectileSpeed;

            for (var projectileIndex = 0; projectileIndex < clampedCount; projectileIndex++)
            {
                var angleStep = clampedCount == 1 ? 0f : spread / (clampedCount - 1);
                var angle = -spread * 0.5f + angleStep * projectileIndex;
                var direction = Quaternion.Euler(0f, 0f, angle) * Vector2.up;

                session.Projectiles.Add(new ProjectileState
                {
                    Position = origin + Vector2.up * 0.03f,
                    Velocity = direction.normalized * projectileSpeed,
                    Damage = damage,
                    Radius = session.Weapon.Family == Cosmostar.Core.Models.WeaponFamily.Lance ? 10f : 7f,
                    RemainingLife = 3.2f,
                    FromPlayer = true,
                    RemainingPierce = session.Build.BonusPierce,
                    AppliesSlow = session.Build.FrostChance > 0f,
                    CanChain = session.Build.ChainChance > 0f,
                    Color = session.Weapon.Family == Cosmostar.Core.Models.WeaponFamily.Arc ? new Color(0.2f, 0.95f, 1f) : new Color(0.26f, 1f, 0.42f)
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
                Color = new Color(1f, 0.95f, 0.3f, 0.95f)
            });
        }
    }
}

