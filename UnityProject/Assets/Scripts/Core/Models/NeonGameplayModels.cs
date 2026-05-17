using System;
using System.Collections.Generic;

namespace Cosmostar.Core.Models
{
    [Serializable]
    public struct NeonVector2
    {
        public float X;
        public float Y;

        public NeonVector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float SqrMagnitude => X * X + Y * Y;
        public float Magnitude => (float)Math.Sqrt(SqrMagnitude);

        public NeonVector2 Normalized
        {
            get
            {
                var magnitude = Magnitude;
                return magnitude <= 0.0001f ? Zero : new NeonVector2(X / magnitude, Y / magnitude);
            }
        }

        public static NeonVector2 Zero => new NeonVector2(0f, 0f);
        public static NeonVector2 Up => new NeonVector2(0f, 1f);

        public static NeonVector2 operator +(NeonVector2 a, NeonVector2 b)
        {
            return new NeonVector2(a.X + b.X, a.Y + b.Y);
        }

        public static NeonVector2 operator -(NeonVector2 a, NeonVector2 b)
        {
            return new NeonVector2(a.X - b.X, a.Y - b.Y);
        }

        public static NeonVector2 operator *(NeonVector2 value, float scalar)
        {
            return new NeonVector2(value.X * scalar, value.Y * scalar);
        }

        public static float Distance(NeonVector2 a, NeonVector2 b)
        {
            return (a - b).Magnitude;
        }

        public static NeonVector2 MoveTowards(NeonVector2 current, NeonVector2 target, float maxDistanceDelta)
        {
            var delta = target - current;
            var distance = delta.Magnitude;
            if (distance <= maxDistanceDelta || distance <= 0.0001f)
            {
                return target;
            }

            return current + delta.Normalized * maxDistanceDelta;
        }
    }

    public enum NeonRunStatus
    {
        Running,
        LevelUpDraft,
        GameOver,
        Victory
    }

    [Serializable]
    public sealed class NeonPlaneRunState
    {
        public NeonVector2 Position = new NeonVector2(0f, 0f);
        public NeonVector2 MovementTarget = new NeonVector2(0f, 0f);
        public NeonVector2 LastMoveDirection = NeonVector2.Up;
        public NeonPlayerStats Stats = new NeonPlayerStats();
        public float DashCooldownRemaining;
        public float InvulnerabilityRemaining;
        public float WeaponCooldownRemaining;
        public int Level = 1;
        public float XP;
        public float XPToNextLevel = 5f;
        public int CoinsCollected;
    }

    [Serializable]
    public sealed class NeonRunUpgradeState
    {
        public Dictionary<string, int> UpgradeLevels = new Dictionary<string, int>();
        public List<string> EvolvedWeapons = new List<string>();

        public int GetLevel(string upgradeId)
        {
            return UpgradeLevels.TryGetValue(upgradeId, out var level) ? level : 0;
        }
    }

    [Serializable]
    public sealed class NeonRunEnemyState
    {
        public string EnemyID = string.Empty;
        public NeonVector2 Position;
        public float HP;
        public float MaxHP;
        public float ContactDamage;
        public float Speed;
        public int XPDrop;
        public bool IsBoss;
    }

    [Serializable]
    public sealed class NeonRunProjectileState
    {
        public NeonVector2 Position;
        public NeonVector2 Velocity;
        public float Damage;
        public float Radius = 0.18f;
        public float RemainingLife = 2.5f;
        public int RemainingPierce;
        public bool FromPlayer = true;
    }

    [Serializable]
    public sealed class NeonXpShardState
    {
        public NeonVector2 Position;
        public float XPValue;
        public bool Collected;
    }

    [Serializable]
    public sealed class NeonDashTrailState
    {
        public NeonVector2 Start;
        public NeonVector2 End;
        public float DamagePerSecond;
        public float RemainingLifetime = 1.5f;
        public bool ExplodesOnExpire;
    }

    [Serializable]
    public sealed class NeonRunState
    {
        public NeonRunStatus Status = NeonRunStatus.Running;
        public float ElapsedSeconds;
        public NeonPlaneRunState Player = new NeonPlaneRunState();
        public NeonRunUpgradeState Build = new NeonRunUpgradeState();
        public List<NeonRunEnemyState> Enemies = new List<NeonRunEnemyState>();
        public List<NeonRunProjectileState> Projectiles = new List<NeonRunProjectileState>();
        public List<NeonXpShardState> XpShards = new List<NeonXpShardState>();
        public List<NeonDashTrailState> DashTrails = new List<NeonDashTrailState>();
        public List<NeonUpgradeDef> DraftChoices = new List<NeonUpgradeDef>();
        public HashSet<string> SpawnedBossIDs = new HashSet<string>();
        public int EnemiesKilled;
        public int BossesKilled;
        public string LastWarning = string.Empty;
    }
}
