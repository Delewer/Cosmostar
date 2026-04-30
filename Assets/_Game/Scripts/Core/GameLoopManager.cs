using System;
using System.Collections;
using UnityEngine;

public enum RunState
{
    None,
    PreRun,
    Running,
    Paused,
    UpgradeSelection,
    BossWarning,
    RunEnded
}

public class GameLoopManager : MonoBehaviour
{
    public RunState CurrentState { get; private set; } = RunState.None;

    public event Action<RunState> OnStateChanged;
    public event Action OnRunStarted;
    public event Action<bool> OnRunEnded;

    public bool IsGameplayState => CurrentState == RunState.Running || CurrentState == RunState.BossWarning;

    public void StartRun()
    {
        SetState(RunState.Running);
        OnRunStarted?.Invoke();
        ServiceLocator.Instance?.AnalyticsService?.Track("run_start");
    }

    public void PauseRun() => SetState(RunState.Paused);
    public void ResumeRun() => SetState(RunState.Running);
    public void OpenUpgradeSelection() => SetState(RunState.UpgradeSelection);
    public void CloseUpgradeSelection() => SetState(RunState.Running);

    public void TriggerBossWarning(float duration)
    {
        StartCoroutine(BossWarningRoutine(duration));
    }

    public void EndRun(bool victory)
    {
        SetState(RunState.RunEnded);
        OnRunEnded?.Invoke(victory);
        ServiceLocator.Instance?.AnalyticsService?.Track("run_end", new System.Collections.Generic.Dictionary<string, object>
        {
            ["victory"] = victory
        });
    }

    private IEnumerator BossWarningRoutine(float duration)
    {
        SetState(RunState.BossWarning);
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, duration));
        if (CurrentState == RunState.BossWarning)
        {
            SetState(RunState.Running);
        }
    }

    private void SetState(RunState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);

        bool shouldPauseTime = CurrentState == RunState.Paused || CurrentState == RunState.UpgradeSelection;
        Time.timeScale = shouldPauseTime ? 0f : 1f;
    }
}
