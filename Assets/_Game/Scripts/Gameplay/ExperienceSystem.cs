using System;
using UnityEngine;

public class ExperienceSystem : MonoBehaviour
{
    [SerializeField] private int baseExperienceToLevel = 10;
    [SerializeField] private float experienceGrowth = 1.25f;

    public int CurrentLevel { get; private set; } = 1;
    public int CurrentExperience { get; private set; }
    public int RequiredExperience { get; private set; }

    public event Action<int> OnLevelUp;
    public event Action<int, int> OnExperienceChanged;

    private void Awake()
    {
        RequiredExperience = Mathf.Max(1, baseExperienceToLevel);
        NotifyExperienceChanged();
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;

        CurrentExperience += amount;
        while (CurrentExperience >= RequiredExperience)
        {
            CurrentExperience -= RequiredExperience;
            LevelUp();
        }

        NotifyExperienceChanged();
    }

    private void LevelUp()
    {
        CurrentLevel++;
        RequiredExperience = Mathf.Max(1, Mathf.RoundToInt(baseExperienceToLevel * Mathf.Pow(experienceGrowth, CurrentLevel - 1)));
        OnLevelUp?.Invoke(CurrentLevel);
    }

    private void NotifyExperienceChanged()
    {
        OnExperienceChanged?.Invoke(CurrentExperience, RequiredExperience);
    }
}
