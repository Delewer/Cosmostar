using System.Collections.Generic;
using UnityEngine;

namespace NeonSkySurvivors.Runtime.App
{
    /// <summary>
    /// Lightweight procedural audio for the MVP. Generates simple synthesized SFX clips
    /// and a looping synth-drone music bed at runtime (no imported assets), mirroring the
    /// web prototype's authored procedural audio. SFX are played through a small voice
    /// pool so overlapping one-shots do not cut each other off.
    /// </summary>
    public sealed class NeonAudioService
    {
        private const int SampleRate = 44100;
        private const int SfxVoices = 8;

        private readonly List<AudioSource> _sfxSources = new List<AudioSource>();
        private AudioSource _musicSource = null!;
        private int _sfxIndex;
        private bool _enabled = true;
        private bool _musicEnabled = true;
        private bool _sfxEnabled = true;
        private string _musicMode = string.Empty;

        private AudioClip _shoot = null!;
        private AudioClip _enemyDeath = null!;
        private AudioClip _xp = null!;
        private AudioClip _levelUp = null!;
        private AudioClip _dash = null!;
        private AudioClip _warning = null!;
        private AudioClip _bossSpawn = null!;
        private AudioClip _playerDamage = null!;
        private AudioClip _gameOver = null!;
        private AudioClip _victory = null!;
        private AudioClip _special = null!;
        private AudioClip _musicNormal = null!;
        private AudioClip _musicBoss = null!;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (!_enabled)
                {
                    _musicSource.Stop();
                    _musicMode = "off";
                }
            }
        }

        public void Initialize(Transform parent)
        {
            var root = new GameObject("Neon Audio");
            root.transform.SetParent(parent, false);

            var musicObject = new GameObject("Music");
            musicObject.transform.SetParent(root.transform, false);
            _musicSource = musicObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = 0.32f;

            for (var index = 0; index < SfxVoices; index++)
            {
                var voiceObject = new GameObject("Sfx " + index);
                voiceObject.transform.SetParent(root.transform, false);
                var source = voiceObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                _sfxSources.Add(source);
            }

            _shoot = CreateBlip("sfx_shoot", 880f, 1280f, 0.10f, 0.7f, false);
            _enemyDeath = CreateBlip("sfx_death", 520f, 90f, 0.22f, 0.9f, true);
            _xp = CreateBlip("sfx_xp", 1320f, 1760f, 0.07f, 0.55f, false);
            _levelUp = CreateChord("sfx_levelup", new[] { 523f, 659f, 784f }, 0.45f, 0.8f);
            _dash = CreateBlip("sfx_dash", 220f, 920f, 0.16f, 0.8f, false);
            _warning = CreateBlip("sfx_warning", 180f, 150f, 0.55f, 0.85f, true);
            _bossSpawn = CreateBlip("sfx_bossspawn", 140f, 60f, 0.7f, 1f, true);
            _playerDamage = CreateBlip("sfx_damage", 260f, 110f, 0.18f, 0.9f, true);
            _gameOver = CreateBlip("sfx_gameover", 440f, 110f, 0.9f, 0.9f, false);
            _victory = CreateChord("sfx_victory", new[] { 523f, 784f, 1046f }, 0.9f, 0.9f);
            _special = CreateBlip("sfx_special", 300f, 1500f, 0.6f, 0.95f, false);

            _musicNormal = CreateDrone("music_normal", 110f, 0.25f, 1f);
            _musicBoss = CreateDrone("music_boss", 130f, 0.5f, 1.5f);
        }

        public void PlayShoot() => PlayOneShot(_shoot, 0.22f);

        public void PlayEnemyDeath() => PlayOneShot(_enemyDeath, 0.5f);

        public void PlayXp() => PlayOneShot(_xp, 0.3f);

        public void PlayLevelUp() => PlayOneShot(_levelUp, 0.7f);

        public void PlayDash() => PlayOneShot(_dash, 0.6f);

        public void PlayWarning() => PlayOneShot(_warning, 0.75f);

        public void PlayBossSpawn() => PlayOneShot(_bossSpawn, 0.9f);

        public void PlayPlayerDamage() => PlayOneShot(_playerDamage, 0.7f);

        public void PlayGameOver() => PlayOneShot(_gameOver, 0.85f);

        public void PlayVictory() => PlayOneShot(_victory, 0.85f);

        public void PlaySpecial() => PlayOneShot(_special, 0.9f);

        public void SetSfxEnabled(bool enabled)
        {
            _sfxEnabled = enabled;
        }

        public void SetMusicEnabled(bool enabled)
        {
            _musicEnabled = enabled;
            if (!enabled)
            {
                _musicSource.Stop();
                _musicMode = "off";
            }
        }

        public void SetMusic(string mode)
        {
            if (!_enabled || !_musicEnabled || mode == _musicMode)
            {
                return;
            }

            _musicMode = mode;
            if (mode == "off")
            {
                _musicSource.Stop();
                return;
            }

            _musicSource.clip = mode == "boss" ? _musicBoss : _musicNormal;
            _musicSource.volume = mode == "boss" ? 0.4f : 0.32f;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicMode = "off";
            _musicSource.Stop();
        }

        private void PlayOneShot(AudioClip clip, float volume)
        {
            if (!_enabled || !_sfxEnabled || clip == null || _sfxSources.Count == 0)
            {
                return;
            }

            var source = _sfxSources[_sfxIndex];
            _sfxIndex = (_sfxIndex + 1) % _sfxSources.Count;
            source.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        private static AudioClip CreateBlip(string name, float startFreq, float endFreq, float duration, float volume, bool square)
        {
            var samples = Mathf.Max(1, Mathf.CeilToInt(SampleRate * duration));
            var clip = AudioClip.Create(name, samples, 1, SampleRate, false);
            var data = new float[samples];
            double phase = 0d;

            for (var index = 0; index < samples; index++)
            {
                var t = index / (float)samples;
                var freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += 2d * Mathf.PI * freq / SampleRate;
                var raw = Mathf.Sin((float)phase);
                var wave = square ? (raw >= 0f ? 1f : -1f) : raw;
                data[index] = wave * Envelope(t) * volume;
            }

            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateChord(string name, float[] frequencies, float duration, float volume)
        {
            var samples = Mathf.Max(1, Mathf.CeilToInt(SampleRate * duration));
            var clip = AudioClip.Create(name, samples, 1, SampleRate, false);
            var data = new float[samples];
            var normalize = frequencies.Length > 0 ? 1f / frequencies.Length : 1f;

            for (var index = 0; index < samples; index++)
            {
                var t = index / (float)samples;
                var sum = 0f;
                for (var voice = 0; voice < frequencies.Length; voice++)
                {
                    // Stagger note onsets for a quick arpeggio feel.
                    var onset = voice * 0.12f;
                    if (t < onset)
                    {
                        continue;
                    }

                    sum += Mathf.Sin(2f * Mathf.PI * frequencies[voice] * (t * duration));
                }

                data[index] = sum * normalize * Envelope(t) * volume;
            }

            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateDrone(string name, float baseFreq, float lfoHz, float overtoneGain)
        {
            const float duration = 4f;
            var samples = SampleRate * (int)duration;
            var clip = AudioClip.Create(name, samples, 1, SampleRate, false);
            var data = new float[samples];

            for (var index = 0; index < samples; index++)
            {
                var time = index / (float)SampleRate;
                var fundamental = Mathf.Sin(2f * Mathf.PI * baseFreq * time);
                var fifth = Mathf.Sin(2f * Mathf.PI * baseFreq * 1.5f * time) * 0.5f;
                var overtone = Mathf.Sin(2f * Mathf.PI * baseFreq * 2f * time) * 0.25f * overtoneGain;
                var lfo = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * lfoHz * time);
                data[index] = (fundamental + fifth + overtone) * 0.3f * lfo;
            }

            clip.SetData(data, 0);
            return clip;
        }

        private static float Envelope(float t)
        {
            // Fast attack then exponential decay; keeps blips punchy without clicks.
            var attack = Mathf.Clamp01(t / 0.05f);
            var decay = Mathf.Exp(-3.5f * t);
            return attack * decay;
        }
    }
}
