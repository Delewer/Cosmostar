export const SLOT_TYPES = ['Weapon', 'Wings', 'Engine', 'Hull', 'Core', 'Radar'];
export const RARITIES = ['Common', 'Uncommon', 'Rare', 'Epic', 'Legendary'];

export const catalog = {
  durationSeconds: 600,
  baseStats: {
    attackDamage: 10,
    fireRate: 1,
    movementSpeed: 5,
    maxHP: 100,
    armor: 0,
    criticalChance: 0.05,
    criticalDamage: 2,
    magnetRange: 2.5,
    startingEnergy: 0,
    dashCooldown: 4,
    dashDistance: 4,
    specialChargeSpeed: 1,
    xpModifier: 1,
    coinBonus: 1
  },
  startingEquipment: {
    Weapon: 'basic_blaster',
    Wings: 'starter_wings',
    Engine: 'old_engine',
    Hull: 'light_hull',
    Core: 'small_battery',
    Radar: 'basic_scanner'
  },
  equipment: [
    item('basic_blaster', 'Basic Blaster', 'Weapon', { attackDamage: 10, fireRate: 1 }),
    item('twin_cannon', 'Twin Cannon', 'Weapon', { attackDamage: 13, fireRatePercent: 0.08 }),
    item('plasma_needle', 'Plasma Needle', 'Weapon', { attackDamage: 16, criticalChance: 0.02 }),
    item('railgun_nose', 'Railgun Nose', 'Weapon', { attackDamage: 24, fireRatePercent: -0.1 }),
    item('starter_wings', 'Starter Wings', 'Wings', { movementSpeed: 0.2 }),
    item('falcon_wings', 'Falcon Wings', 'Wings', { movementSpeed: 0.45, fireRatePercent: 0.04 }),
    item('combat_wings', 'Combat Wings', { fireRatePercent: 0.08, criticalChance: 0.015 }, 'Wings'),
    item('neon_wings', 'Neon Wings', 'Wings', { movementSpeed: 0.35, criticalChance: 0.025 }, 'After dash, gain +20% FireRate for 2 seconds.'),
    item('old_engine', 'Old Engine', 'Engine', { movementSpeed: 0.25, dashCooldown: -0.1 }),
    item('turbo_engine', 'Turbo Engine', 'Engine', { movementSpeed: 0.55, dashCooldown: -0.3 }),
    item('ion_engine', 'Ion Engine', 'Engine', { dashCooldown: -0.45, dashDistance: 0.4 }),
    item('phantom_engine', 'Phantom Engine', 'Engine', { movementSpeed: 0.35, dashCooldown: -0.55 }, 'Dash leaves a damaging trail.'),
    item('light_hull', 'Light Hull', 'Hull', { maxHP: 12 }),
    item('steel_hull', 'Steel Hull', 'Hull', { maxHP: 28, armor: 1 }),
    item('guardian_frame', 'Guardian Frame', 'Hull', { maxHP: 34, armor: 2 }, 'Block the first hit every 30 seconds.'),
    item('solar_shield_hull', 'Solar Shield Hull', 'Hull', { maxHP: 24, armor: 3 }, 'Gain shield when HP drops below 30%.'),
    item('small_battery', 'Small Battery', 'Core', { startingEnergy: 10, specialChargeSpeedPercent: 0.02 }),
    item('fusion_core', 'Fusion Core', 'Core', { startingEnergy: 18, xpModifierPercent: 0.04 }),
    item('plasma_core', 'Plasma Core', 'Core', { specialChargeSpeedPercent: 0.12, xpModifierPercent: 0.06 }),
    item('overdrive_core', 'Overdrive Core', 'Core', { startingEnergy: 25, specialChargeSpeedPercent: 0.16 }, 'After leveling up, gain a temporary damage boost.'),
    item('basic_scanner', 'Basic Scanner', 'Radar', { magnetRange: 0.35 }),
    item('magnet_scanner', 'Magnet Scanner', 'Radar', { magnetRange: 0.9, xpModifierPercent: 0.03 }),
    item('hunter_radar', 'Hunter Radar', 'Radar', { criticalChance: 0.03, coinBonusPercent: 0.04 }),
    item('quantum_sensor', 'Quantum Sensor', 'Radar', { magnetRange: 0.7, coinBonusPercent: 0.1 }, 'Boss rewards improved.')
  ],
  upgrades: [
    upgrade('plasma_blaster', 'Plasma Blaster', 'Weapon', 'Unlock and improve plasma projectiles.', { attackDamagePercent: 0.12 }, 'attack_boost', 'plasma_storm'),
    upgrade('homing_missiles', 'Homing Missiles', 'Weapon', 'Missiles target nearby enemies.', { fireRatePercent: 0.08 }, 'cooldown_reduction', 'rocket_swarm'),
    upgrade('laser_wings', 'Laser Wings', 'Weapon', 'Side laser beam attacks.', { criticalChance: 0.02 }),
    upgrade('orbit_blades', 'Orbit Blades', 'Weapon', 'Energy blades rotate around the plane.', { armor: 0.5 }),
    upgrade('attack_boost', 'Attack Boost', 'Passive', '+10% damage per level.', { attackDamagePercent: 0.1 }),
    upgrade('fire_rate_boost', 'Fire Rate Boost', 'Passive', '+10% fire rate per level.', { fireRatePercent: 0.1 }),
    upgrade('movement_speed_boost', 'Movement Speed Boost', 'Passive', '+8% movement speed per level.', { movementSpeedPercent: 0.08 }),
    upgrade('max_hp_boost', 'Max HP Boost', 'Passive', '+15 max HP per level.', { maxHP: 15 }),
    upgrade('armor_boost', 'Armor Boost', 'Passive', '+1 armor per level.', { armor: 1 }),
    upgrade('magnet_boost', 'Magnet Boost', 'Passive', '+15% magnet range per level.', { magnetRangePercent: 0.15 }),
    upgrade('critical_chance_boost', 'Critical Chance Boost', 'Passive', '+4% critical chance per level.', { criticalChance: 0.04 }),
    upgrade('cooldown_reduction', 'Cooldown Reduction', 'Passive', 'Dash and weapons recover faster.', { dashCooldown: -0.12 }),
    upgrade('xp_gain_boost', 'XP Gain Boost', 'Passive', '+10% XP gain per level.', { xpModifierPercent: 0.1 }),
    upgrade('longer_trail', 'Longer Trail', 'Trail', 'Dash trail lasts longer.', {}),
    upgrade('trail_damage_boost', 'Trail Damage Boost', 'Trail', 'Dash trail deals more damage.', {}),
    upgrade('trail_explosion', 'Trail Explosion', 'Trail', 'Trail explodes at the end.', {})
  ],
  enemies: {
    chaser_drone: enemy('chaser_drone', 'Chaser Drone', 20, 10, 2, 1, 'chaser'),
    fast_wing: enemy('fast_wing', 'Fast Wing', 12, 8, 3.5, 1, 'fast'),
    shooter_drone: enemy('shooter_drone', 'Shooter Drone', 30, 8, 1.5, 2, 'shooter'),
    shield_drone: enemy('shield_drone', 'Shield Drone', 70, 12, 1.1, 3, 'tank'),
    mine_carrier: enemy('mine_carrier', 'Mine Carrier', 45, 14, 0.9, 3, 'mine'),
    splitter_orb: enemy('splitter_orb', 'Splitter Orb', 36, 9, 1.4, 2, 'splitter'),
    elite_chaser: enemy('elite_chaser', 'Elite Chaser', 95, 18, 2.35, 6, 'chaser', true),
    elite_shooter: enemy('elite_shooter', 'Elite Shooter', 110, 14, 1.4, 7, 'shooter', true)
  },
  waves: [
    wave(0, 60, 0.9, ['chaser_drone', 'fast_wing']),
    wave(60, 120, 1.35, ['chaser_drone', 'fast_wing', 'shooter_drone']),
    wave(120, 180, 1.9, ['chaser_drone', 'fast_wing', 'shooter_drone', 'shield_drone'], 'WARNING: SKY REAPER APPROACHING', 170),
    wave(180, 240, 1.25, ['shooter_drone', 'fast_wing']),
    wave(240, 360, 2.55, ['shooter_drone', 'shield_drone', 'mine_carrier', 'splitter_orb', 'fast_wing'], 'NEON HYDRA APPROACHING', 350),
    wave(360, 420, 1.7, ['shooter_drone', 'shield_drone', 'splitter_orb']),
    wave(420, 450, 3.0, ['chaser_drone', 'fast_wing', 'mine_carrier', 'elite_chaser'], 'VIPER ACE INCOMING', 442),
    wave(450, 525, 3.25, ['fast_wing', 'shooter_drone', 'mine_carrier', 'splitter_orb', 'elite_chaser'], 'BOMBARDIER PRIME INCOMING', 517),
    wave(525, 570, 3.5, ['chaser_drone', 'fast_wing', 'shield_drone', 'mine_carrier', 'splitter_orb', 'elite_chaser', 'elite_shooter']),
    wave(570, 600, 4.25, ['chaser_drone', 'fast_wing', 'shooter_drone', 'shield_drone', 'mine_carrier', 'splitter_orb', 'elite_chaser', 'elite_shooter'], 'FINAL BOSS INCOMING', 590)
  ],
  bosses: [
    boss('sky_reaper', 'Sky Reaper', 180, 950, 18, 9, 'WARNING: SKY REAPER APPROACHING', { rewardCoins: 35, dropTier: 'Uncommon', pattern: 'reaper' }),
    boss('neon_hydra', 'Neon Hydra', 360, 2200, 24, 12, 'NEON HYDRA APPROACHING', { rewardCoins: 55, dropTier: 'Rare', pattern: 'hydra' }),
    boss('viper_ace', 'Viper Ace', 450, 850, 20, 10, 'VIPER ACE INCOMING', { mini: true, rewardCoins: 18, dropTier: 'Uncommon', pattern: 'reaper' }),
    boss('bombardier_prime', 'Bombardier Prime', 525, 1250, 22, 11, 'BOMBARDIER PRIME INCOMING', { mini: true, rewardCoins: 24, dropTier: 'Rare', pattern: 'hydra' }),
    boss('eclipse_core', 'Eclipse Core', 600, 4800, 30, 15, 'FINAL BOSS INCOMING', { rewardCoins: 90, dropTier: 'Rare', pattern: 'eclipse' })
  ],
  rewards: {
    baseCoins: 18,
    coinPerKill: 1,
    bossCoinBonus: 40,
    miniBossCoinBonus: 18,
    survivalMinuteCoins: 3,
    equipmentDrops: [
      { minBosses: 0, minMiniBosses: 0, rarity: 'Common', chance: 0.32, killBonus: 0.0015 },
      { minBosses: 1, minMiniBosses: 0, rarity: 'Uncommon', chance: 0.52, killBonus: 0.001 },
      { minBosses: 2, minMiniBosses: 0, rarity: 'Rare', chance: 0.62, killBonus: 0.0008 },
      { minBosses: 2, minMiniBosses: 1, rarity: 'Rare', chance: 0.78, killBonus: 0.0008 },
      { minBosses: 3, minMiniBosses: 2, rarity: 'Rare', chance: 1, killBonus: 0 },
      { minBosses: 3, minMiniBosses: 2, rarity: 'Epic', chance: 0.22, killBonus: 0.0005 }
    ]
  }
};

export function createDefaultSave() {
  const inventory = [];
  const equipped = {};
  for (const slot of SLOT_TYPES) {
    const id = catalog.startingEquipment[slot];
    inventory.push({ instanceId: `${id}_001`, itemId: id, rarity: 'Common', level: 1 });
    equipped[slot] = id;
  }
  return { coins: 120, materials: 0, inventory, equipped, completedRuns: 0, bestSurvivalTime: 0, bossesDefeated: 0, settings: { sound: true, music: true, screenShake: true, reducedMotion: false } };
}

export function computeStats(save) {
  const stats = { ...catalog.baseStats };
  for (const slot of SLOT_TYPES) {
    const itemId = save.equipped[slot];
    const owned = save.inventory.find((entry) => entry.itemId === itemId);
    const def = catalog.equipment.find((entry) => entry.id === itemId);
    if (!owned || !def) continue;
    const rarityMultiplier = 1 + RARITIES.indexOf(owned.rarity) * 0.35;
    const levelMultiplier = 1 + Math.max(0, owned.level - 1) * 0.08;
    const milestoneMultiplier = 1 + Math.floor(owned.level / 5) * 0.05;
    applyStatMap(stats, def.stats, rarityMultiplier * levelMultiplier * milestoneMultiplier);
  }
  stats.maxHP = Math.max(1, stats.maxHP);
  stats.currentHP = stats.maxHP;
  stats.dashCooldown = Math.max(0.6, stats.dashCooldown);
  return stats;
}


export function getEquipmentDef(itemId) {
  return catalog.equipment.find((entry) => entry.id === itemId);
}

export function getOwnedEquipment(save, instanceId) {
  return save.inventory.find((entry) => entry.instanceId === instanceId);
}

export function getEquippedInstance(save, slot) {
  const equippedItemId = save.equipped[slot];
  return save.inventory.find((entry) => entry.itemId === equippedItemId);
}

export function equipEquipment(save, instanceId) {
  const owned = getOwnedEquipment(save, instanceId);
  if (!owned) return false;
  const def = getEquipmentDef(owned.itemId);
  if (!def) return false;
  save.equipped[def.slot] = owned.itemId;
  return true;
}

export function unequipEquipment(save, slot) {
  if (!SLOT_TYPES.includes(slot)) return false;
  save.equipped[slot] = '';
  return true;
}

export function getEquipmentUpgradeCost(owned) {
  if (!owned) return Infinity;
  return 20 + owned.level * 10 + RARITIES.indexOf(owned.rarity) * 25;
}

export function upgradeEquipment(save, instanceId) {
  const owned = getOwnedEquipment(save, instanceId);
  if (!owned || owned.level >= 20) return false;
  const cost = getEquipmentUpgradeCost(owned);
  if (save.coins < cost) return false;
  save.coins -= cost;
  owned.level += 1;
  return true;
}

export function mergeEquipment(save, itemId, rarity) {
  const rarityIndex = RARITIES.indexOf(rarity);
  if (rarityIndex < 0 || rarityIndex >= RARITIES.indexOf('Legendary')) return null;
  const duplicates = save.inventory
    .filter((entry) => entry.itemId === itemId && entry.rarity === rarity)
    .sort((a, b) => a.level - b.level || a.instanceId.localeCompare(b.instanceId))
    .slice(0, 3);
  if (duplicates.length < 3) return null;
  const consumed = new Set(duplicates.map((entry) => entry.instanceId));
  save.inventory = save.inventory.filter((entry) => !consumed.has(entry.instanceId));
  const merged = {
    instanceId: `${itemId}_${RARITIES[rarityIndex + 1].toLowerCase()}_${Date.now()}_${Math.floor(Math.random() * 10000)}`,
    itemId,
    rarity: RARITIES[rarityIndex + 1],
    level: 1
  };
  save.inventory.push(merged);
  return merged;
}

export function rollEquipmentRewards(run) {
  const drops = catalog.rewards.equipmentDrops
    .filter((drop) => (run.bossesKilled ?? 0) >= drop.minBosses && (run.miniBossesKilled ?? 0) >= (drop.minMiniBosses ?? 0) && Math.random() <= Math.min(1, drop.chance + (run.kills ?? 0) * (drop.killBonus ?? 0)))
    .map((drop) => drop.rarity);
  if (drops.length === 0 && (run.kills ?? 0) >= 25) {
    drops.push('Common');
  }
  return drops;
}

export function addRewardEquipment(save, rarity = 'Common') {
  const def = catalog.equipment[Math.floor(Math.random() * catalog.equipment.length)];
  const owned = {
    instanceId: `${def.id}_${rarity.toLowerCase()}_${Date.now()}_${Math.floor(Math.random() * 10000)}`,
    itemId: def.id,
    rarity,
    level: 1
  };
  save.inventory.push(owned);
  return owned;
}

export function summarizeStats(stats) {
  return [
    `ATK ${stats.attackDamage.toFixed(1)}`,
    `Fire ${stats.fireRate.toFixed(2)}/s`,
    `Speed ${stats.movementSpeed.toFixed(1)}`,
    `HP ${stats.maxHP.toFixed(0)}`,
    `Armor ${stats.armor.toFixed(1)}`,
    `Magnet ${stats.magnetRange.toFixed(1)}`,
    `Dash ${stats.dashCooldown.toFixed(1)}s`
  ];
}

export function createRun(save) {
  return {
    status: 'running',
    elapsed: 0,
    player: {
      x: 0,
      y: 0,
      targetX: 0,
      targetY: 0,
      lastDx: 0,
      lastDy: -1,
      stats: computeStats(save),
      dashCooldown: 0,
      invulnerable: 0,
      weaponCooldown: 0,
      level: 1,
      xp: 0,
      xpToNext: 5,
      coins: 0
    },
    build: { levels: {}, evolved: [] },
    enemies: [],
    projectiles: [],
    xpShards: [],
    trails: [],
    enemyProjectiles: [],
    dangerZones: [],
    effects: [],
    feedbackEvents: [],
    draftChoices: [],
    spawnedBosses: new Set(),
    kills: 0,
    bossesKilled: 0,
    miniBossesKilled: 0,
    defeatedBossRewards: [],
    spawnAccumulator: 0,
    bossAttackTimer: 1.2,
    screenShake: 0,
    message: 'Survive for 10 minutes. Bosses at 3:00, 6:00, 10:00.'
  };
}

export function setMovementTarget(run, x, y) {
  run.player.targetX = clamp(x, -1, 1);
  run.player.targetY = clamp(y, -1, 1);
}

export function dash(run) {
  if (run.status !== 'running' || run.player.dashCooldown > 0) return false;
  const direction = normalize(run.player.lastDx, run.player.lastDy || -1);
  const start = { x: run.player.x, y: run.player.y };
  const distance = run.player.stats.dashDistance / 10;
  run.player.x = clamp(run.player.x + direction.x * distance, -1, 1);
  run.player.y = clamp(run.player.y + direction.y * distance, -1, 1);
  run.player.dashCooldown = run.player.stats.dashCooldown;
  run.player.invulnerable = Math.max(run.player.invulnerable, 0.28);
  run.trails.push({
    start,
    end: { x: run.player.x, y: run.player.y },
    life: run.build.levels.longer_trail ? 2.2 : 1.5,
    damagePerSecond: run.player.stats.attackDamage * (run.build.levels.trail_damage_boost ? 1.8 : 1),
    explodes: Boolean(run.build.levels.trail_explosion)
  });
  return true;
}

export function updateRun(run, deltaSeconds) {
  if (run.status !== 'running') return run;
  run.feedbackEvents = [];
  const previousElapsed = run.elapsed;
  run.elapsed += Math.max(0, deltaSeconds);
  run.player.dashCooldown = Math.max(0, run.player.dashCooldown - deltaSeconds);
  run.player.invulnerable = Math.max(0, run.player.invulnerable - deltaSeconds);
  run.player.weaponCooldown = Math.max(0, run.player.weaponCooldown - deltaSeconds);
  run.screenShake = Math.max(0, run.screenShake - deltaSeconds);
  movePlayer(run, deltaSeconds);
  updateTimeline(run, previousElapsed);
  updateAutoFire(run);
  updateProjectiles(run, deltaSeconds);
  updateBossAttacks(run, deltaSeconds);
  updateEnemyProjectiles(run, deltaSeconds);
  updateDangerZones(run, deltaSeconds);
  updateEnemies(run, deltaSeconds);
  updateTrails(run, deltaSeconds);
  cleanupDefeated(run);
  updateXp(run, deltaSeconds);
  updateEffects(run, deltaSeconds);
  spawnWaveEnemies(run, deltaSeconds);
  return run;
}

export function chooseUpgrade(run, upgradeId) {
  if (run.status !== 'level-up') return false;
  const upgradeDef = run.draftChoices.find((entry) => entry.id === upgradeId);
  if (!upgradeDef) return false;
  const current = run.build.levels[upgradeId] || 0;
  if (current >= upgradeDef.maxLevel) return false;
  run.build.levels[upgradeId] = current + 1;
  applyStatMap(run.player.stats, upgradeDef.stats, 1);
  if (upgradeDef.evolution && (run.build.levels[upgradeDef.requiredPassive] || 0) > 0 && run.build.levels[upgradeId] >= upgradeDef.maxLevel) {
    run.build.evolved.push(upgradeDef.evolution);
  }
  run.draftChoices = [];
  run.status = 'running';
  return true;
}

export function formatTime(seconds) {
  const clamped = Math.max(0, Math.floor(seconds));
  const minutes = Math.floor(clamped / 60).toString().padStart(2, '0');
  const secs = (clamped % 60).toString().padStart(2, '0');
  return `${minutes}:${secs}`;
}

export function calculateRewards(run) {
  const survivalCoins = Math.floor(Math.min(run.elapsed, catalog.durationSeconds) / 60) * catalog.rewards.survivalMinuteCoins;
  const bossCoins = run.defeatedBossRewards?.reduce((total, reward) => total + (reward.coins ?? 0), 0) ?? ((run.bossesKilled ?? 0) * catalog.rewards.bossCoinBonus + (run.miniBossesKilled ?? 0) * catalog.rewards.miniBossCoinBonus);
  const coins = Math.round(catalog.rewards.baseCoins + run.kills * catalog.rewards.coinPerKill + bossCoins + survivalCoins + run.player.coins);
  const materials = Math.max(1, (run.bossesKilled ?? 0) * 2 + (run.miniBossesKilled ?? 0) + Math.floor(run.kills / 30));
  const dropSummary = run.bossesKilled >= 3 && run.miniBossesKilled >= 2 ? 'Guaranteed Rare equipment, Epic chance' : run.bossesKilled >= 2 || run.miniBossesKilled >= 2 ? 'Rare equipment chance' : run.bossesKilled >= 1 || run.miniBossesKilled >= 1 ? 'Uncommon equipment chance' : 'Common equipment chance';
  return { coins, materials, item: dropSummary };
}

function movePlayer(run, deltaSeconds) {
  const dx = run.player.targetX - run.player.x;
  const dy = run.player.targetY - run.player.y;
  const distance = Math.hypot(dx, dy);
  if (distance > 0.001) {
    run.player.lastDx = dx / distance;
    run.player.lastDy = dy / distance;
  }
  const step = (run.player.stats.movementSpeed / 10) * deltaSeconds;
  if (distance <= step || distance <= 0.001) {
    run.player.x = run.player.targetX;
    run.player.y = run.player.targetY;
  } else {
    run.player.x += (dx / distance) * step;
    run.player.y += (dy / distance) * step;
  }
}

function updateTimeline(run, previousElapsed) {
  for (const waveDef of catalog.waves) {
    if (waveDef.warningSecond >= 0 && waveDef.warningSecond > previousElapsed && waveDef.warningSecond <= run.elapsed) {
      run.message = waveDef.warning;
    }
  }
  for (const bossDef of catalog.bosses) {
    if (bossDef.time > previousElapsed && bossDef.time <= run.elapsed && !run.spawnedBosses.has(bossDef.id)) {
      spawnBoss(run, bossDef);
      run.spawnedBosses.add(bossDef.id);
      run.message = bossDef.warning;
    }
  }
}

function updateAutoFire(run) {
  if (run.player.weaponCooldown > 0 || run.enemies.length === 0) return;
  const target = nearestEnemy(run);
  if (!target) return;
  const direction = normalize(target.x - run.player.x, target.y - run.player.y);
  const critical = Math.random() <= run.player.stats.criticalChance;
  run.projectiles.push({
    x: run.player.x,
    y: run.player.y,
    vx: direction.x * 1.8,
    vy: direction.y * 1.8,
    damage: run.player.stats.attackDamage * (critical ? run.player.stats.criticalDamage : 1),
    radius: critical ? 0.24 : 0.18,
    pierce: run.build.levels.plasma_blaster >= 5 ? 1 : 0,
    life: 2.5
  });
  run.player.weaponCooldown = 1 / Math.max(0.1, run.player.stats.fireRate);
}

function updateProjectiles(run, deltaSeconds) {
  for (let projectileIndex = run.projectiles.length - 1; projectileIndex >= 0; projectileIndex -= 1) {
    const projectile = run.projectiles[projectileIndex];
    projectile.x += projectile.vx * deltaSeconds;
    projectile.y += projectile.vy * deltaSeconds;
    projectile.life -= deltaSeconds;
    for (const target of run.enemies) {
      if (distance(projectile.x, projectile.y, target.x, target.y) > projectile.radius + 0.12) continue;
      damageEnemy(run, target, projectile.damage, 'projectile', projectile.x, projectile.y);
      if (projectile.pierce > 0) {
        projectile.pierce -= 1;
      } else {
        run.projectiles.splice(projectileIndex, 1);
      }
      break;
    }
    if (projectileIndex >= run.projectiles.length) continue;
    if (projectile.life <= 0 || Math.abs(projectile.x) > 1.2 || Math.abs(projectile.y) > 1.2) {
      run.projectiles.splice(projectileIndex, 1);
    }
  }
}


function updateBossAttacks(run, deltaSeconds) {
  const bossTargets = run.enemies.filter((target) => target.boss);
  if (!bossTargets.length) return;
  run.bossAttackTimer -= deltaSeconds;
  if (run.bossAttackTimer > 0) return;
  const bossTarget = bossTargets[0];
  const hpRatio = bossTarget.hp / bossTarget.maxHP;
  bossTarget.phase = hpRatio <= 0.25 ? 3 : hpRatio <= 0.5 ? 2 : 1;
  bossTarget.attackMode = resolveBossAttackMode(bossTarget);
  run.bossAttackTimer = Math.max(0.42, (bossTarget.id === 'eclipse_core' ? 1.25 : 1.45) - bossTarget.phase * 0.22);

  if ((bossTarget.pattern ?? bossTarget.id) === 'reaper') {
    spawnDangerZone(run, run.player.x, run.player.y, 0.2 + bossTarget.phase * 0.025, bossTarget.damage * 0.65, 0.58, bossTarget.id);
    spawnConeBullets(run, bossTarget, run.player.x - bossTarget.x, run.player.y - bossTarget.y, 3 + bossTarget.phase * 2, 0.42 + bossTarget.phase * 0.08, Math.PI / 3);
    return;
  }

  if ((bossTarget.pattern ?? bossTarget.id) === 'hydra') {
    spawnRadialBullets(run, bossTarget, 6 + bossTarget.phase * 4, 0.42 + bossTarget.phase * 0.08, bossTarget.phase % 2 ? 0 : Math.PI / 8);
    if (bossTarget.phase >= 2) {
      spawnDangerZone(run, Math.sin(run.elapsed) * 0.55, Math.cos(run.elapsed * 0.7) * 0.55, 0.24, bossTarget.damage * 0.7, 0.72, bossTarget.id);
    }
    return;
  }

  spawnRadialBullets(run, bossTarget, 8 + bossTarget.phase * 6, 0.45 + bossTarget.phase * 0.1, run.elapsed * 0.6);
  spawnDangerZone(run, run.player.x, run.player.y, 0.24 + bossTarget.phase * 0.03, bossTarget.damage * 0.75, 0.62, bossTarget.id);
  if (bossTarget.phase >= 2) {
    spawnRotatingLaserWarnings(run, bossTarget.phase);
  }
}

function resolveBossAttackMode(bossTarget) {
  if ((bossTarget.pattern ?? bossTarget.id) === 'reaper') return bossTarget.phase === 1 ? 'charge-cone' : bossTarget.phase === 2 ? 'drone-cone' : 'reaper-barrage';
  if ((bossTarget.pattern ?? bossTarget.id) === 'hydra') return bossTarget.phase === 1 ? 'bullet-circle' : bossTarget.phase === 2 ? 'danger-meteors' : 'hydra-rage';
  return bossTarget.phase === 1 ? 'bullet-rings' : bossTarget.phase === 2 ? 'laser-arms' : 'eclipse-rage';
}

function spawnDangerZone(run, x, y, radius, damage, windup, source = 'boss') {
  run.dangerZones.push({ x, y, radius, damage, windup, life: windup + 0.28, source });
  pushBurst(run, 'dangerCharge', x, y, { count: 5, radius, life: windup, color: source === 'eclipse_core' ? '#ffef6a' : '#ff42df' });
}

function spawnRadialBullets(run, bossTarget, bulletCount, speed, offset = 0) {
  for (let index = 0; index < bulletCount; index += 1) {
    const angle = offset + (Math.PI * 2 * index) / bulletCount;
    run.enemyProjectiles.push({ x: bossTarget.x, y: bossTarget.y, vx: Math.cos(angle) * speed, vy: Math.sin(angle) * speed, damage: bossTarget.damage * 0.45, radius: 0.06, life: 4, source: bossTarget.id });
  }
}

function spawnConeBullets(run, bossTarget, dx, dy, bulletCount, speed, spreadRadians) {
  const baseAngle = Math.atan2(dy, dx);
  const startAngle = baseAngle - spreadRadians * 0.5;
  const step = bulletCount <= 1 ? 0 : spreadRadians / (bulletCount - 1);
  for (let index = 0; index < bulletCount; index += 1) {
    const angle = startAngle + step * index;
    run.enemyProjectiles.push({ x: bossTarget.x, y: bossTarget.y, vx: Math.cos(angle) * speed, vy: Math.sin(angle) * speed, damage: bossTarget.damage * 0.42, radius: 0.06, life: 4, source: bossTarget.id });
  }
}

function spawnRotatingLaserWarnings(run, phase) {
  const laneCount = phase === 3 ? 4 : 2;
  for (let lane = 0; lane < laneCount; lane += 1) {
    const angle = run.elapsed * 0.7 + (Math.PI * lane) / laneCount;
    spawnDangerZone(run, Math.cos(angle) * 0.42, Math.sin(angle) * 0.42, 0.16, 18 + phase * 5, 0.85, 'eclipse_core');
  }
}

function updateEnemyProjectiles(run, deltaSeconds) {
  for (let index = run.enemyProjectiles.length - 1; index >= 0; index -= 1) {
    const projectile = run.enemyProjectiles[index];
    projectile.x += projectile.vx * deltaSeconds;
    projectile.y += projectile.vy * deltaSeconds;
    projectile.life -= deltaSeconds;
    if (distance(projectile.x, projectile.y, run.player.x, run.player.y) <= projectile.radius + 0.09 && damagePlayer(run, projectile.damage, projectile.x, projectile.y)) {
      run.enemyProjectiles.splice(index, 1);
      continue;
    }
    if (projectile.life <= 0 || Math.abs(projectile.x) > 1.2 || Math.abs(projectile.y) > 1.2) run.enemyProjectiles.splice(index, 1);
  }
}

function updateDangerZones(run, deltaSeconds) {
  for (let index = run.dangerZones.length - 1; index >= 0; index -= 1) {
    const zone = run.dangerZones[index];
    zone.windup -= deltaSeconds;
    zone.life -= deltaSeconds;
    if (zone.windup <= 0 && !zone.fired) {
      zone.fired = true;
      pushBurst(run, 'dangerDetonate', zone.x, zone.y, { count: 12, radius: zone.radius, color: zone.source === 'eclipse_core' ? '#ffef6a' : '#ff42df' });
      if (distance(zone.x, zone.y, run.player.x, run.player.y) <= zone.radius) damagePlayer(run, zone.damage, zone.x, zone.y);
    }
    if (zone.life <= 0) run.dangerZones.splice(index, 1);
  }
}

function updateEnemies(run, deltaSeconds) {
  for (const target of run.enemies) {
    const direction = normalize(run.player.x - target.x, run.player.y - target.y);
    target.x += direction.x * (target.speed / 10) * deltaSeconds;
    target.y += direction.y * (target.speed / 10) * deltaSeconds;
    if (distance(target.x, target.y, run.player.x, run.player.y) <= 0.16) damagePlayer(run, target.damage, target.x, target.y);
  }
}

function updateTrails(run, deltaSeconds) {
  for (let trailIndex = run.trails.length - 1; trailIndex >= 0; trailIndex -= 1) {
    const trail = run.trails[trailIndex];
    for (const target of run.enemies) {
      if (distanceToSegment(target, trail.start, trail.end) <= 0.13) target.hp -= trail.damagePerSecond * deltaSeconds;
    }
    trail.life -= deltaSeconds;
    if (trail.life > 0) continue;
    if (trail.explodes) {
      for (const target of run.enemies) {
        if (distance(target.x, target.y, trail.end.x, trail.end.y) <= 0.28) damageEnemy(run, target, run.player.stats.attackDamage * 1.5, 'trailExplosion', trail.end.x, trail.end.y);
      }
    }
    run.trails.splice(trailIndex, 1);
  }
}

function cleanupDefeated(run) {
  for (let enemyIndex = run.enemies.length - 1; enemyIndex >= 0; enemyIndex -= 1) {
    const target = run.enemies[enemyIndex];
    if (target.hp > 0) continue;
    run.enemies.splice(enemyIndex, 1);
    run.kills += 1;
    pushFeedback(run, 'enemyDeath', { x: target.x, y: target.y, id: target.id, boss: Boolean(target.boss) });
    pushBurst(run, target.boss ? 'bossDeath' : 'enemyDeath', target.x, target.y, { count: target.boss ? 34 : 15, radius: target.boss ? 0.24 : 0.12, color: target.boss ? '#ff42df' : '#ff3f5e' });
    run.xpShards.push({ x: target.x, y: target.y, value: target.xp });
    if (Math.random() <= 0.2 * run.player.stats.coinBonus) run.player.coins += 1;
    if (target.boss) {
      run.defeatedBossRewards.push({ id: target.id, coins: target.rewardCoins ?? (target.mini ? catalog.rewards.miniBossCoinBonus : catalog.rewards.bossCoinBonus), rarity: target.dropTier ?? 'Common', mini: Boolean(target.mini) });
      if (target.mini) {
        run.miniBossesKilled += 1;
      } else {
        run.bossesKilled += 1;
      }
      if (target.id === 'eclipse_core') run.status = 'victory';
    }
  }
}

function updateXp(run, deltaSeconds) {
  for (let shardIndex = run.xpShards.length - 1; shardIndex >= 0; shardIndex -= 1) {
    const shard = run.xpShards[shardIndex];
    const pickupRange = Math.max(0.08, run.player.stats.magnetRange / 20);
    if (distance(shard.x, shard.y, run.player.x, run.player.y) > pickupRange) {
      const direction = normalize(run.player.x - shard.x, run.player.y - shard.y);
      shard.x += direction.x * pickupRange * deltaSeconds;
      shard.y += direction.y * pickupRange * deltaSeconds;
      continue;
    }
    run.player.xp += shard.value * run.player.stats.xpModifier;
    run.xpShards.splice(shardIndex, 1);
    pushFeedback(run, 'xpCollect', { x: run.player.x, y: run.player.y, value: shard.value });
    pushBurst(run, 'xpCollect', run.player.x, run.player.y, { count: 8, radius: 0.08, color: '#42ffc8', life: 0.28 });
    if (run.player.xp >= run.player.xpToNext) {
      openDraft(run);
      return;
    }
  }
}

function spawnWaveEnemies(run, deltaSeconds) {
  const activeWave = catalog.waves.find((entry) => run.elapsed >= entry.start && run.elapsed < entry.end);
  if (!activeWave) return;
  run.spawnAccumulator += activeWave.rate * deltaSeconds;
  while (run.spawnAccumulator >= 1) {
    run.spawnAccumulator -= 1;
    const enemyId = activeWave.enemies[Math.floor(Math.random() * activeWave.enemies.length)];
    spawnEnemy(run, enemyId);
  }
}

function openDraft(run) {
  run.player.level += 1;
  run.player.xp -= run.player.xpToNext;
  run.player.xpToNext = Math.ceil(run.player.xpToNext * 1.35 + 2);
  run.draftChoices = catalog.upgrades
    .filter((entry) => (run.build.levels[entry.id] || 0) < entry.maxLevel)
    .sort(() => Math.random() - 0.5)
    .slice(0, 3);
  run.status = run.draftChoices.length ? 'level-up' : 'running';
}

function spawnEnemy(run, enemyId) {
  const def = catalog.enemies[enemyId];
  if (!def) return;
  const angle = Math.random() * Math.PI * 2;
  const radius = 1.05;
  run.enemies.push({ ...def, x: Math.cos(angle) * radius, y: Math.sin(angle) * radius, hp: def.hp, maxHP: def.hp, boss: false });
}

function spawnBoss(run, bossDef) {
  run.enemies.push({ id: bossDef.id, name: bossDef.name, x: 0, y: 0.82, hp: bossDef.hp, maxHP: bossDef.hp, damage: bossDef.contactDamage, speed: bossDef.mini ? 0.62 : 0.5, xp: bossDef.mini ? 12 : 20, boss: true, mini: Boolean(bossDef.mini), pattern: bossDef.pattern, rewardCoins: bossDef.rewardCoins, dropTier: bossDef.dropTier, phase: 1, attackMode: 'opening', visualSeed: Math.random() });
  pushFeedback(run, 'bossSpawn', { x: 0, y: 0.82, id: bossDef.id });
  pushBurst(run, 'bossSpawn', 0, 0.82, { count: 38, radius: 0.28, color: bossDef.id === 'eclipse_core' ? '#ffef6a' : '#ff42df', life: 0.9 });
}

function damageEnemy(run, target, amount, source, x = target.x, y = target.y) {
  target.hp -= amount;
  pushFeedback(run, 'enemyHit', { x, y, id: target.id, boss: Boolean(target.boss), source, damage: amount });
  pushBurst(run, target.boss ? 'bossHit' : 'enemyHit', x, y, { count: target.boss ? 14 : 7, radius: target.boss ? 0.13 : 0.07, color: target.boss ? '#ff42df' : '#ff3f5e', life: 0.24 });
}

function damagePlayer(run, rawDamage, x = run.player.x, y = run.player.y) {
  if (run.player.invulnerable > 0 || run.status !== 'running') return false;
  const mitigated = Math.max(1, rawDamage - run.player.stats.armor);
  run.player.stats.currentHP -= mitigated;
  run.player.invulnerable = 0.3;
  run.screenShake = Math.max(run.screenShake, 0.22);
  pushFeedback(run, 'playerDamage', { x, y, damage: mitigated });
  pushBurst(run, 'playerDamage', run.player.x, run.player.y, { count: 18, radius: 0.12, color: '#ffef6a', life: 0.32 });
  if (run.player.stats.currentHP <= 0) {
    run.player.stats.currentHP = 0;
    run.status = 'game-over';
  }
  return true;
}

function pushFeedback(run, type, payload = {}) {
  run.feedbackEvents.push({ type, ...payload });
}

function pushBurst(run, type, x, y, options = {}) {
  const life = options.life ?? 0.38;
  const radius = options.radius ?? 0.1;
  const color = options.color ?? '#ffffff';
  const count = options.count ?? 10;
  const particles = Array.from({ length: count }, (_, index) => {
    const angle = (Math.PI * 2 * index) / count + Math.random() * 0.45;
    const speed = (0.22 + Math.random() * 0.36) * (type === 'bossDeath' ? 1.6 : 1);
    return {
      x,
      y,
      vx: Math.cos(angle) * speed,
      vy: Math.sin(angle) * speed,
      size: 0.008 + Math.random() * 0.018
    };
  });
  run.effects.push({ type, x, y, life, maxLife: life, radius, color, particles });
  if (run.effects.length > 70) run.effects.splice(0, run.effects.length - 70);
}

function updateEffects(run, deltaSeconds) {
  for (let index = run.effects.length - 1; index >= 0; index -= 1) {
    const effect = run.effects[index];
    effect.life -= deltaSeconds;
    for (const particle of effect.particles) {
      particle.x += particle.vx * deltaSeconds;
      particle.y += particle.vy * deltaSeconds;
      particle.vx *= 0.92;
      particle.vy *= 0.92;
    }
    if (effect.life <= 0) run.effects.splice(index, 1);
  }
}

function applyStatMap(stats, statMap, multiplier) {
  for (const [key, value] of Object.entries(statMap)) {
    if (key.endsWith('Percent')) {
      const stat = key.replace('Percent', '');
      stats[stat] *= 1 + value;
    } else {
      stats[key] += value * multiplier;
    }
  }
  stats.dashCooldown = Math.max(0.6, stats.dashCooldown);
}

function item(id, name, slotOrStats, statsOrSlot, special = '') {
  const slot = typeof slotOrStats === 'string' ? slotOrStats : statsOrSlot;
  const stats = typeof slotOrStats === 'string' ? statsOrSlot : slotOrStats;
  return { id, name, slot, stats, special, maxLevel: 20 };
}

function upgrade(id, name, category, description, stats, requiredPassive = '', evolution = '') {
  return { id, name, category, description, stats, requiredPassive, evolution, maxLevel: 5 };
}

function enemy(id, name, hp, damage, speed, xp, behavior, elite = false) {
  return { id, name, hp, damage, speed, xp, behavior, elite };
}

function wave(start, end, rate, enemies, warning = '', warningSecond = -1) {
  return { start, end, rate, enemies, warning, warningSecond };
}

function boss(id, name, time, hp, contactDamage, bulletDamage, warning, options = {}) {
  return { id, name, time, hp, contactDamage, bulletDamage, warning, ...options };
}

function clamp(value, min, max) {
  return Math.max(min, Math.min(max, value));
}

function normalize(x, y) {
  const magnitude = Math.hypot(x, y);
  return magnitude <= 0.0001 ? { x: 0, y: -1 } : { x: x / magnitude, y: y / magnitude };
}

function nearestEnemy(run) {
  let best = null;
  let bestDistance = Infinity;
  for (const target of run.enemies) {
    const candidate = distance(run.player.x, run.player.y, target.x, target.y);
    if (candidate < bestDistance) {
      best = target;
      bestDistance = candidate;
    }
  }
  return best;
}

function distance(ax, ay, bx, by) {
  return Math.hypot(ax - bx, ay - by);
}

function distanceToSegment(point, start, end) {
  const vx = end.x - start.x;
  const vy = end.y - start.y;
  const lengthSquared = vx * vx + vy * vy;
  if (lengthSquared <= 0.0001) return distance(point.x, point.y, start.x, start.y);
  const t = clamp(((point.x - start.x) * vx + (point.y - start.y) * vy) / lengthSquared, 0, 1);
  return distance(point.x, point.y, start.x + vx * t, start.y + vy * t);
}
