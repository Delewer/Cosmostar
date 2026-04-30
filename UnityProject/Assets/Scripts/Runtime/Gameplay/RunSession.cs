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
        public List<PickupState> Pickups = new List<PickupState>();
        public List<UpgradeDef> DraftChoices = new List<UpgradeDef>();
        public float SpawnAccumulator;
        public float WeaponCooldown;
        public float DroneCooldown;
        public float OverclockCooldown;
        public int Kills;
        public int PickupsCollected;
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
        public string RewardMessage = string.Empty;
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
        public int RemainingPierce;
        public bool AppliesSlow;
        public bool CanChain;
        public Color Color;
    }

    public sealed class PickupState
    {
        public Vector2 Position;
        public float Value;
        public float Radius;
    }
}
