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
            AddPulse(session, session.Player.Position, new Color(1f, 0.95f, 0.28f, 0.85f), 18f, 90f, 0.42f);
            for (var index = session.Enemies.Count - 1; index >= 0; index--)
            {
                var enemy = session.Enemies[index];
                if (Vector2.Distance(session.Player.Position, enemy.Position) <= 0.28f)
                {
                    enemy.Hull -= session.Build.OverclockBurstDamage;
                    AddPulse(session, enemy.Position, new Color(1f, 0.95f, 0.28f, 0.8f), 10f, 34f, 0.28f);
                }
            }
        }

        public void TryApplyHitEffects(RunSession session, EnemyState primaryEnemy)
        {
            if (session.Build.FrostChance > 0f && Random.value <= session.Build.FrostChance)
            {
                primaryEnemy.SlowTimer = 1.25f;
                AddPulse(session, primaryEnemy.Position, new Color(0.25f, 0.95f, 1f, 0.8f), 12f, 32f, 0.35f);
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
                    AddLine(session, primaryEnemy.Position, enemy.Position, new Color(0.4f, 0.95f, 1f, 0.95f), 6f, 0.2f);
                    AddPulse(session, enemy.Position, new Color(0.4f, 0.95f, 1f, 0.8f), 8f, 26f, 0.28f);
                    return;
                }
            }
        }

        private static void AddPulse(RunSession session, Vector2 position, Color color, float startRadius, float endRadius, float duration)
        {
            session.CombatEffects.Add(new CombatEffectState
            {
                Position = position,
                Color = color,
                StartRadius = startRadius,
                EndRadius = endRadius,
                TotalDuration = duration,
                RemainingDuration = duration
            });
        }

        private static void AddLine(RunSession session, Vector2 start, Vector2 end, Color color, float width, float duration)
        {
            session.CombatEffects.Add(new CombatEffectState
            {
                Position = start,
                TargetPosition = end,
                Color = color,
                Width = width,
                TotalDuration = duration,
                RemainingDuration = duration,
                IsLine = true
            });
        }
    }
}
