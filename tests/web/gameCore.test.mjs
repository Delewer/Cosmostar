import assert from 'node:assert/strict';
import { addRewardEquipment, calculateRewards, catalog, chooseUpgrade, createDefaultSave, createRun, dash, equipEquipment, formatTime, getEquipmentUpgradeCost, mergeEquipment, rollEquipmentRewards, setMovementTarget, unequipEquipment, updateRun, upgradeEquipment } from '../../web/gameCore.js';

assert.equal(catalog.durationSeconds, 600);
assert.equal(catalog.bosses.map((boss) => boss.time).join(','), '180,360,450,525,600');
assert.equal(catalog.equipment.length, 24);
assert.equal(catalog.waves.length, 10);
assert.ok(catalog.waves[0].rate < catalog.waves[2].rate);
assert.ok(catalog.waves.find((wave) => wave.start === 570).rate > catalog.waves.find((wave) => wave.start === 525).rate);
assert.equal(catalog.bosses.filter((boss) => boss.mini).length, 2);

const save = createDefaultSave();
assert.equal(save.inventory.length, 6);
assert.equal(save.equipped.Weapon, 'basic_blaster');


const gearSave = createDefaultSave();
gearSave.inventory.push({ instanceId: 'turbo_extra', itemId: 'turbo_engine', rarity: 'Common', level: 1 });
assert.equal(equipEquipment(gearSave, 'turbo_extra'), true);
assert.equal(gearSave.equipped.Engine, 'turbo_engine');
const upgradeCost = getEquipmentUpgradeCost(gearSave.inventory.find((item) => item.instanceId === 'turbo_extra'));
gearSave.coins = upgradeCost;
assert.equal(upgradeEquipment(gearSave, 'turbo_extra'), true);
assert.equal(gearSave.coins, 0);
assert.equal(gearSave.inventory.find((item) => item.instanceId === 'turbo_extra').level, 2);
assert.equal(unequipEquipment(gearSave, 'Engine'), true);
assert.equal(gearSave.equipped.Engine, '');
gearSave.inventory.push({ instanceId: 'merge_a', itemId: 'basic_blaster', rarity: 'Common', level: 1 });
gearSave.inventory.push({ instanceId: 'merge_b', itemId: 'basic_blaster', rarity: 'Common', level: 1 });
const merged = mergeEquipment(gearSave, 'basic_blaster', 'Common');
assert.equal(merged.rarity, 'Uncommon');
const rewarded = addRewardEquipment(gearSave, 'Rare');
assert.equal(rewarded.rarity, 'Rare');
const guaranteedDropRun = { bossesKilled: 3, miniBossesKilled: 2 };
const rolledDrops = rollEquipmentRewards(guaranteedDropRun);
assert.ok(rolledDrops.includes('Rare'));

const run = createRun(save);
assert.equal(run.status, 'running');
assert.ok(run.player.stats.attackDamage > catalog.baseStats.attackDamage);

setMovementTarget(run, 0.5, 0.5);
updateRun(run, 0.25);
assert.ok(run.player.x > 0);
assert.ok(run.player.y > 0);
assert.equal(dash(run), true);
assert.equal(run.trails.length, 1);
assert.ok(run.player.dashCooldown > 0);

run.enemies.push({ id: 'chaser_drone', name: 'Chaser Drone', x: run.player.x, y: run.player.y + 0.05, hp: 1, maxHP: 1, damage: 10, speed: 0, xp: 1, boss: false });
updateRun(run, 0.1);
assert.ok(run.feedbackEvents.some((event) => event.type === 'enemyHit'));
assert.ok(run.feedbackEvents.some((event) => event.type === 'enemyDeath'));
assert.ok(run.feedbackEvents.some((event) => event.type === 'xpCollect') || run.xpShards.length >= 1);
assert.ok(run.effects.some((effect) => effect.type === 'enemyDeath'));
updateRun(run, 0.1);
assert.ok(run.kills >= 1);
assert.ok(run.xpShards.length >= 1 || run.player.xp >= 1);

const draftRun = createRun(save);
draftRun.xpShards.push({ x: draftRun.player.x, y: draftRun.player.y, value: 99 });
updateRun(draftRun, 0.1);
assert.equal(draftRun.status, 'level-up');
assert.equal(draftRun.draftChoices.length, 3);
assert.equal(chooseUpgrade(draftRun, draftRun.draftChoices[0].id), true);
assert.equal(draftRun.status, 'running');

const bossRun = createRun(save);
bossRun.elapsed = 179.9;
updateRun(bossRun, 0.2);
assert.ok(bossRun.enemies.some((enemy) => enemy.boss && enemy.id === 'sky_reaper'));
assert.match(bossRun.message, /SKY REAPER/);
assert.ok(bossRun.feedbackEvents.some((event) => event.type === 'bossSpawn'));
assert.ok(bossRun.effects.some((effect) => effect.type === 'bossSpawn'));
bossRun.bossAttackTimer = 0;
updateRun(bossRun, 0.1);
assert.ok(bossRun.dangerZones.length > 0);
assert.ok(bossRun.enemyProjectiles.length > 0);
const spawnedBoss = bossRun.enemies.find((enemy) => enemy.id === 'sky_reaper');
assert.equal(spawnedBoss.phase, 1);
spawnedBoss.hp = spawnedBoss.maxHP * 0.2;
bossRun.bossAttackTimer = 0;
updateRun(bossRun, 0.1);
assert.equal(spawnedBoss.phase, 3);
assert.equal(spawnedBoss.attackMode, 'reaper-barrage');

const miniBossRun = createRun(save);
miniBossRun.elapsed = 449.9;
updateRun(miniBossRun, 0.2);
assert.ok(miniBossRun.enemies.some((enemy) => enemy.boss && enemy.mini && enemy.id === 'viper_ace'));
assert.match(miniBossRun.message, /VIPER ACE/);

const damageRun = createRun(save);
damageRun.enemies.push({ id: 'chaser_drone', name: 'Chaser Drone', x: damageRun.player.x, y: damageRun.player.y, hp: 20, maxHP: 20, damage: 10, speed: 0, xp: 1, boss: false });
updateRun(damageRun, 0.1);
assert.ok(damageRun.feedbackEvents.some((event) => event.type === 'playerDamage'));
assert.ok(damageRun.screenShake > 0);

bossRun.kills = 10;
bossRun.bossesKilled = 1;
bossRun.miniBossesKilled = 1;
bossRun.defeatedBossRewards = [{ id: 'sky_reaper', coins: 35 }, { id: 'viper_ace', coins: 18, mini: true }];
const rewards = calculateRewards(bossRun);
assert.ok(rewards.coins >= 80);
assert.equal(formatTime(600), '10:00');

console.log('web game core tests passed');
