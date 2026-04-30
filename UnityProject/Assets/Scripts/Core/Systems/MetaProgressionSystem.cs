#nullable disable

using System.Collections.Generic;
using Cosmostar.Core.Models;

namespace Cosmostar.Core.Systems
{
    public sealed class MetaProgressionSystem
    {
        public const int MaxEquippedModules = 3;

        public MetaModifiers BuildModifiers(SaveProfile profile, VerticalSliceCatalog catalog, List<string> equippedModuleIds)
        {
            var modifiers = new MetaModifiers();

            for (var index = 0; index < equippedModuleIds.Count; index++)
            {
                var moduleId = equippedModuleIds[index];
                var module = FindModule(catalog.Modules, moduleId);
                var progress = ProfileQueries.GetModuleProgress(profile, moduleId);

                if (module == null || progress == null || !progress.Unlocked)
                {
                    continue;
                }

                ApplyModule(modifiers, module, progress.Level);
            }

            return modifiers;
        }

        public bool TryUnlockOrUpgradeModule(SaveProfile profile, ModuleDef module)
        {
            var progress = ProfileQueries.GetModuleProgress(profile, module.Id);
            if (progress == null)
            {
                return false;
            }

            if (!progress.Unlocked)
            {
                if (profile.SoftCurrency < module.UnlockCost)
                {
                    return false;
                }

                progress.Unlocked = true;
                progress.Level = 1;
                progress.Equipped = ProfileQueries.GetEquippedModuleCount(profile) < MaxEquippedModules;
                profile.SoftCurrency -= module.UnlockCost;
                return true;
            }

            if (progress.Level >= module.MaxLevel)
            {
                return false;
            }

            var upgradeCost = module.UpgradeCost * progress.Level;
            if (profile.ModuleShards < upgradeCost)
            {
                return false;
            }

            profile.ModuleShards -= upgradeCost;
            progress.Level += 1;
            return true;
        }

        public bool ToggleEquip(SaveProfile profile, string moduleId)
        {
            var progress = ProfileQueries.GetModuleProgress(profile, moduleId);
            if (progress == null || !progress.Unlocked)
            {
                return false;
            }

            if (!progress.Equipped && ProfileQueries.GetEquippedModuleCount(profile) >= MaxEquippedModules)
            {
                return false;
            }

            progress.Equipped = !progress.Equipped;
            return true;
        }

        public List<UnlockTrackEntry> CollectNewUnlocks(SaveProfile profile, VerticalSliceCatalog catalog)
        {
            var unlocked = new List<UnlockTrackEntry>();
            for (var index = 0; index < catalog.UnlockTrack.Count; index++)
            {
                var entry = catalog.UnlockTrack[index];
                if (profile.UnlockTrackXp < entry.RequiredXp || profile.ClaimedUnlockTrackIds.Contains(entry.Id))
                {
                    continue;
                }

                profile.ClaimedUnlockTrackIds.Add(entry.Id);
                profile.ModuleShards += entry.ModuleShards;
                if (!string.IsNullOrEmpty(entry.AbilityUnlockId) && !profile.UnlockedAbilityIds.Contains(entry.AbilityUnlockId))
                {
                    profile.UnlockedAbilityIds.Add(entry.AbilityUnlockId);
                }

                unlocked.Add(entry);
            }

            return unlocked;
        }

        private static ModuleDef FindModule(List<ModuleDef> modules, string moduleId)
        {
            for (var index = 0; index < modules.Count; index++)
            {
                if (modules[index].Id == moduleId)
                {
                    return modules[index];
                }
            }

            return null;
        }

        private static void ApplyModule(MetaModifiers modifiers, ModuleDef module, int level)
        {
            switch (module.EffectType)
            {
                case ModuleEffectType.HullPlating:
                    modifiers.BonusHull += module.Magnitude * level;
                    modifiers.BonusShield += module.Magnitude * 0.75f * level;
                    break;
                case ModuleEffectType.ReactorCore:
                    modifiers.DamageMultiplier += module.Magnitude * level;
                    break;
                case ModuleEffectType.ThrusterMesh:
                    modifiers.MoveSpeedMultiplier += module.Magnitude * level;
                    break;
                case ModuleEffectType.CapacitorLattice:
                    modifiers.FireRateMultiplier += module.Magnitude * level;
                    break;
                case ModuleEffectType.SalvageMagnet:
                    modifiers.PickupRadiusBonus += module.Magnitude * level;
                    break;
                case ModuleEffectType.TacticalReroll:
                    modifiers.StartingRerolls += (int)(module.Magnitude * level);
                    break;
                case ModuleEffectType.BackupSpark:
                    modifiers.ReviveCharges += (int)(module.Magnitude * level);
                    break;
                case ModuleEffectType.CreditCache:
                    modifiers.RewardMultiplier += module.Magnitude * level;
                    break;
            }
        }
    }
}
