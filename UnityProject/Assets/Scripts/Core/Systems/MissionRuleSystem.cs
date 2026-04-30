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
                    ConfigureAnomaly(rules, RunAnomalyKind.MeteorShower, "Meteor showers", 24f, 28f, 0.85f, 16f, 3);
                    break;
                case "kill_eighty":
                    rules.GlobalSpawnRateMultiplier = 1.06f;
                    ConfigureAnomaly(rules, RunAnomalyKind.SalvageBloom, "Salvage blooms", 20f, 30f, 0f, 0f, 6);
                    break;
                case "boss_clear":
                    rules.BossStartTimeOverride = 185f;
                    ConfigureAnomaly(rules, RunAnomalyKind.SolarFlare, "Solar flare lanes", 38f, 34f, 0.95f, 13f, 2);
                    break;
                case "shield_clear":
                    rules.StartingShieldMultiplier = 0.78f;
                    rules.BossStartTimeOverride = 195f;
                    ConfigureAnomaly(rules, RunAnomalyKind.SolarFlare, "Shield-draining flares", 32f, 32f, 1.05f, 12f, 2);
                    break;
                case "kill_one_twenty":
                    rules.GlobalSpawnRateMultiplier = 1.22f;
                    ConfigureAnomaly(rules, RunAnomalyKind.SalvageBloom, "Volatile salvage blooms", 18f, 24f, 0f, 0f, 5);
                    break;
                case "survive_hard":
                    rules.GlobalSpawnRateMultiplier = 1.12f;
                    rules.AddRammerToIntroWave = true;
                    ConfigureAnomaly(rules, RunAnomalyKind.MeteorShower, "Dense meteor showers", 18f, 24f, 0.8f, 18f, 4);
                    break;
            }

            return rules;
        }

        private static void ConfigureAnomaly(MissionRuleSet rules, RunAnomalyKind kind, string label, float firstSecond, float intervalSeconds, float telegraphSeconds, float damage, int count)
        {
            rules.AnomalyKind = kind;
            rules.AnomalyLabel = label;
            rules.AnomalyFirstSecond = firstSecond;
            rules.AnomalyIntervalSeconds = intervalSeconds;
            rules.AnomalyTelegraphSeconds = telegraphSeconds;
            rules.AnomalyDamage = damage;
            rules.AnomalyCount = count;
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
