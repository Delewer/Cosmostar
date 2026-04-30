using System.Collections.Generic;
using UnityEngine;

public class RunProgressionCoordinator : MonoBehaviour
{
    [SerializeField] private GameLoopManager gameLoopManager;
    [SerializeField] private ExperienceSystem experienceSystem;
    [SerializeField] private UpgradeSystem upgradeSystem;

    private List<UpgradeData> pendingChoices = new();

    private void OnEnable()
    {
        if (experienceSystem != null)
        {
            experienceSystem.OnLevelUp += HandleLevelUp;
        }
    }

    private void OnDisable()
    {
        if (experienceSystem != null)
        {
            experienceSystem.OnLevelUp -= HandleLevelUp;
        }
    }

    private void HandleLevelUp(int level)
    {
        pendingChoices = upgradeSystem != null ? upgradeSystem.RollChoices(3) : new List<UpgradeData>();
        gameLoopManager?.OpenUpgradeSelection();

        ServiceLocator.Instance?.AnalyticsService?.Track("level_up", new Dictionary<string, object>
        {
            ["level"] = level,
            ["choice_count"] = pendingChoices.Count
        });
    }

    public IReadOnlyList<UpgradeData> GetPendingChoices() => pendingChoices;

    public bool SelectUpgrade(UpgradeData selected)
    {
        if (selected == null || !pendingChoices.Contains(selected)) return false;
        bool applied = upgradeSystem != null && upgradeSystem.ApplyChoice(selected);
        if (!applied) return false;

        pendingChoices.Clear();
        gameLoopManager?.CloseUpgradeSelection();
        return true;
    }
}
