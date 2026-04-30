using Cosmostar.Runtime.Gameplay;
using UnityEngine;

namespace Cosmostar.Runtime.Systems
{
    public sealed class PlayerController
    {
        public void TickMovement(RunSession session, Vector2 targetPosition, float deltaTime)
        {
            var speed = session.Ship.MoveSpeed * session.Meta.MoveSpeedMultiplier * session.Build.MoveSpeedMultiplier;
            session.Player.Position = Vector2.MoveTowards(session.Player.Position, targetPosition, speed * deltaTime);
            session.Player.Position = new Vector2(
                Mathf.Clamp01(session.Player.Position.x),
                Mathf.Clamp(session.Player.Position.y, 0.08f, 0.92f));

            if (session.Player.InvulnerabilityTimer > 0f)
            {
                session.Player.InvulnerabilityTimer -= deltaTime;
            }
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
    }
}
