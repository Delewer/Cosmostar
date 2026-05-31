#nullable enable annotations

using System;
using System.Collections.Generic;
using System.Linq;
using NeonSkySurvivors.Core.Models;

namespace NeonSkySurvivors.Core.Systems
{
    public sealed class NeonEquipmentSystem
    {
        public const int RequiredDuplicatesForMerge = 3;
        public const int MvpMaxEquipmentLevel = 20;

        public void EnsureStartingProfile(NeonSaveProfile profile, NeonSkySurvivorsCatalog catalog)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            foreach (var slot in GetMvpSlots())
            {
                var itemId = catalog.StartingEquipmentBySlot[slot];
                if (string.IsNullOrWhiteSpace(itemId))
                {
                    throw new InvalidOperationException("Starting equipment is missing for slot " + slot + ".");
                }

                if (!profile.OwnedEquipmentItems.Any(item => item.ItemID == itemId))
                {
                    profile.OwnedEquipmentItems.Add(new NeonOwnedEquipmentItem
                    {
                        InstanceID = CreateInstanceId(itemId, profile.OwnedEquipmentItems.Count),
                        ItemID = itemId,
                        Rarity = NeonEquipmentRarity.Common,
                        Level = 1
                    });
                }

                EquipSlot(profile, slot, itemId);
            }

            if (!profile.UnlockedWeapons.Contains("basic_blaster"))
            {
                profile.UnlockedWeapons.Add("basic_blaster");
            }
        }

        public NeonPlayerStats CalculateStats(NeonSaveProfile profile, NeonSkySurvivorsCatalog catalog)
        {
            var stats = catalog.BasePlayerStats.Clone();
            foreach (var slot in GetMvpSlots())
            {
                var equippedId = GetEquippedItemId(profile, slot);
                var ownedItem = profile.OwnedEquipmentItems.FirstOrDefault(item => item.ItemID == equippedId);
                if (ownedItem == null)
                {
                    continue;
                }

                var definition = catalog.Equipment.FirstOrDefault(item => item.ItemID == ownedItem.ItemID);
                if (definition == null)
                {
                    continue;
                }

                ApplyEquipmentStats(stats, definition, ownedItem);
            }

            stats.CurrentHP = stats.MaxHP;
            stats.DashCooldown = Math.Max(0.6f, stats.DashCooldown);
            return stats;
        }

        public bool TryEquip(NeonSaveProfile profile, NeonSkySurvivorsCatalog catalog, string instanceId)
        {
            var ownedItem = profile.OwnedEquipmentItems.FirstOrDefault(item => item.InstanceID == instanceId);
            if (ownedItem == null)
            {
                return false;
            }

            var definition = catalog.Equipment.FirstOrDefault(item => item.ItemID == ownedItem.ItemID);
            if (definition == null)
            {
                return false;
            }

            EquipSlot(profile, definition.SlotType, ownedItem.ItemID);
            return true;
        }

        public bool TryUnequip(NeonSaveProfile profile, NeonEquipmentSlot slot)
        {
            switch (slot)
            {
                case NeonEquipmentSlot.Weapon:
                    profile.EquippedWeaponItemID = string.Empty;
                    return true;
                case NeonEquipmentSlot.Wings:
                    profile.EquippedWingsItemID = string.Empty;
                    return true;
                case NeonEquipmentSlot.Engine:
                    profile.EquippedEngineItemID = string.Empty;
                    return true;
                case NeonEquipmentSlot.Hull:
                    profile.EquippedHullItemID = string.Empty;
                    return true;
                case NeonEquipmentSlot.Core:
                    profile.EquippedCoreItemID = string.Empty;
                    return true;
                case NeonEquipmentSlot.Radar:
                    profile.EquippedRadarItemID = string.Empty;
                    return true;
                default:
                    return false;
            }
        }

        public bool TryUpgrade(NeonSaveProfile profile, NeonSkySurvivorsCatalog catalog, string instanceId)
        {
            var ownedItem = profile.OwnedEquipmentItems.FirstOrDefault(item => item.InstanceID == instanceId);
            if (ownedItem == null || ownedItem.Level >= MvpMaxEquipmentLevel)
            {
                return false;
            }

            var definition = catalog.Equipment.FirstOrDefault(item => item.ItemID == ownedItem.ItemID);
            if (definition == null)
            {
                return false;
            }

            var cost = GetUpgradeCost(definition, ownedItem);
            if (profile.PlayerCoins < cost)
            {
                return false;
            }

            profile.PlayerCoins -= cost;
            ownedItem.Level += 1;
            return true;
        }

        public bool TryMergeDuplicates(NeonSaveProfile profile, string itemId, NeonEquipmentRarity rarity, out NeonOwnedEquipmentItem? mergedItem)
        {
            mergedItem = null;
            if (rarity >= NeonEquipmentRarity.Legendary)
            {
                return false;
            }

            var duplicates = profile.OwnedEquipmentItems
                .Where(item => item.ItemID == itemId && item.Rarity == rarity)
                .OrderBy(item => item.Level)
                .ThenBy(item => item.InstanceID, StringComparer.Ordinal)
                .Take(RequiredDuplicatesForMerge)
                .ToList();

            if (duplicates.Count < RequiredDuplicatesForMerge)
            {
                return false;
            }

            foreach (var duplicate in duplicates)
            {
                profile.OwnedEquipmentItems.Remove(duplicate);
            }

            mergedItem = new NeonOwnedEquipmentItem
            {
                InstanceID = CreateInstanceId(itemId + "_merged", profile.OwnedEquipmentItems.Count),
                ItemID = itemId,
                Rarity = (NeonEquipmentRarity)((int)rarity + 1),
                Level = 1
            };
            profile.OwnedEquipmentItems.Add(mergedItem);

            ReplaceMergedEquipmentIfNeeded(profile, itemId, mergedItem);
            return true;
        }

        public static IReadOnlyList<NeonEquipmentSlot> GetMvpSlots()
        {
            return new[]
            {
                NeonEquipmentSlot.Weapon,
                NeonEquipmentSlot.Wings,
                NeonEquipmentSlot.Engine,
                NeonEquipmentSlot.Hull,
                NeonEquipmentSlot.Core,
                NeonEquipmentSlot.Radar
            };
        }

        private static void ApplyEquipmentStats(NeonPlayerStats stats, NeonEquipmentItemDef definition, NeonOwnedEquipmentItem ownedItem)
        {
            var rarityMultiplier = 1f + (int)ownedItem.Rarity * 0.35f;
            var levelMultiplier = 1f + Math.Max(0, ownedItem.Level - 1) * 0.08f;
            var milestoneMultiplier = 1f + (ownedItem.Level / 5) * 0.05f;

            foreach (var modifier in definition.BaseStats)
            {
                var value = modifier.Value;
                if (!modifier.IsPercent)
                {
                    value *= rarityMultiplier * levelMultiplier * milestoneMultiplier;
                }

                ApplyModifier(stats, modifier.StatType, value, modifier.IsPercent);
            }
        }

        private static void ApplyModifier(NeonPlayerStats stats, NeonStatType statType, float value, bool isPercent)
        {
            switch (statType)
            {
                case NeonStatType.AttackDamage:
                    stats.AttackDamage = Apply(stats.AttackDamage, value, isPercent);
                    break;
                case NeonStatType.FireRate:
                    stats.FireRate = Apply(stats.FireRate, value, isPercent);
                    break;
                case NeonStatType.MovementSpeed:
                    stats.MovementSpeed = Apply(stats.MovementSpeed, value, isPercent);
                    break;
                case NeonStatType.MaxHP:
                    stats.MaxHP = Apply(stats.MaxHP, value, isPercent);
                    break;
                case NeonStatType.CurrentHP:
                    stats.CurrentHP = Apply(stats.CurrentHP, value, isPercent);
                    break;
                case NeonStatType.Armor:
                    stats.Armor = Apply(stats.Armor, value, isPercent);
                    break;
                case NeonStatType.CriticalChance:
                    stats.CriticalChance = Apply(stats.CriticalChance, value, isPercent);
                    break;
                case NeonStatType.CriticalDamage:
                    stats.CriticalDamage = Apply(stats.CriticalDamage, value, isPercent);
                    break;
                case NeonStatType.MagnetRange:
                    stats.MagnetRange = Apply(stats.MagnetRange, value, isPercent);
                    break;
                case NeonStatType.StartingEnergy:
                    stats.StartingEnergy = Apply(stats.StartingEnergy, value, isPercent);
                    break;
                case NeonStatType.DashCooldown:
                    stats.DashCooldown = Apply(stats.DashCooldown, value, isPercent);
                    break;
                case NeonStatType.DashDistance:
                    stats.DashDistance = Apply(stats.DashDistance, value, isPercent);
                    break;
                case NeonStatType.SpecialChargeSpeed:
                    stats.SpecialChargeSpeed = Apply(stats.SpecialChargeSpeed, value, isPercent);
                    break;
                case NeonStatType.XPModifier:
                    stats.XPModifier = Apply(stats.XPModifier, value, isPercent);
                    break;
                case NeonStatType.CoinBonus:
                    stats.CoinBonus = Apply(stats.CoinBonus, value, isPercent);
                    break;
            }
        }

        private static float Apply(float current, float value, bool isPercent)
        {
            return isPercent ? current * value : current + value;
        }

        private static int GetUpgradeCost(NeonEquipmentItemDef definition, NeonOwnedEquipmentItem ownedItem)
        {
            return definition.UpgradeCoinCost + ownedItem.Level * 10 + (int)ownedItem.Rarity * 25;
        }

        private static string GetEquippedItemId(NeonSaveProfile profile, NeonEquipmentSlot slot)
        {
            switch (slot)
            {
                case NeonEquipmentSlot.Weapon:
                    return profile.EquippedWeaponItemID;
                case NeonEquipmentSlot.Wings:
                    return profile.EquippedWingsItemID;
                case NeonEquipmentSlot.Engine:
                    return profile.EquippedEngineItemID;
                case NeonEquipmentSlot.Hull:
                    return profile.EquippedHullItemID;
                case NeonEquipmentSlot.Core:
                    return profile.EquippedCoreItemID;
                case NeonEquipmentSlot.Radar:
                    return profile.EquippedRadarItemID;
                default:
                    return string.Empty;
            }
        }

        private static void EquipSlot(NeonSaveProfile profile, NeonEquipmentSlot slot, string itemId)
        {
            switch (slot)
            {
                case NeonEquipmentSlot.Weapon:
                    profile.EquippedWeaponItemID = itemId;
                    break;
                case NeonEquipmentSlot.Wings:
                    profile.EquippedWingsItemID = itemId;
                    break;
                case NeonEquipmentSlot.Engine:
                    profile.EquippedEngineItemID = itemId;
                    break;
                case NeonEquipmentSlot.Hull:
                    profile.EquippedHullItemID = itemId;
                    break;
                case NeonEquipmentSlot.Core:
                    profile.EquippedCoreItemID = itemId;
                    break;
                case NeonEquipmentSlot.Radar:
                    profile.EquippedRadarItemID = itemId;
                    break;
            }
        }

        private static void ReplaceMergedEquipmentIfNeeded(NeonSaveProfile profile, string itemId, NeonOwnedEquipmentItem mergedItem)
        {
            if (profile.EquippedWeaponItemID == itemId)
            {
                profile.EquippedWeaponItemID = mergedItem.ItemID;
            }

            if (profile.EquippedWingsItemID == itemId)
            {
                profile.EquippedWingsItemID = mergedItem.ItemID;
            }

            if (profile.EquippedEngineItemID == itemId)
            {
                profile.EquippedEngineItemID = mergedItem.ItemID;
            }

            if (profile.EquippedHullItemID == itemId)
            {
                profile.EquippedHullItemID = mergedItem.ItemID;
            }

            if (profile.EquippedCoreItemID == itemId)
            {
                profile.EquippedCoreItemID = mergedItem.ItemID;
            }

            if (profile.EquippedRadarItemID == itemId)
            {
                profile.EquippedRadarItemID = mergedItem.ItemID;
            }
        }

        private static string CreateInstanceId(string itemId, int index)
        {
            return itemId + "_" + index.ToString("0000");
        }
    }
}
