using System.Collections.Generic;
using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    [SerializeField] private List<UpgradeData> upgradePool = new();
    [SerializeField] private PlayerController player;
    [SerializeField] private SynergyResolver synergyResolver;

    public List<UpgradeData> RollChoices(int count = 3)
    {
        var results = new List<UpgradeData>();
        if (upgradePool.Count == 0 || count <= 0) return results;

        int safeCount = Mathf.Min(count, upgradePool.Count);
        var remaining = new List<UpgradeData>(upgradePool);

        while (results.Count < safeCount && remaining.Count > 0)
        {
            UpgradeData selected = PickWeighted(remaining);
            if (selected == null) break;

            results.Add(selected);
            remaining.Remove(selected);
        }

        return results;
    }

    public bool ApplyChoice(UpgradeData chosen)
    {
        if (chosen == null || player == null || synergyResolver == null)
        {
            Debug.LogWarning("UpgradeSystem.ApplyChoice failed due to missing dependencies or null choice.");
            return false;
        }

        player.ApplyUpgrade(chosen);
        synergyResolver.RegisterUpgrade(chosen);
        ServiceLocator.Instance?.AnalyticsService?.Track("upgrade_pick", new Dictionary<string, object>
        {
            ["upgrade_id"] = chosen.Id,
            ["rarity"] = chosen.Rarity.ToString()
        });

        return true;
    }

    private static UpgradeData PickWeighted(List<UpgradeData> source)
    {
        float totalWeight = 0f;
        for (int i = 0; i < source.Count; i++)
        {
            totalWeight += Mathf.Max(0.01f, source[i].RollWeight);
        }

        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        for (int i = 0; i < source.Count; i++)
        {
            cumulative += Mathf.Max(0.01f, source[i].RollWeight);
            if (roll <= cumulative)
            {
                return source[i];
            }
        }

        return source[^1];
    }
}
