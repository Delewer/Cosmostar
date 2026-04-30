using UnityEngine;
using UnityEngine.UI;

public class RunHudController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerDash playerDash;
    [SerializeField] private ExperienceSystem experienceSystem;
    [SerializeField] private RunSessionTracker runSessionTracker;
    [SerializeField] private StageProgressionController stageProgressionController;

    [Header("UI Refs")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private Slider bossHealthSlider;
    [SerializeField] private Text levelText;
    [SerializeField] private Text killsText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text dashCooldownText;
    [SerializeField] private Text bossStateText;

    private BossController trackedBoss;

    private void OnEnable()
    {
        if (experienceSystem != null)
        {
            experienceSystem.OnExperienceChanged += HandleExperienceChanged;
            experienceSystem.OnLevelUp += HandleLevelUp;
        }

        if (runSessionTracker != null)
        {
            runSessionTracker.OnKillCountChanged += HandleKillChanged;
            runSessionTracker.OnTimeChanged += HandleTimeChanged;
        }

        if (playerDash != null)
        {
            playerDash.OnCooldownChanged += HandleDashCooldownChanged;
            HandleDashCooldownChanged(playerDash.CooldownRemaining);
        }

        if (stageProgressionController != null)
        {
            stageProgressionController.OnBossSpawned += HandleBossSpawned;
        }

        SetBossHudVisible(false);
    }

    private void OnDisable()
    {
        if (experienceSystem != null)
        {
            experienceSystem.OnExperienceChanged -= HandleExperienceChanged;
            experienceSystem.OnLevelUp -= HandleLevelUp;
        }

        if (runSessionTracker != null)
        {
            runSessionTracker.OnKillCountChanged -= HandleKillChanged;
            runSessionTracker.OnTimeChanged -= HandleTimeChanged;
        }

        if (playerDash != null)
        {
            playerDash.OnCooldownChanged -= HandleDashCooldownChanged;
        }

        if (stageProgressionController != null)
        {
            stageProgressionController.OnBossSpawned -= HandleBossSpawned;
        }

        UnbindTrackedBoss();
    }

    private void Update()
    {
        if (player != null && healthSlider != null)
        {
            healthSlider.value = player.GetHealthNormalized();
        }

        if (trackedBoss == null && bossHealthSlider != null && bossHealthSlider.gameObject.activeSelf)
        {
            SetBossHudVisible(false);
        }
    }

    private void HandleBossSpawned(BossController boss)
    {
        UnbindTrackedBoss();
        trackedBoss = boss;

        if (trackedBoss == null)
        {
            SetBossHudVisible(false);
            return;
        }

        trackedBoss.OnBossHealthChanged += HandleBossHealthChanged;
        SetBossHudVisible(true);
        HandleBossHealthChanged(trackedBoss.HealthNormalized);
        if (bossStateText != null) bossStateText.text = "Boss Engaged";
    }

    private void UnbindTrackedBoss()
    {
        if (trackedBoss == null) return;
        trackedBoss.OnBossHealthChanged -= HandleBossHealthChanged;
        trackedBoss = null;
    }

    private void HandleExperienceChanged(int current, int required)
    {
        if (experienceSlider == null) return;
        experienceSlider.value = required <= 0 ? 0f : (float)current / required;
    }

    private void HandleLevelUp(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Lv {level}";
        }
    }

    private void HandleKillChanged(int kills)
    {
        if (killsText != null)
        {
            killsText.text = $"Kills: {kills}";
        }
    }

    private void HandleTimeChanged(float seconds)
    {
        if (timerText == null) return;

        int total = Mathf.FloorToInt(seconds);
        int min = total / 60;
        int sec = total % 60;
        timerText.text = $"{min:00}:{sec:00}";
    }

    private void HandleDashCooldownChanged(float seconds)
    {
        if (dashCooldownText == null) return;
        dashCooldownText.text = seconds <= 0f ? "Dash: Ready" : $"Dash: {seconds:0.0}s";
    }

    private void HandleBossHealthChanged(float normalizedHealth)
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.value = normalizedHealth;
        }

        if (bossStateText != null)
        {
            bossStateText.text = normalizedHealth <= 0f ? "Boss Defeated" : "Boss Engaged";
        }
    }

    private void SetBossHudVisible(bool visible)
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.gameObject.SetActive(visible);
        }

        if (bossStateText != null)
        {
            bossStateText.gameObject.SetActive(visible);
        }
    }
}
