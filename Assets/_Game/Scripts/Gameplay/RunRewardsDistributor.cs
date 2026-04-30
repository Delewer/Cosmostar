using UnityEngine;

public class RunRewardsDistributor : MonoBehaviour
{
    [SerializeField] private GameLoopManager gameLoopManager;
    [SerializeField] private RunSessionTracker runSessionTracker;

    [Header("Reward tuning")]
    [SerializeField] private int baseCredits = 25;
    [SerializeField] private int creditsPerKill = 3;
    [SerializeField] private float creditsPerSecond = 0.5f;
    [SerializeField] private int baseCores = 1;

    private void OnEnable()
    {
        if (gameLoopManager != null)
        {
            gameLoopManager.OnRunEnded += HandleRunEnded;
        }
    }

    private void OnDisable()
    {
        if (gameLoopManager != null)
        {
            gameLoopManager.OnRunEnded -= HandleRunEnded;
        }
    }

    private void HandleRunEnded(bool victory)
    {
        if (runSessionTracker == null || ServiceLocator.Instance?.MetaProgressionService == null) return;

        int credits = baseCredits + runSessionTracker.Kills * creditsPerKill + Mathf.RoundToInt(runSessionTracker.ElapsedSeconds * creditsPerSecond);
        int cores = victory ? baseCores + 1 : baseCores;
        int prisms = victory ? 1 : 0;

        ServiceLocator.Instance.MetaProgressionService.AddRunRewards(credits, cores, prisms);
        ServiceLocator.Instance.AnalyticsService?.Track("run_rewards_granted", new System.Collections.Generic.Dictionary<string, object>
        {
            ["credits"] = credits,
            ["cores"] = cores,
            ["prisms"] = prisms,
            ["kills"] = runSessionTracker.Kills,
            ["time_seconds"] = Mathf.RoundToInt(runSessionTracker.ElapsedSeconds)
        });
    }
}
