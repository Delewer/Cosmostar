using System;
using System.Collections.Generic;
using System.Linq;
using Cosmostar.Core.Models;

namespace Cosmostar.Core.Systems
{
    public sealed class NeonRunTimelineSystem
    {
        public NeonWaveSegmentDef GetActiveWave(NeonSkySurvivorsCatalog catalog, float elapsedSeconds)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            return catalog.Waves
                .OrderBy(wave => wave.StartSecond)
                .LastOrDefault(wave => elapsedSeconds >= wave.StartSecond && elapsedSeconds < wave.EndSecond);
        }

        public IReadOnlyList<NeonBossDef> GetBossesDue(NeonSkySurvivorsCatalog catalog, float previousElapsedSeconds, float currentElapsedSeconds, ISet<string> alreadySpawnedBossIds)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            alreadySpawnedBossIds ??= new HashSet<string>();
            return catalog.Bosses
                .Where(boss => boss.SpawnSecond > previousElapsedSeconds && boss.SpawnSecond <= currentElapsedSeconds)
                .Where(boss => !alreadySpawnedBossIds.Contains(boss.BossID))
                .OrderBy(boss => boss.SpawnSecond)
                .ToList();
        }

        public string GetWarning(NeonSkySurvivorsCatalog catalog, float previousElapsedSeconds, float currentElapsedSeconds)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            var warning = catalog.Waves
                .Where(wave => wave.WarningSecond >= 0f)
                .Where(wave => wave.WarningSecond > previousElapsedSeconds && wave.WarningSecond <= currentElapsedSeconds)
                .OrderBy(wave => wave.WarningSecond)
                .Select(wave => wave.WarningText)
                .FirstOrDefault();

            return warning ?? string.Empty;
        }

        public bool IsFinalBossVictory(NeonSkySurvivorsCatalog catalog, string defeatedBossId)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            var finalBoss = catalog.Bosses.OrderByDescending(boss => boss.SpawnSecond).FirstOrDefault();
            return finalBoss != null && finalBoss.BossID == defeatedBossId;
        }
    }
}
