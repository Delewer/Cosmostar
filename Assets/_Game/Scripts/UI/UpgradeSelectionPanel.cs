using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSelectionPanel : MonoBehaviour
{
    [SerializeField] private GameLoopManager gameLoopManager;
    [SerializeField] private RunProgressionCoordinator runProgressionCoordinator;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private Text[] choiceLabels;

    private IReadOnlyList<UpgradeData> choices = new List<UpgradeData>();

    private void Awake()
    {
        BindButtons();
        SetVisible(false);
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
        bool show = state == RunState.UpgradeSelection;
        SetVisible(show);

        if (!show) return;

        choices = runProgressionCoordinator != null
            ? runProgressionCoordinator.GetPendingChoices()
            : new List<UpgradeData>();

        RefreshChoices();
    }

    private void SetVisible(bool show)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(show);
        }
    }

    private void BindButtons()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int idx = i;
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => PickChoice(idx));
        }
    }

    private void RefreshChoices()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            bool valid = choices != null && i < choices.Count && choices[i] != null;
            choiceButtons[i].gameObject.SetActive(valid);

            if (!valid) continue;

            if (i < choiceLabels.Length && choiceLabels[i] != null)
            {
                UpgradeData upgrade = choices[i];
                choiceLabels[i].text = $"{upgrade.DisplayName}\n{upgrade.Description}";
            }
        }
    }

    private void PickChoice(int index)
    {
        if (choices == null || index < 0 || index >= choices.Count) return;
        runProgressionCoordinator?.SelectUpgrade(choices[index]);
    }
}
