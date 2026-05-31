const SOUND_PRESETS = {
  runStart: [{ frequency: 392, duration: 0.06 }, { frequency: 523, duration: 0.08, delay: 0.05 }],
  shoot: [{ frequency: 390, duration: 0.025, type: 'square', gain: 0.012 }],
  enemyHit: [{ frequency: 520, duration: 0.018, type: 'square', gain: 0.01 }],
  bossHit: [{ frequency: 240, duration: 0.026, type: 'sawtooth', gain: 0.018 }],
  enemyDeath: [{ frequency: 180, duration: 0.05, type: 'triangle', gain: 0.022 }, { frequency: 340, duration: 0.035, delay: 0.018, gain: 0.012 }],
  bossDeath: [{ frequency: 110, duration: 0.12, type: 'triangle', gain: 0.03 }, { frequency: 440, duration: 0.16, delay: 0.04, gain: 0.018 }],
  xpCollect: [{ frequency: 920, duration: 0.025, type: 'sine', gain: 0.012 }, { frequency: 1240, duration: 0.028, delay: 0.02, type: 'sine', gain: 0.008 }],
  playerDamage: [{ frequency: 120, duration: 0.08, type: 'sawtooth', gain: 0.03 }, { frequency: 80, duration: 0.08, delay: 0.035, type: 'triangle', gain: 0.018 }],
  bossSpawn: [{ frequency: 98, duration: 0.16, type: 'sawtooth', gain: 0.024 }, { frequency: 196, duration: 0.12, delay: 0.08, gain: 0.016 }],
  dash: [{ frequency: 740, duration: 0.05, type: 'sawtooth', gain: 0.028 }, { frequency: 980, duration: 0.035, delay: 0.035, gain: 0.018 }],
  levelUp: [{ frequency: 660, duration: 0.06 }, { frequency: 880, duration: 0.08, delay: 0.055 }],
  victory: [{ frequency: 784, duration: 0.08 }, { frequency: 1046, duration: 0.16, delay: 0.08 }],
  gameOver: [{ frequency: 260, duration: 0.12, type: 'triangle' }, { frequency: 180, duration: 0.18, delay: 0.1, type: 'triangle' }],
  setting: [{ frequency: 660, duration: 0.04 }]
};

export function createAudioController(getSettings) {
  let context = null;
  let musicOscillator = null;
  let musicGain = null;
  let musicMode = 'garage';

  function ensureContext() {
    context ??= new AudioContext();
    return context;
  }

  function play(name) {
    const settings = getSettings();
    if (!settings.sound) return;
    const preset = SOUND_PRESETS[name] ?? SOUND_PRESETS.setting;
    const audioContext = ensureContext();
    for (const note of preset) {
      const oscillator = audioContext.createOscillator();
      const gain = audioContext.createGain();
      oscillator.type = note.type ?? 'sawtooth';
      oscillator.frequency.value = note.frequency;
      gain.gain.value = note.gain ?? 0.025;
      oscillator.connect(gain);
      gain.connect(audioContext.destination);
      const startAt = audioContext.currentTime + (note.delay ?? 0);
      oscillator.start(startAt);
      oscillator.stop(startAt + note.duration);
    }
  }

  function setMusicMode(nextMode) {
    musicMode = nextMode;
    const settings = getSettings();
    if (!settings.music) {
      stopMusic();
      return;
    }

    const audioContext = ensureContext();
    if (!musicOscillator) {
      musicOscillator = audioContext.createOscillator();
      musicGain = audioContext.createGain();
      musicOscillator.type = 'triangle';
      musicGain.gain.value = 0.008;
      musicOscillator.connect(musicGain);
      musicGain.connect(audioContext.destination);
      musicOscillator.start();
    }

    musicOscillator.frequency.setTargetAtTime(resolveMusicFrequency(musicMode), audioContext.currentTime, 0.25);
  }

  function stopMusic() {
    if (!musicOscillator) return;
    musicOscillator.stop();
    musicOscillator.disconnect();
    musicGain?.disconnect();
    musicOscillator = null;
    musicGain = null;
  }

  function refreshSettings() {
    if (getSettings().music) {
      setMusicMode(musicMode);
    } else {
      stopMusic();
    }
  }

  return { play, setMusicMode, stopMusic, refreshSettings };
}

function resolveMusicFrequency(mode) {
  if (mode === 'boss') return 98;
  if (mode === 'final') return 123;
  if (mode === 'run') return 82;
  return 65;
}
