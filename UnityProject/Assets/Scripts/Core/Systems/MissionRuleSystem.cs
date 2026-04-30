using System.Collections.Generic;
using Cosmostar.Core.Models;

namespace Cosmostar.Core.Systems
{
    public sealed class MissionRuleSystem
    {
        public MissionRuleSet Resolve(MissionDef mission)
        {
            var rules = new MissionRuleSet();

            switch (mission.Id)
            {
                case "survive_four":
                    rules.GlobalSpawnRateMultiplier = 1.08f;
                    break;
                case "kill_eighty":
                    rules.GlobalSpawnRateMultiplier = 1.06f;
                    break;
                case "boss_clear":
                    rules.BossStartTimeOverride = 185f;
                    break;
                case "shield_clear":
                    rules.StartingShieldMultiplier = 0.78f;
                    rules.BossStartTimeOverride = 195f;
                    break;
                case "kill_one_twenty":
                    rules.GlobalSpawnRateMultiplier = 1.22f;
                    break;
                case "survive_hard":
                    rules.GlobalSpawnRateMultiplier = 1.12f;
                    rules.AddRammerToIntroWave = true;
                    break;
            }

            return rules;
        }

        public List<WaveDef> CreateModifiedWaves(List<WaveDef> baseWaves, MissionRuleSet rules)
        {
            var waves = new List<WaveDef>(baseWaves.Count);
            for (var index = 0; index < baseWaves.Count; index++)
            {
                var source = baseWaves[index];
                var clone = new WaveDef
                {
                    Id = source.Id,
                    DisplayName = source.DisplayName,
                    Phase = source.Phase,
                    StartSecond = source.StartSecond,
                    EndSecond = source.EndSecond,
                    SpawnRatePerSecond = source.SpawnRatePerSecond * rules.GlobalSpawnRateMultiplier,
                    GrantsUpgradeDraft = source.GrantsUpgradeDraft,
                    SpawnArchetypes = new List<EnemyArchetype>(source.SpawnArchetypes)
                };

                waves.Add(clone);
            }

            if (rules.AddRammerToIntroWave && waves.Count > 0 && !waves[0].SpawnArchetypes.Contains(EnemyArchetype.Rammer))
            {
                waves[0].SpawnArchetypes.Add(EnemyArchetype.Rammer);
            }

            if (rules.BossStartTimeOverride > 0f)
            {
                for (var index = 0; index < waves.Count; index++)
                {
                    if (waves[index].Phase == RunPhase.Boss)
                    {
                        var previousEnd = index > 0 ? waves[index - 1].StartSecond : 0f;
                        waves[index].StartSecond = rules.BossStartTimeOverride;
                        if (waves[index].EndSecond < waves[index].StartSecond + 80f)
                        {
                            waves[index].EndSecond = waves[index].StartSecond + 80f;
                        }

                        if (index > 0)
                        {
                            waves[index - 1].EndSecond = rules.BossStartTimeOverride;
                            if (waves[index - 1].StartSecond > waves[index - 1].EndSecond)
                            {
                                waves[index - 1].StartSecond = previousEnd;
                            }
                        }

                        break;
                    }
                }
            }

            return waves;
        }
    }
}
