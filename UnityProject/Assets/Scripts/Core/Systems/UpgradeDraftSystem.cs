using System;
using System.Collections.Generic;
using Cosmostar.Core.Models;

namespace Cosmostar.Core.Systems
{
    public interface IRandomSource
    {
        float NextFloat();
    }

    public sealed class DefaultRandomSource : IRandomSource
    {
        private readonly Random _random = new Random();

        public float NextFloat()
        {
            return (float)_random.NextDouble();
        }
    }

    public sealed class UpgradeDraftSystem
    {
        public List<UpgradeDef> GenerateChoices(List<UpgradeDef> upgrades, RunBuildState buildState, int count, IRandomSource random)
        {
            return GenerateChoices(upgrades, buildState, null, count, random);
        }

        public List<UpgradeDef> GenerateChoices(List<UpgradeDef> upgrades, RunBuildState buildState, IReadOnlyCollection<string>? unlockedAbilityIds, int count, IRandomSource random)
        {
            var available = new List<UpgradeDef>();
            for (var index = 0; index < upgrades.Count; index++)
            {
                var upgrade = upgrades[index];
                if (GetStacks(buildState, upgrade.Id) < upgrade.MaxStacks && IsUpgradeAvailable(upgrade, buildState, unlockedAbilityIds))
                {
                    available.Add(upgrade);
                }
            }

            var result = new List<UpgradeDef>();
            while (result.Count < count && available.Count > 0)
            {
                var picked = PickWeighted(available, random);
                result.Add(picked);
                available.Remove(picked);
            }

            return result;
        }

        public void ApplyUpgrade(RunBuildState buildState, UpgradeDef upgrade)
        {
            var stack = GetOrCreateStack(buildState, upgrade.Id);
            stack.Stacks += 1;

            var abilityId = ResolveAbilityId(upgrade);
            if (!string.IsNullOrEmpty(abilityId))
            {
                if (!buildState.GrantedAbilityIds.Contains(abilityId))
                {
                    buildState.GrantedAbilityIds.Add(abilityId);
                }
            }

            switch (upgrade.EffectType)
            {
                case UpgradeEffectType.Damage:
                    buildState.DamageMultiplier += upgrade.Magnitude;
                    break;
                case UpgradeEffectType.FireRate:
                    buildState.FireRateMultiplier += upgrade.Magnitude;
                    break;
                case UpgradeEffectType.ProjectileCount:
                    buildState.BonusProjectiles += (int)upgrade.Magnitude;
                    break;
                case UpgradeEffectType.MaxShield:
                    buildState.BonusShield += upgrade.Magnitude;
                    break;
                case UpgradeEffectType.RestoreShield:
                    buildState.ShieldRestore += upgrade.Magnitude;
                    break;
                case UpgradeEffectType.MoveSpeed:
                    buildState.MoveSpeedMultiplier += upgrade.Magnitude;
                    break;
                case UpgradeEffectType.PickupRadius:
                    buildState.PickupRadiusBonus += upgrade.Magnitude;
                    break;
                case UpgradeEffectType.Piercing:
                    buildState.BonusPierce += (int)upgrade.Magnitude;
                    break;
                case UpgradeEffectType.FrostChance:
                    buildState.FrostChance += upgrade.Magnitude;
                    break;
                case UpgradeEffectType.ChainChance:
                    buildState.ChainChance += upgrade.Magnitude;
                    break;
                case UpgradeEffectType.DroneCompanion:
                    buildState.DroneCompanions += (int)upgrade.Magnitude;
                    break;
                case UpgradeEffectType.OverclockBurst:
                    buildState.OverclockBurstDamage += upgrade.Magnitude;
                    break;
            }
        }

        private static UpgradeDef PickWeighted(List<UpgradeDef> upgrades, IRandomSource random)
        {
            var totalWeight = 0f;
            for (var index = 0; index < upgrades.Count; index++)
            {
                totalWeight += upgrades[index].Weight;
            }

            var threshold = random.NextFloat() * totalWeight;
            var rolling = 0f;
            for (var index = 0; index < upgrades.Count; index++)
            {
                rolling += upgrades[index].Weight;
                if (rolling >= threshold)
                {
                    return upgrades[index];
                }
            }

            return upgrades[upgrades.Count - 1];
        }

        private static bool IsUpgradeAvailable(UpgradeDef upgrade, RunBuildState buildState, IReadOnlyCollection<string>? unlockedAbilityIds)
        {
            var abilityId = ResolveAbilityId(upgrade);
            if (string.IsNullOrEmpty(abilityId))
            {
                return true;
            }

            if (buildState.GrantedAbilityIds.Contains(abilityId))
            {
                return true;
            }

            return unlockedAbilityIds != null && unlockedAbilityIds.Contains(abilityId);
        }

        private static int GetStacks(RunBuildState buildState, string upgradeId)
        {
            for (var index = 0; index < buildState.Upgrades.Count; index++)
            {
                if (buildState.Upgrades[index].UpgradeId == upgradeId)
                {
                    return buildState.Upgrades[index].Stacks;
                }
            }

            return 0;
        }

        private static UpgradeStack GetOrCreateStack(RunBuildState buildState, string upgradeId)
        {
            for (var index = 0; index < buildState.Upgrades.Count; index++)
            {
                if (buildState.Upgrades[index].UpgradeId == upgradeId)
                {
                    return buildState.Upgrades[index];
                }
            }

            var stack = new UpgradeStack { UpgradeId = upgradeId };
            buildState.Upgrades.Add(stack);
            return stack;
        }

        private static string ResolveAbilityId(UpgradeDef upgrade)
        {
            if (!string.IsNullOrEmpty(upgrade.AbilityId))
            {
                return upgrade.AbilityId;
            }

            return string.Empty;
        }
    }
}
