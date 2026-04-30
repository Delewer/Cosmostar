# Cosmostar Base Code (Unity)

Unity-ready gameplay foundation for a mobile neon roguelite shooter.

## Current gameplay loop implemented
1. Start run (`GameLoopManager` enters `Running`).
2. `SpawnDirector` spawns pooled enemies with scaling HP/speed over time.
3. Enemies die and drop pooled XP pickups.
4. Player collects XP -> `ExperienceSystem` levels up.
5. `RunProgressionCoordinator` opens upgrade selection and applies a chosen upgrade.
6. Stage timer triggers boss spawn/warning (`StageProgressionController`).
7. Boss can run multiple attack components (`BossRadialAttack`, `BossAimedVolley`, `BossMineBurst`).
8. Run ends on player death (loss) or boss defeat (victory).
9. `RunRewardsDistributor` grants meta currency through `MetaProgressionService`.

## Systems included
### Core + services
- `GameBootstrap`
- `ServiceLocator`
- `GameLoopManager`
- `AnalyticsService` (stub)
- `SaveService`
- `MetaProgressionService`

### Combat + actors
- Player: `PlayerController`, `PlayerDash`, `PlayerStats`, `PlayerExperienceCollector`
- Weapons/Projectiles: `WeaponController`, `ProjectileEmitter`, `Projectile`, `ProjectilePool`
- Enemies: `EnemyController`, `EnemyPool`, `SpawnDirector`, `EnemyShooter` (telegraphed burst support)
- Boss: `BossController`, `BossRadialAttack`, `BossAimedVolley`, `BossMineBurst`
- Shared combat types: `DamageContext`, `ContactDamage`, `IDamageable`

### Progression
- Run/session: `RunSessionTracker`, `RunProgressionCoordinator`, `RunRewardsDistributor`, `StageProgressionController`, `EnemyTelegraphWarningSystem`
- XP: `ExperienceSystem`, `ExperiencePickup`, `ExperiencePickupPool`
- Upgrades: `UpgradeData`, `UpgradeSystem`, `SynergyResolver`, `SynergyEffectApplier`

### UI binders
- `RunHudController`
- `UpgradeSelectionPanel`
- `BossWarningBanner`
- `DashButtonController`

## Scene wiring checklist (minimum playable)
### Boot scene
- Empty GameObject with:
  - `GameBootstrap`
  - `ServiceLocator`

### Run scene
- `GameLoopManager`
- Player prefab with:
  - `PlayerController`
  - `WeaponController`
  - `PlayerDash`
  - `PlayerExperienceCollector`
- `ProjectilePool` with projectile prefab assigned
- `EnemyPool` with enemy prefab assigned
- `ExperiencePickupPool` with pickup prefab assigned
- `SpawnDirector` wired to player, enemy pool, pickup pool
- `ExperienceSystem`
- `UpgradeSystem` (+ list of `UpgradeData` assets)
- `SynergyResolver`
- `RunProgressionCoordinator` wired to loop + XP + upgrade system
- `RunSessionTracker`
- `RunRewardsDistributor`
- `StageProgressionController` wired to loop + spawn director + boss prefab + player
- Optional boss attack components on boss prefab:
  - `BossRadialAttack`
  - `BossAimedVolley`
  - `BossMineBurst`

## Known limitations (current state)
- Analytics is logging-only (no backend SDK integration).
- Only `solar_flare` has a concrete gameplay effect; additional synergy effects still need implementation.
- No formal unit/playmode test suite yet.
- UI uses basic controller scripts; production art/polish flow still pending.

## Recommended next implementation tasks
1. Implement concrete gameplay effects per unlocked synergy (not just unlock events).
2. Integrate audio cues into `EnemyTelegraphWarningSystem` when telegraphs begin.
3. Add a boss pattern scheduler/state machine for phase-based attacks.
4. Add scriptable balance configs for wave tables and reward curves.
5. Add playmode smoke tests for run start, level-up flow, boss spawn, and reward payout.
