using Cosmostar.Runtime.Gameplay;
using UnityEngine;

namespace Cosmostar.Runtime.Systems
{
    public sealed class AbilitySystem
    {
        public void TickOverclock(RunSession session, float deltaTime)
        {
            if (session.Build.OverclockBurstDamage <= 0f)
            {
                return;
            }

            session.OverclockCooldown -= deltaTime;
            if (session.OverclockCooldown > 0f)
            {
                return;
            }

            session.OverclockCooldown = 4.5f;
            for (var index = session.Enemies.Count - 1; index >= 0; index--)
            {
                var enemy = session.Enemies[index];
                if (Vector2.Distance(session.Player.Position, enemy.Position) <= 0.28f)
                {
                    enemy.Hull -= session.Build.OverclockBurstDamage;
                }
            }
        }

        public void TryApplyHitEffects(RunSession session, EnemyState primaryEnemy)
        {
            if (session.Build.FrostChance > 0f && Random.value <= session.Build.FrostChance)
            {
                primaryEnemy.SlowTimer = 1.25f;
            }

            if (session.Build.ChainChance <= 0f || Random.value > session.Build.ChainChance)
            {
                return;
            }

            for (var index = 0; index < session.Enemies.Count; index++)
            {
                var enemy = session.Enemies[index];
                if (enemy == primaryEnemy)
                {
                    continue;
                }

                if (Vector2.Distance(enemy.Position, primaryEnemy.Position) <= 0.18f)
                {
                    enemy.Hull -= session.Weapon.ProjectileDamage * 0.35f * session.Build.DamageMultiplier;
                    return;
                }
            }
        }
    }
}

