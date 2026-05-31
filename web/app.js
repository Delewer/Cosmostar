import { createAudioController } from './audio.js';
import { addRewardEquipment, calculateRewards, catalog, chooseUpgrade, computeStats, createDefaultSave, createRun, dash, equipEquipment, formatTime, getEquipmentDef, getEquipmentUpgradeCost, getOwnedEquipment, mergeEquipment, rollEquipmentRewards, setMovementTarget, summarizeStats, unequipEquipment, updateRun, upgradeEquipment } from './gameCore.js';

const canvas = document.querySelector('#arena');
const ctx = canvas.getContext('2d');
const hud = document.querySelector('#hud');
const panel = document.querySelector('#panel');
const primaryButton = document.querySelector('#primaryButton');
const garageButton = document.querySelector('#garageButton');
const dashButton = document.querySelector('#dashButton');
const pauseButton = document.querySelector('#pauseButton');
const settingsButton = document.querySelector('#settingsButton');

let save = loadSave();
let run = null;
let screen = 'garage';
let paused = false;
let lastFrame = performance.now();
let pointerActive = false;
let selectedInstanceId = '';
const audio = createAudioController(() => save.settings);
let lastRunStatus = 'running';
let lastProjectileCount = 0;
const feedbackAudioGate = {};

function loadSave() {
  const defaults = createDefaultSave();
  const raw = localStorage.getItem('neon-sky-save');
  if (!raw) return defaults;

  try {
    const parsed = JSON.parse(raw);
    return {
      ...defaults,
      ...parsed,
      inventory: Array.isArray(parsed.inventory) ? parsed.inventory : defaults.inventory,
      equipped: { ...defaults.equipped, ...(parsed.equipped ?? {}) },
      settings: { ...defaults.settings, ...(parsed.settings ?? {}) }
    };
  } catch {
    localStorage.removeItem('neon-sky-save');
    return defaults;
  }
}

function persistSave() {
  localStorage.setItem('neon-sky-save', JSON.stringify(save));
}

function startRun() {
  run = createRun(save);
  lastRunStatus = run.status;
  lastProjectileCount = 0;
  audio.play('runStart');
  audio.setMusicMode('run');
  screen = 'run';
  paused = false;
  panel.hidden = true;
  canvas.hidden = false;
}

function returnToGarage() {
  screen = 'garage';
  paused = false;
  run = null;
  panel.hidden = false;
  canvas.hidden = true;
  renderGarage();
}

function finishRun() {
  const rewards = calculateRewards(run);
  save.coins += rewards.coins;
  save.materials += rewards.materials;
  save.completedRuns += 1;
  save.bestSurvivalTime = Math.max(save.bestSurvivalTime, run.elapsed);
  save.bossesDefeated += run.bossesKilled;
  const droppedRarities = rollEquipmentRewards(run);
  const droppedItems = droppedRarities.map((rarity) => addRewardEquipment(save, rarity));
  run.rewardItems = droppedItems;
  persistSave();
  audio.setMusicMode('garage');
  screen = 'results';
  panel.hidden = false;
  canvas.hidden = true;
  renderResults(rewards);
}

function renderGarage() {
  if (!selectedInstanceId || !save.inventory.some((item) => item.instanceId === selectedInstanceId)) {
    selectedInstanceId = save.inventory[0]?.instanceId ?? '';
  }

  const stats = computeStats(save);
  const statPreview = summarizeStats(stats).map((line) => `<span>${line}</span>`).join('');
  const equippedCards = Object.entries(save.equipped).map(([slot, itemId]) => {
    const owned = save.inventory.find((item) => item.itemId === itemId);
    const def = getEquipmentDef(itemId);
    return `<button class="slot" data-select="${owned?.instanceId ?? ''}"><span>${slot}</span><strong>${def?.name ?? 'Empty'}</strong><em>${owned ? `${owned.rarity} Lv.${owned.level}` : 'Unequipped'}</em></button>`;
  }).join('');

  const selected = getOwnedEquipment(save, selectedInstanceId);
  const selectedDef = selected ? getEquipmentDef(selected.itemId) : null;
  const selectedCost = getEquipmentUpgradeCost(selected);
  const duplicateCount = selected ? save.inventory.filter((item) => item.itemId === selected.itemId && item.rarity === selected.rarity).length : 0;
  const inventoryCards = save.inventory.map((owned) => {
    const def = getEquipmentDef(owned.itemId);
    const selectedClass = owned.instanceId === selectedInstanceId ? ' selected' : '';
    return `<button class="inventory-card rarity-${owned.rarity.toLowerCase()}${selectedClass}" data-select="${owned.instanceId}"><strong>${def?.name ?? owned.itemId}</strong><span>${def?.slot ?? 'Part'} • ${owned.rarity}</span><em>Lv.${owned.level}</em></button>`;
  }).join('');

  panel.innerHTML = `
    <h1>Neon Sky Survivors</h1>
    <p class="lede">Build your neon aircraft in the garage, then survive a 10-minute drone storm.</p>
    <div class="meta"><span>Coins: ${save.coins}</span><span>Materials: ${save.materials}</span><span>Best: ${formatTime(save.bestSurvivalTime)}</span></div>
    <h2>Plane Stats</h2>
    <div class="stat-grid">${statPreview}</div>
    <h2>Equipped Parts</h2>
    <div class="slots">${equippedCards}</div>
    <h2>Inventory</h2>
    <div class="inventory-grid">${inventoryCards}</div>
    <div class="detail-card">
      <h3>${selectedDef?.name ?? 'Select equipment'}</h3>
      <p>${selectedDef ? `${selectedDef.slot} • ${selected.rarity} • Lv.${selected.level}` : 'Tap an item card to inspect it.'}</p>
      <p>${selectedDef?.special || 'Basic stat part.'}</p>
      <div class="button-row">
        <button id="equipButton" ${selected ? '' : 'disabled'}>Equip</button>
        <button id="unequipButton" ${selectedDef ? '' : 'disabled'}>Unequip Slot</button>
      </div>
      <div class="button-row">
        <button id="upgradeButton" ${selected && save.coins >= selectedCost ? '' : 'disabled'}>Upgrade ${Number.isFinite(selectedCost) ? `(${selectedCost} coins)` : ''}</button>
        <button id="mergeButton" ${duplicateCount >= 3 ? '' : 'disabled'}>Merge ${duplicateCount}/3</button>
      </div>
    </div>
    <button id="startRunButton" class="primary">Start 10-Minute Mission</button>
  `;

  for (const card of panel.querySelectorAll('[data-select]')) {
    card.addEventListener('click', () => {
      if (!card.dataset.select) return;
      selectedInstanceId = card.dataset.select;
      renderGarage();
    });
  }

  document.querySelector('#equipButton').addEventListener('click', () => {
    if (equipEquipment(save, selectedInstanceId)) {
      persistSave();
      renderGarage();
    }
  });
  document.querySelector('#unequipButton').addEventListener('click', () => {
    const selectedItem = getOwnedEquipment(save, selectedInstanceId);
    const def = selectedItem ? getEquipmentDef(selectedItem.itemId) : null;
    if (def && unequipEquipment(save, def.slot)) {
      persistSave();
      renderGarage();
    }
  });
  document.querySelector('#upgradeButton').addEventListener('click', () => {
    if (upgradeEquipment(save, selectedInstanceId)) {
      persistSave();
      renderGarage();
    }
  });
  document.querySelector('#mergeButton').addEventListener('click', () => {
    const selectedItem = getOwnedEquipment(save, selectedInstanceId);
    if (!selectedItem) return;
    const merged = mergeEquipment(save, selectedItem.itemId, selectedItem.rarity);
    if (merged) {
      selectedInstanceId = merged.instanceId;
      persistSave();
      renderGarage();
    }
  });
  document.querySelector('#startRunButton').addEventListener('click', startRun);
}


function renderSettings() {
  screen = 'settings';
  paused = true;
  panel.hidden = false;
  canvas.hidden = true;
  panel.innerHTML = `
    <h1>Settings</h1>
    <p class="lede">MVP settings are saved locally on this device.</p>
    <div class="settings-list">
      <label class="setting-toggle"><span>Sound Effects</span><input id="soundToggle" type="checkbox" ${save.settings.sound ? 'checked' : ''}></label>
      <label class="setting-toggle"><span>Music Placeholder</span><input id="musicToggle" type="checkbox" ${save.settings.music ? 'checked' : ''}></label>
      <label class="setting-toggle"><span>Screen Shake</span><input id="shakeToggle" type="checkbox" ${save.settings.screenShake ? 'checked' : ''}></label>
      <label class="setting-toggle"><span>Reduced Motion</span><input id="motionToggle" type="checkbox" ${save.settings.reducedMotion ? 'checked' : ''}></label>
    </div>
    <textarea id="saveExport" readonly>${JSON.stringify(save, null, 2)}</textarea>
    <button id="resetSaveButton" class="danger-zone">Reset Local Save</button>
    <button id="settingsBackButton" class="primary">Back to Garage</button>
  `;
  document.querySelector('#soundToggle').addEventListener('change', (event) => updateSetting('sound', event.target.checked));
  document.querySelector('#musicToggle').addEventListener('change', (event) => updateSetting('music', event.target.checked));
  document.querySelector('#shakeToggle').addEventListener('change', (event) => updateSetting('screenShake', event.target.checked));
  document.querySelector('#motionToggle').addEventListener('change', (event) => updateSetting('reducedMotion', event.target.checked));
  document.querySelector('#resetSaveButton').addEventListener('click', () => {
    save = createDefaultSave();
    selectedInstanceId = '';
    persistSave();
    renderSettings();
  });
  document.querySelector('#settingsBackButton').addEventListener('click', returnToGarage);
}

function updateSetting(key, value) {
  save.settings[key] = value;
  persistSave();
  audio.play('setting');
  audio.refreshSettings();
}


function renderLevelUp() {
  panel.hidden = false;
  canvas.hidden = false;
  const cards = run.draftChoices.map((choice) => `
    <button class="upgrade-card" data-upgrade="${choice.id}">
      <strong>${choice.name}</strong>
      <span>${choice.category}</span>
      <p>${choice.description}</p>
    </button>
  `).join('');
  panel.innerHTML = `
    <h1>Level ${run.player.level}</h1>
    <p class="lede">Choose one temporary upgrade. It lasts only for this run.</p>
    <div class="upgrade-grid">${cards}</div>
  `;
  for (const card of panel.querySelectorAll('[data-upgrade]')) {
    card.addEventListener('click', () => {
      chooseUpgrade(run, card.dataset.upgrade);
      panel.hidden = true;
    });
  }
}

function renderResults(rewards) {
  const won = run.status === 'victory';
  panel.innerHTML = `
    <h1>${won ? 'Mission Complete' : 'Game Over'}</h1>
    <p class="lede">${won ? 'Final boss defeated.' : `Survived ${formatTime(run.elapsed)}.`}</p>
    <div class="meta"><span>Kills: ${run.kills}</span><span>Bosses: ${run.bossesKilled}</span><span>Mini: ${run.miniBossesKilled}</span><span>Coins: +${rewards.coins}</span></div>
    <div class="reward-card"><strong>${rewards.item}</strong><span>Materials +${rewards.materials}</span><span>Found: ${(run.rewardItems ?? []).map((item) => `${getEquipmentDef(item.itemId)?.name ?? item.itemId} (${item.rarity})`).join(', ') || 'No equipment drop'}</span></div>
    <button id="retryButton" class="primary">Retry</button>
    <button id="garageReturnButton">Garage</button>
  `;
  document.querySelector('#retryButton').addEventListener('click', startRun);
  document.querySelector('#garageReturnButton').addEventListener('click', returnToGarage);
}

function drawRun() {
  const w = canvas.width;
  const h = canvas.height;
  ctx.clearRect(0, 0, w, h);
  ctx.save();
  const shake = save.settings.screenShake && !save.settings.reducedMotion ? (run.screenShake ?? 0) * 26 : 0;
  if (shake > 0.01) ctx.translate((Math.random() - 0.5) * shake, (Math.random() - 0.5) * shake);
  drawBackground(w, h);
  drawTrails(w, h);
  drawDangerZones(w, h);
  drawXp(w, h);
  drawProjectiles(w, h);
  drawEnemyProjectiles(w, h);
  drawEffects(w, h);
  drawEnemies(w, h);
  drawPlayer(w, h);
  ctx.restore();
}

function drawBackground(w, h) {
  const gradient = ctx.createLinearGradient(0, 0, 0, h);
  gradient.addColorStop(0, '#07091d');
  gradient.addColorStop(1, '#02040c');
  ctx.fillStyle = gradient;
  ctx.fillRect(0, 0, w, h);
  ctx.strokeStyle = 'rgba(71, 250, 255, 0.12)';
  ctx.lineWidth = 1;
  for (let x = 0; x < w; x += 36) {
    ctx.beginPath();
    ctx.moveTo(x, 0);
    ctx.lineTo(x, h);
    ctx.stroke();
  }
  for (let y = 0; y < h; y += 36) {
    ctx.beginPath();
    ctx.moveTo(0, y);
    ctx.lineTo(w, y);
    ctx.stroke();
  }
}

function drawPlayer(w, h) {
  const p = worldToScreen(run.player.x, run.player.y, w, h);
  ctx.save();
  ctx.translate(p.x, p.y);
  ctx.fillStyle = '#4dfdff';
  ctx.shadowColor = '#4dfdff';
  ctx.shadowBlur = 18;
  ctx.beginPath();
  ctx.moveTo(0, -18);
  ctx.lineTo(14, 14);
  ctx.lineTo(0, 7);
  ctx.lineTo(-14, 14);
  ctx.closePath();
  ctx.fill();
  ctx.restore();
}

function drawEnemies(w, h) {
  for (const enemy of run.enemies) {
    const p = worldToScreen(enemy.x, enemy.y, w, h);
    if (enemy.boss) {
      drawBoss(enemy, p);
      continue;
    }

    ctx.fillStyle = enemy.boss ? '#ff42df' : enemy.elite ? '#ffb22e' : '#ff3f5e';
    ctx.shadowColor = ctx.fillStyle;
    ctx.shadowBlur = enemy.boss ? 24 : 10;
    ctx.beginPath();
    ctx.arc(p.x, p.y, enemy.boss ? 24 : 10, 0, Math.PI * 2);
    ctx.fill();
    ctx.shadowBlur = 0;
  }
}

function drawBoss(enemy, p) {
  const palette = bossPalette(enemy.id);
  const phase = enemy.phase ?? 1;
  const pulse = 1 + Math.sin(run.elapsed * 5 + (enemy.visualSeed ?? 0) * 4) * 0.06;
  ctx.save();
  ctx.translate(p.x, p.y);
  ctx.rotate(Math.sin(run.elapsed * 1.4) * 0.08);
  ctx.shadowColor = palette.glow;
  ctx.shadowBlur = 28;

  if ((enemy.pattern ?? enemy.id) === 'reaper') {
    drawSkyReaperBoss(palette, phase, pulse);
  } else if ((enemy.pattern ?? enemy.id) === 'hydra') {
    drawHydraBoss(palette, phase, pulse);
  } else {
    drawEclipseBoss(palette, phase, pulse);
  }

  ctx.restore();
  drawBossHealth(p, enemy, palette);
}

function drawSkyReaperBoss(palette, phase, pulse) {
  ctx.fillStyle = palette.fill;
  ctx.strokeStyle = palette.edge;
  ctx.lineWidth = 3;
  ctx.beginPath();
  ctx.moveTo(0, -30 * pulse);
  ctx.lineTo(24 + phase * 3, 18);
  ctx.lineTo(7, 10);
  ctx.lineTo(0, 28);
  ctx.lineTo(-7, 10);
  ctx.lineTo(-24 - phase * 3, 18);
  ctx.closePath();
  ctx.fill();
  ctx.stroke();
  ctx.strokeStyle = palette.accent;
  ctx.beginPath();
  ctx.arc(-24, 0, 20 + phase * 3, Math.PI * 0.58, Math.PI * 1.38);
  ctx.arc(24, 0, 20 + phase * 3, Math.PI * 1.62, Math.PI * 0.42);
  ctx.stroke();
}

function drawHydraBoss(palette, phase, pulse) {
  ctx.strokeStyle = palette.edge;
  ctx.lineWidth = 5;
  for (let index = 0; index < 3; index += 1) {
    const angle = -Math.PI / 2 + (index - 1) * 0.82 + Math.sin(run.elapsed * 1.7 + index) * 0.08;
    const headX = Math.cos(angle) * (28 + phase * 4);
    const headY = Math.sin(angle) * (25 + phase * 3);
    ctx.beginPath();
    ctx.moveTo(0, 7);
    ctx.lineTo(headX, headY);
    ctx.stroke();
    ctx.fillStyle = palette.fill;
    ctx.beginPath();
    ctx.arc(headX, headY, (10 + phase) * pulse, 0, Math.PI * 2);
    ctx.fill();
  }
  ctx.fillStyle = palette.accent;
  ctx.beginPath();
  ctx.arc(0, 8, 18 * pulse, 0, Math.PI * 2);
  ctx.fill();
}

function drawEclipseBoss(palette, phase, pulse) {
  ctx.strokeStyle = palette.edge;
  ctx.lineWidth = 4;
  for (let ring = 0; ring < 3; ring += 1) {
    ctx.beginPath();
    ctx.arc(0, 0, (14 + ring * 10 + phase * 2) * pulse, 0, Math.PI * 2);
    ctx.stroke();
  }
  ctx.fillStyle = palette.fill;
  ctx.beginPath();
  ctx.arc(0, 0, 16 * pulse, 0, Math.PI * 2);
  ctx.fill();
  ctx.strokeStyle = palette.accent;
  ctx.lineWidth = 3;
  for (let arm = 0; arm < 4 + phase; arm += 1) {
    const angle = run.elapsed * 0.9 + (Math.PI * 2 * arm) / (4 + phase);
    ctx.beginPath();
    ctx.moveTo(Math.cos(angle) * 20, Math.sin(angle) * 20);
    ctx.lineTo(Math.cos(angle) * (42 + phase * 5), Math.sin(angle) * (42 + phase * 5));
    ctx.stroke();
  }
}

function drawBossHealth(p, enemy, palette) {
  const ratio = Math.max(0, enemy.hp / enemy.maxHP);
  ctx.save();
  ctx.fillStyle = 'rgba(2, 4, 12, .74)';
  ctx.fillRect(p.x - 46, p.y - 48, 92, 7);
  ctx.fillStyle = palette.edge;
  ctx.fillRect(p.x - 46, p.y - 48, 92 * ratio, 7);
  ctx.restore();
}

function drawDangerZones(w, h) {
  for (const zone of run.dangerZones) {
    const p = worldToScreen(zone.x, zone.y, w, h);
    const palette = bossPalette(zone.source);
    ctx.strokeStyle = zone.windup > 0 ? palette.warning : 'rgba(255, 72, 72, .9)';
    ctx.fillStyle = zone.windup > 0 ? palette.warningFill : 'rgba(255, 72, 72, .18)';
    ctx.lineWidth = 3;
    ctx.beginPath();
    ctx.arc(p.x, p.y, zone.radius * w * 0.5, 0, Math.PI * 2);
    ctx.fill();
    ctx.stroke();
  }
}

function drawEnemyProjectiles(w, h) {
  for (const projectile of run.enemyProjectiles) {
    const palette = bossPalette(projectile.source);
    ctx.fillStyle = palette.projectile;
    ctx.shadowColor = palette.glow;
    ctx.shadowBlur = 12;
    const p = worldToScreen(projectile.x, projectile.y, w, h);
    ctx.beginPath();
    ctx.arc(p.x, p.y, 6, 0, Math.PI * 2);
    ctx.fill();
  }
  ctx.shadowBlur = 0;
}

function drawProjectiles(w, h) {
  ctx.fillStyle = '#63ff84';
  ctx.shadowColor = '#63ff84';
  ctx.shadowBlur = 12;
  for (const projectile of run.projectiles) {
    const p = worldToScreen(projectile.x, projectile.y, w, h);
    ctx.beginPath();
    ctx.arc(p.x, p.y, 5, 0, Math.PI * 2);
    ctx.fill();
  }
  ctx.shadowBlur = 0;
}

function drawXp(w, h) {
  ctx.fillStyle = '#42ffc8';
  for (const shard of run.xpShards) {
    const p = worldToScreen(shard.x, shard.y, w, h);
    ctx.fillRect(p.x - 3, p.y - 3, 6, 6);
  }
}

function drawTrails(w, h) {
  for (const trail of run.trails) {
    const a = worldToScreen(trail.start.x, trail.start.y, w, h);
    const b = worldToScreen(trail.end.x, trail.end.y, w, h);
    ctx.strokeStyle = 'rgba(105, 244, 255, .75)';
    ctx.lineWidth = 10;
    ctx.shadowColor = '#b86cff';
    ctx.shadowBlur = 16;
    ctx.beginPath();
    ctx.moveTo(a.x, a.y);
    ctx.lineTo(b.x, b.y);
    ctx.stroke();
    ctx.shadowBlur = 0;
  }
}

function drawEffects(w, h) {
  if (!run.effects?.length) return;
  for (const effect of run.effects) {
    const alpha = Math.max(0, effect.life / effect.maxLife);
    const origin = worldToScreen(effect.x, effect.y, w, h);
    ctx.save();
    ctx.globalAlpha = alpha;
    ctx.strokeStyle = effect.color;
    ctx.lineWidth = effect.type === 'bossSpawn' || effect.type === 'bossDeath' ? 4 : 2;
    ctx.shadowColor = effect.color;
    ctx.shadowBlur = effect.type.startsWith('boss') ? 26 : 14;
    ctx.beginPath();
    ctx.arc(origin.x, origin.y, effect.radius * w * 0.5 * (1.35 - alpha * 0.35), 0, Math.PI * 2);
    ctx.stroke();

    if (!save.settings.reducedMotion) {
      ctx.fillStyle = effect.color;
      for (const particle of effect.particles) {
        const p = worldToScreen(particle.x, particle.y, w, h);
        const size = Math.max(2, particle.size * w);
        ctx.beginPath();
        ctx.arc(p.x, p.y, size, 0, Math.PI * 2);
        ctx.fill();
      }
    }
    ctx.restore();
  }
}

function updateHud() {
  if (!run) {
    hud.textContent = '';
    return;
  }
  const hpRatio = Math.max(0, run.player.stats.currentHP / run.player.stats.maxHP);
  const xpRatio = Math.max(0, run.player.xp / run.player.xpToNext);
  hud.innerHTML = `
    <span>${formatTime(run.elapsed)}</span>
    <span>HP <b style="--w:${hpRatio * 100}%"></b></span>
    <span>XP <b style="--w:${xpRatio * 100}%"></b></span>
    <span>Lv.${run.player.level}</span>
    <span>Coins ${run.player.coins}</span>
    <span>Dash ${run.player.dashCooldown <= 0 ? 'Ready' : run.player.dashCooldown.toFixed(1)}</span>
    <span>${run.message}</span>
  `;
}

function loop(now) {
  const delta = Math.min(0.05, (now - lastFrame) / 1000);
  lastFrame = now;
  if (screen === 'run' && run && !paused) {
    const previousProjectileCount = run.projectiles.length;
    updateRun(run, delta);
    playFeedbackEvents(run.feedbackEvents);
    const boss = run.enemies.find((enemy) => enemy.boss);
    audio.setMusicMode(boss?.id === 'eclipse_core' ? 'final' : boss ? 'boss' : 'run');
    if (run.projectiles.length > previousProjectileCount && run.projectiles.length !== lastProjectileCount) {
      audio.play('shoot');
    }
    lastProjectileCount = run.projectiles.length;
    if (run.status !== lastRunStatus) {
      audio.play(run.status === 'level-up' ? 'levelUp' : run.status === 'victory' ? 'victory' : 'gameOver');
      lastRunStatus = run.status;
    }
    drawRun();
    updateHud();
    if (run.status === 'level-up') renderLevelUp();
    if (run.status === 'game-over' || run.status === 'victory') finishRun();
  }
  requestAnimationFrame(loop);
}

function playFeedbackEvents(events = []) {
  for (const event of events) {
    if (event.type === 'enemyHit') playGated(event.boss ? 'bossHit' : 'enemyHit', event.boss ? 70 : 45);
    if (event.type === 'enemyDeath') playGated(event.boss ? 'bossDeath' : 'enemyDeath', event.boss ? 180 : 70);
    if (event.type === 'xpCollect') playGated('xpCollect', 55);
    if (event.type === 'playerDamage') playGated('playerDamage', 120);
    if (event.type === 'bossSpawn') playGated('bossSpawn', 300);
  }
}

function playGated(name, gapMs) {
  const now = performance.now();
  if ((feedbackAudioGate[name] ?? 0) + gapMs > now) return;
  feedbackAudioGate[name] = now;
  audio.play(name);
}

function worldToScreen(x, y, w, h) {
  return { x: (x + 1) * 0.5 * w, y: (y + 1) * 0.5 * h };
}

function bossPalette(source = '') {
  if (source === 'neon_hydra' || source === 'bombardier_prime' || source === 'hydra') {
    return { fill: '#23ff9b', edge: '#42ffc8', accent: '#fff06a', glow: '#23ff9b', projectile: '#42ffc8', warning: 'rgba(66, 255, 200, .72)', warningFill: 'rgba(66, 255, 200, .08)' };
  }
  if (source === 'eclipse_core' || source === 'eclipse') {
    return { fill: '#ffef6a', edge: '#ff8a3d', accent: '#ffffff', glow: '#ffef6a', projectile: '#ffef6a', warning: 'rgba(255, 239, 106, .72)', warningFill: 'rgba(255, 239, 106, .1)' };
  }
  return { fill: '#ff42df', edge: '#ff6ce8', accent: '#4dfdff', glow: '#ff42df', projectile: '#ff8a3d', warning: 'rgba(255, 108, 232, .72)', warningFill: 'rgba(255, 108, 232, .08)' };
}

function screenToWorld(clientX, clientY) {
  const rect = canvas.getBoundingClientRect();
  const x = ((clientX - rect.left) / rect.width) * 2 - 1;
  const y = ((clientY - rect.top) / rect.height) * 2 - 1;
  return { x, y };
}

canvas.addEventListener('pointerdown', (event) => {
  pointerActive = true;
  const point = screenToWorld(event.clientX, event.clientY);
  setMovementTarget(run, point.x, point.y);
});

canvas.addEventListener('pointermove', (event) => {
  if (!pointerActive || !run) return;
  const point = screenToWorld(event.clientX, event.clientY);
  setMovementTarget(run, point.x, point.y);
});

window.addEventListener('pointerup', () => {
  pointerActive = false;
});

dashButton.addEventListener('click', () => {
  if (run && dash(run)) audio.play('dash');
});

pauseButton.addEventListener('click', () => {
  if (screen === 'run') paused = !paused;
});

primaryButton.addEventListener('click', startRun);
garageButton.addEventListener('click', returnToGarage);
settingsButton.addEventListener('click', renderSettings);

renderGarage();
requestAnimationFrame(loop);
