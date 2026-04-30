using Cosmostar.Runtime.Gameplay;
using UnityEngine;

namespace Cosmostar.Runtime.Systems
{
    public sealed class PlayerController
    {
        public void TickMovement(RunSession session, Vector2 targetPosition, float deltaTime)
        {
            var speed = session.Ship.MoveSpeed * session.Meta.MoveSpeedMultiplier * session.Build.MoveSpeedMultiplier;
            var moveDelta = targetPosition - session.Player.Position;
            if (moveDelta.sqrMagnitude > 0.0001f)
            {
                session.LastMoveDirection = moveDelta.normalized;
            }

            session.Player.Position = Vector2.MoveTowards(session.Player.Position, targetPosition, speed * deltaTime);
            session.Player.Position = ClampToGameplayBounds(session.Player.Position);

            if (session.Player.InvulnerabilityTimer > 0f)
            {
                session.Player.InvulnerabilityTimer = Mathf.Max(0f, session.Player.InvulnerabilityTimer - deltaTime);
            }

            if (session.DashCooldownRemaining > 0f)
            {
                session.DashCooldownRemaining = Mathf.Max(0f, session.DashCooldownRemaining - deltaTime);
            }
        }

        public bool TryDash(RunSession session)
        {
            if (session == null || session.DashCooldownRemaining > 0f || session.DraftOpen || session.Paused || session.TutorialOpen || session.AwaitingRewardedRevive || session.Completed || session.Failed)
            {
                return false;
            }

            var direction = session.LastMoveDirection.sqrMagnitude > 0.0001f ? session.LastMoveDirection.normalized : Vector2.up;
            var startPosition = session.Player.Position;
            session.Player.Position = ClampToGameplayBounds(session.Player.Position + direction * session.DashDistance);
            session.Player.InvulnerabilityTimer = Mathf.Max(session.Player.InvulnerabilityTimer, session.DashInvulnerabilitySeconds);
            session.DashCooldownRemaining = session.DashCooldownSeconds;
            AddDashEffects(session, startPosition, session.Player.Position);
            return true;
        }

        public bool ApplyDamage(RunSession session, float damage)
        {
            if (session.Paused || session.AwaitingRewardedRevive)
            {
                return false;
            }

            if (session.Player.InvulnerabilityTimer > 0f)
            {
                return false;
            }

            if (session.Player.Shield > 0f)
            {
                session.Player.Shield -= damage;
                if (session.Player.Shield < 0f)
                {
                    session.Player.Hull += session.Player.Shield;
                    session.Player.Shield = 0f;
                }
            }
            else
            {
                session.Player.Hull -= damage;
            }

            session.Player.InvulnerabilityTimer = 0.3f;
            if (session.Player.Hull > 0f)
            {
                return false;
            }

            if (session.ReviveCharges > 0)
            {
                session.ReviveCharges -= 1;
                session.Revived = true;
                session.Player.Hull = session.Player.MaxHull * 0.6f;
                session.Player.Shield = session.Player.MaxShield * 0.5f;
                session.Player.InvulnerabilityTimer = 1f;
                session.RewardMessage = "Backup spark triggered.";
                session.RewardMessageTimer = 2.8f;
                return true;
            }

            if (!session.RewardedReviveUsed)
            {
                session.AwaitingRewardedRevive = true;
                session.Paused = true;
                session.Player.Hull = 0f;
                session.RewardMessage = "Watch a clip to reignite your ship.";
                return false;
            }

            session.Failed = true;
            session.Player.Hull = 0f;
            return false;
        }

        private static Vector2 ClampToGameplayBounds(Vector2 position)
        {
            return new Vector2(
                Mathf.Clamp01(position.x),
                Mathf.Clamp(position.y, 0.08f, 0.92f));
        }

        private static void AddDashEffects(RunSession session, Vector2 startPosition, Vector2 endPosition)
        {
            session.CombatEffects.Add(new CombatEffectState
            {
                Position = startPosition,
                TargetPosition = endPosition,
                Color = new Color(0.34f, 1f, 0.92f, 0.9f),
                Width = 10f,
                TotalDuration = 0.22f,
                RemainingDuration = 0.22f,
                IsLine = true
            });

            session.CombatEffects.Add(new CombatEffectState
            {
                Position = endPosition,
                Color = new Color(0.34f, 1f, 0.92f, 0.78f),
                StartRadius = 16f,
                EndRadius = 42f,
                TotalDuration = 0.3f,
                RemainingDuration = 0.3f
            });
        }
    }
}
