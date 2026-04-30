using System.Collections.Generic;

namespace Cosmostar.Core.Models
{
    public enum RunPhase
    {
        Intro,
        Escalation,
        Elite,
        Boss,
        Results
    }

    public sealed class RunDirectorTick
    {
        public RunPhase Phase;
        public bool WaveChanged;
        public bool DraftPending;
        public float SpawnRatePerSecond;
    }

    public sealed class RunDirector
    {
        private readonly List<WaveDef> _waves;

        public RunDirector(List<WaveDef> waves)
        {
            _waves = waves;
            WaveIndex = 0;
            Phase = waves.Count > 0 ? waves[0].Phase : RunPhase.Intro;
        }

        public float ElapsedSeconds { get; private set; }

        public int WaveIndex { get; private set; }

        public RunPhase Phase { get; private set; }

        public bool DraftPending { get; private set; }

        public WaveDef? CurrentWave
        {
            get
            {
                if (_waves.Count == 0)
                {
                    return null;
                }

                return _waves[WaveIndex];
            }
        }

        public RunDirectorTick Advance(float deltaTime, bool bossDefeated)
        {
            var tick = new RunDirectorTick();
            tick.Phase = Phase;
            tick.SpawnRatePerSecond = CurrentWave == null ? 0f : CurrentWave.SpawnRatePerSecond;

            if (bossDefeated)
            {
                Phase = RunPhase.Results;
                tick.Phase = Phase;
                tick.SpawnRatePerSecond = 0f;
                return tick;
            }

            ElapsedSeconds += deltaTime;
            var waveChanged = false;

            while (WaveIndex < _waves.Count - 1 && ElapsedSeconds >= _waves[WaveIndex].EndSecond)
            {
                if (_waves[WaveIndex].GrantsUpgradeDraft)
                {
                    DraftPending = true;
                }

                WaveIndex++;
                waveChanged = true;
            }

            if (CurrentWave != null)
            {
                Phase = CurrentWave.Phase;
                tick.SpawnRatePerSecond = CurrentWave.SpawnRatePerSecond;
            }

            tick.Phase = Phase;
            tick.WaveChanged = waveChanged;
            tick.DraftPending = DraftPending;
            return tick;
        }

        public void ConsumeDraft()
        {
            DraftPending = false;
        }
    }
}
