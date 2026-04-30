using System;
using System.Collections.Generic;
using UnityEngine;

public class SynergyResolver : MonoBehaviour
{
    [System.Serializable]
    private struct SynergyRule
    {
        public string Key;
        public List<string> RequiredTags;
    }

    [SerializeField] private List<SynergyRule> rules = new();

    private readonly HashSet<string> ownedTags = new();
    private readonly HashSet<string> unlockedSynergies = new();

    public event Action<string> OnSynergyUnlocked;

    private void Reset()
    {
        rules = new List<SynergyRule>
        {
            new SynergyRule
            {
                Key = "solar_flare",
                RequiredTags = new List<string> { "burn", "beam", "crit" }
            }
        };
    }

    public void RegisterUpgrade(UpgradeData data)
    {
        if (data == null || data.Tags == null) return;

        foreach (string tag in data.Tags)
        {
            if (!string.IsNullOrWhiteSpace(tag))
            {
                ownedTags.Add(tag.Trim().ToLowerInvariant());
            }
        }

        EvaluateSynergies();
    }

    private void EvaluateSynergies()
    {
        for (int i = 0; i < rules.Count; i++)
        {
            var rule = rules[i];
            if (string.IsNullOrWhiteSpace(rule.Key) || rule.RequiredTags == null || rule.RequiredTags.Count == 0)
            {
                continue;
            }

            bool allPresent = true;
            for (int j = 0; j < rule.RequiredTags.Count; j++)
            {
                string requiredTag = rule.RequiredTags[j];
                if (string.IsNullOrWhiteSpace(requiredTag) || !ownedTags.Contains(requiredTag.Trim().ToLowerInvariant()))
                {
                    allPresent = false;
                    break;
                }
            }

            if (allPresent)
            {
                UnlockSynergy(rule.Key);
            }
        }
    }

    private void UnlockSynergy(string key)
    {
        if (unlockedSynergies.Contains(key)) return;

        unlockedSynergies.Add(key);
        OnSynergyUnlocked?.Invoke(key);

        ServiceLocator.Instance?.AnalyticsService?.Track("synergy_unlocked", new Dictionary<string, object>
        {
            ["synergy_key"] = key
        });
        Debug.Log($"Synergy unlocked: {key}");
    }
}
