using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    public static ServiceLocator Instance { get; private set; }

    public SaveService SaveService { get; private set; }
    public AnalyticsService AnalyticsService { get; private set; }
    public MetaProgressionService MetaProgressionService { get; private set; }

    public void InitializeServices()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SaveService = new SaveService();
        AnalyticsService = new AnalyticsService();
        MetaProgressionService = new MetaProgressionService(SaveService);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
