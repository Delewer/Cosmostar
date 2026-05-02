using System.Collections.Generic;
using Cosmostar.Core.Models;
using UnityEngine;

namespace Cosmostar.Runtime.Gameplay
{
    public sealed class RunSession
    {
        public ShipDef Ship;
        public WeaponDef Weapon;
        public MissionDef Mission;
        public MetaModifiers Meta;
        public MissionRuleSet Rules;
        public RunBuildState Build = new RunBuildState();
        public RunDirector Director;
        public PlayerState Player = new PlayerState();
        public List<EnemyState> Enemies = new List<EnemyState>();
        public List<ProjectileState> Projectiles = new List<ProjectileState>();
        public List<AttackTelegraphState> AttackTelegraphs = new List<AttackTelegraphState>();
        public List<RunAnomalyTelegraphState> AnomalyTelegraphs = new List<RunAnomalyTelegraphState>();
        public List<CombatEffectState> CombatEffects = new List<CombatEffectState>();
        public List<PickupState> Pickups = new List<PickupState>();
        public List<UpgradeDef> DraftChoices = new List<UpgradeDef>();
        public float SpawnAccumulator;
        public float WeaponCooldown;
        public float DroneCooldown;
        public float OverclockCooldown;
        public float DashCooldownRemaining;
        public float DashCooldownSeconds = 4f;
        public float DashDistance = 0.18f;
        public float DashInvulnerabilitySeconds = 0.28f;
        public Vector2 LastMoveDirection = Vector2.up;
        public float ReactorCharge;
        public float ReactorChargeRequired = 100f;
        public float ReactorSurgeDamage = 72f;
        public float ReactorSurgeRadius = 0.62f;
        public float NextAnomalySecond = -1f;
        public float BossStartSecond = -1f;
        public int AnomalyEventsTriggered;
        public int ComboCount;
        public int BestComboCount;
        public float ComboTimer;
        public float ComboWindowSeconds = 3.5f;
        public int Kills;
        public int Grazes;
        public int PickupsCollected;
        public int SalvageCollected;
        public bool BossSpawned;
        public bool BossDefeated;
        public bool DraftOpen;
        public bool Failed;
        public bool Completed;
        public bool Revived;
        public bool Paused;
        public bool TutorialOpen;
        public bool AwaitingRewardedRevive;
        public bool RewardedReviveUsed;
        public int RerollsRemaining;
        public int ReviveCharges;
        public bool EmergencyBarrierUsed;
        public float EmergencyBarrierThreshold = 0.32f;
        public float RewardMessageTimer;
        public string RewardMessage = string.Empty;
    }

    public sealed class AttackTelegraphState
    {
        public EnemyState Source;
        public Vector2 Origin;
        public Vector2 Direction;
        public float Damage;
        public float SpeedScale;
        public bool IsBossShot;
        public float TotalDuration;
        public float RemainingDuration;
    }

    public enum RunAnomalyShape
    {
        Circle,
        VerticalLane
    }

    public sealed class RunAnomalyTelegraphState
    {
        public RunAnomalyKind Kind;
        public RunAnomalyShape Shape;
        public Vector2 Position;
        public float Radius;
        public float Width;
        public float Damage;
        public bool DamagesEnemies;
        public float TotalDuration;
        public float RemainingDuration;
    }

    public sealed class CombatEffectState
    {
        public Vector2 Position;
        public Vector2 TargetPosition;
        public Color Color;
        public float StartRadius;
        public float EndRadius;
        public float Width;
        public float TotalDuration;
        public float RemainingDuration;
        public bool IsLine;
    }

    public sealed class PlayerState
    {
        public Vector2 Position = new Vector2(0.5f, 0.18f);
        public float Hull;
        public float MaxHull;
        public float Shield;
        public float MaxShield;
        public float InvulnerabilityTimer;
    }

    public sealed class EnemyState
    {
        public EnemyDef Def;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Hull;
        public float MaxHull;
        public float FireCooldown;
        public float SlowTimer;
        public int BossPhaseIndex = 1;
        public float Oscillator;
    }

    public sealed class ProjectileState
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Damage;
        public float Radius;
        public float RemainingLife;
        public bool FromPlayer;
        public bool IsCritical;
        public bool GrazedByPlayer;
        public int RemainingPierce;
        public bool AppliesSlow;
        public bool CanChain;
        public bool Homes;
        public float HomingStrength;
        public Color Color;
    }

    public sealed class PickupState
    {
        public Vector2 Position;
        public float Value;
        public float Radius;
    }
}
