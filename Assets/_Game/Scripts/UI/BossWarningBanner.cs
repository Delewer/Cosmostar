using UnityEngine;
using UnityEngine.UI;

public class BossWarningBanner : MonoBehaviour
{
    [SerializeField] private GameLoopManager gameLoopManager;
    [SerializeField] private GameObject bannerRoot;
    [SerializeField] private Text bannerText;

    private void Awake()
    {
        if (bannerText != null)
        {
            bannerText.text = "WARNING: BOSS INCOMING";
        }

        if (bannerRoot != null)
        {
            bannerRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (gameLoopManager != null)
        {
            gameLoopManager.OnStateChanged += HandleStateChanged;
        }
    }

    private void OnDisable()
    {
        if (gameLoopManager != null)
        {
            gameLoopManager.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(RunState state)
    {
        if (bannerRoot == null) return;
        bannerRoot.SetActive(state == RunState.BossWarning);
    }
}
