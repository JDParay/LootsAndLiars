using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Phase1 : MonoBehaviour
{
    public List<TaskSlot> tasks;
    public Button actionButton;
    public TMP_Text actionButtonLabel;
    public LogBox logBox;
    public GameObject taskUIRoot;
    public GameObject doingTaskText;
    public TaskTimerUI timerUI;
    public GameObject actionBox1;
    public Button kickButton;
    public Phase2 phase2Manager;
    public KickMngr kickManager;

    string[] flavorLines = {
        "The wagon creaks forward...",
        "Someone mutters under their breath.",
        "A distant howl echoes through the trees.",
        "Footsteps crunch over gravel.",
        "The fire crackles behind you."
    };
    private Coroutine flavorLoopHandle;

    private List<CharacterData> roster;
    private List<string> allNames;

    public enum TaskTier { Success, Mediocre, Fail }
    private int successCount = 0;
    private int failCount = 0;

    void Start()
    {
        kickButton.onClick.AddListener(OnKickButtonPressed);
        kickButton.interactable = false;

        actionButton.onClick.AddListener(OnSubmitPressed);
        actionButtonLabel.text = "Submit";

        RefreshRosterAndDropdowns();
    }

    void RefreshRosterAndDropdowns()
    {
        roster = GameMngr.Instance.teammates.Where(c => !GameMngr.Instance.IsKicked(c)).ToList();
        allNames = roster.Select(c => c.characterName).ToList();
        allNames.Add("You");

        foreach (var task in tasks)
        {
            foreach (var dd in task.dropdowns)
            {
                dd.ClearOptions();
                dd.AddOptions(new List<string> { "--Select--" }.Concat(allNames).ToList());
                dd.onValueChanged.RemoveAllListeners();

                var capturedDropdown = dd;
                dd.onValueChanged.AddListener(_ => OnAnySelectionChanged(capturedDropdown));
            }
        }

        RefreshAll();
    }

    void OnAnySelectionChanged(TMP_Dropdown changedDropdown)
    {
        string selectedName = changedDropdown.options[changedDropdown.value].text;

        if (selectedName != "--Select--")
        {
            foreach (var task in tasks)
            {
                foreach (var dd in task.dropdowns)
                {
                    if (dd == changedDropdown) continue;

                    string otherName = dd.options[dd.value].text;
                    if (otherName == selectedName)
                    {
                        dd.value = 0;
                    }
                }
            }
        }

        RefreshAll();
    }

    void RefreshAll()
    {
        foreach (var task in tasks)
        {
            UpdateTaskTimeLabel(task);
        }

        actionButton.interactable = IsValidAssignment();
    }

    StatBlock GetStatsFor(string name)
    {
        if (name == "You") return GameMngr.Instance.playerStats;
        var character = roster.FirstOrDefault(c => c.characterName == name);
        return character != null ? character.runtimeStats : null;
    }

    void UpdateTaskTimeLabel(TaskSlot task)
    {
        var selectedNames = task.dropdowns
            .Select(dd => dd.options[dd.value].text)
            .Where(n => n != "--Select--")
            .ToList();

        float preview = task.baseSeconds;

        if (selectedNames.Count > 0)
        {
            switch (task.type)
            {
                case TaskType.FightMonsters:
                    int totalStr = selectedNames.Sum(n => GetStatsFor(n)?.strength ?? 0);
                    preview = Mathf.Max(1f, task.baseSeconds - totalStr);
                    break;
                case TaskType.CollectLoot:
                    int totalWis = selectedNames.Sum(n => GetStatsFor(n)?.wisdom ?? 0);
                    preview = Mathf.Max(1f, task.baseSeconds - totalWis);
                    break;
                case TaskType.FixWagon:
                    int haste = GetStatsFor(selectedNames[0])?.haste ?? 0;
                    preview = Mathf.Max(1f, task.baseSeconds - haste);
                    break;
            }
        }

        task.timeLabel.text = $"{TaskDisplayName(task.type)} ({task.baseSeconds}s) → {preview}s";
    }

    string TaskDisplayName(TaskType type)
    {
        switch (type)
        {
            case TaskType.FightMonsters: return "Fight Monsters";
            case TaskType.CollectLoot: return "Collect Loot/Supplies";
            case TaskType.FixWagon: return "Fix Wagon";
        }
        return type.ToString();
    }

    // ---- Validation: now allows exactly (5 - allNames.Count) slots to stay unfilled ----
    bool IsValidAssignment()
    {
        var filledSelections = new List<string>();

        foreach (var task in tasks)
        {
            var filled = task.dropdowns
                .Select(dd => dd.options[dd.value].text)
                .Where(n => n != "--Select--")
                .ToList();
            filledSelections.AddRange(filled);
        }

        if (filledSelections.Distinct().Count() != filledSelections.Count) return false; // no duplicates
        if (filledSelections.Count != allNames.Count) return false; // everyone still in the party must be placed exactly once

        return true;
    }

    void OnSubmitPressed()
    {
        if (!IsValidAssignment())
        {
            logBox.Log("No more than one name in the tasks.");
            return;
        }

        StartCoroutine(RunTaskSequence());
    }

    IEnumerator RunTaskSequence()
    {
        taskUIRoot.SetActive(false);
        doingTaskText.SetActive(true);

        logBox.Log("Tasks submitted. Resolving...");

        flavorLoopHandle = StartCoroutine(FlavorLineLoop());

        yield return StartCoroutine(timerUI.RunTimer(displaySeconds: 20, realSeconds: 10));

        if (flavorLoopHandle != null)
        {
            StopCoroutine(flavorLoopHandle);
            flavorLoopHandle = null;
        }

        doingTaskText.SetActive(false);

        ResolveAllTasks();
        ShowPhase2Button();
    }

    IEnumerator FlavorLineLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1.5f, 3f));
            logBox.Log(flavorLines[Random.Range(0, flavorLines.Length)]);
        }
    }

    float GetEffectiveStat(string name, StatType type)
    {
        StatBlock stats = GetStatsFor(name);
        if (stats == null) return 0;

        int fullValue = stats.Get(type);
        float nonUseChance = 0.35f;

        if (name != "You")
        {
            var character = roster.FirstOrDefault(c => c.characterName == name);
            if (character != null && GameMngr.Instance.IsImposter(character))
            {
                nonUseChance = 0.60f;
            }
        }

        if (Random.value < nonUseChance)
        {
            return Mathf.FloorToInt(fullValue / 2f);
        }

        return fullValue;
    }

    TaskTier GetTier(float baseTime, float finishedTime)
    {
        float reduction = (baseTime - finishedTime) / baseTime;
        if (reduction >= 0.5f) return TaskTier.Success;
        if (reduction >= 0.25f) return TaskTier.Mediocre;
        return TaskTier.Fail;
    }

    void ResolveAllTasks()
    {
        successCount = 0;
        failCount = 0;

        foreach (var task in tasks)
        {
            var names = task.dropdowns.Select(dd => dd.options[dd.value].text).ToList();

            switch (task.type)
            {
                case TaskType.FightMonsters: ResolveFight(task, names); break;
                case TaskType.CollectLoot: ResolveLoot(task, names); break;
                case TaskType.FixWagon: ResolveWagon(task, names); break;
            }
        }

        ApplyDayTrust();
    }

    void ResolveFight(TaskSlot task, List<string> names)
    {
        float totalStr = names.Sum(n => GetEffectiveStat(n, StatType.Strength));
        float finished = Mathf.Max(0f, task.baseSeconds - totalStr);
        TrackTier(GetTier(task.baseSeconds, finished));
        logBox.Log($"Fighting Monsters ({task.baseSeconds}s): time finished - {finished:0}s");
    }

    void ResolveLoot(TaskSlot task, List<string> names)
    {
        float totalWis = names.Sum(n => GetEffectiveStat(n, StatType.Wisdom));
        float totalScv = names.Sum(n => GetEffectiveStat(n, StatType.Scavenge));
        float finished = Mathf.Max(0f, task.baseSeconds - totalWis);
        TrackTier(GetTier(task.baseSeconds, finished));

        float chestChance = 0.5f + (totalScv / 100f);
        int gold = 0;
        if (Random.value < chestChance) gold = Random.Range(200, 601);

        GameMngr.Instance.AddGold(gold);
        logBox.Log($"Collecting Loot/Supplies ({task.baseSeconds}s): time finished - {finished:0}s, Loot: {gold} worth of GOLD");
    }

    void ResolveWagon(TaskSlot task, List<string> names)
    {
        string name = names[0];
        float haste = GetEffectiveStat(name, StatType.Haste);
        float finished = Mathf.Max(0f, task.baseSeconds - haste);
        TrackTier(GetTier(task.baseSeconds, finished));

        string condition;
        if (haste >= 4) condition = "fixed (100%)";
        else if (haste >= 2) condition = $"slightly fixed ({Random.Range(60,91)}%)";
        else if (haste >= 1) condition = $"slightly damaged ({Random.Range(30,51)}%)";
        else condition = $"badly damaged ({Random.Range(5,21)}%)";

        logBox.Log($"Wagon - {condition}");
    }

    void TrackTier(TaskTier tier)
    {
        if (tier == TaskTier.Success || tier == TaskTier.Mediocre) successCount++;
        else failCount++;
    }

    void ApplyDayTrust()
    {
        int trustChange = successCount - failCount;
        if (successCount == 3) trustChange += 3;
        if (failCount == 3) trustChange -= 1;

        GameMngr.Instance.AdjustTrust(trustChange);
        logBox.Log($"Trust {(trustChange >= 0 ? "+" : "")}{trustChange}");
    }

    // ---- Kick handoff ----
    void OnKickButtonPressed()
    {
        actionBox1.SetActive(false);
        kickManager.BeginKickPhase(roster, OnKickPhaseComplete);
    }

    void OnKickPhaseComplete(bool actuallyKicked)
    {
        actionBox1.SetActive(true);

        if (actuallyKicked)
        {
            GameMngr.Instance.hasKickedToday = true;
            kickButton.interactable = false;
            RefreshRosterAndDropdowns(); // roster shrank, rebuild dropdown options + allNames
        }
        // if backed out (actuallyKicked == false), kickButton stays interactable
    }

    // ---- Phase 2 handoff ----
    void ShowPhase2Button()
    {
        actionButton.onClick.RemoveAllListeners();
        actionButtonLabel.text = "Next";
        actionButton.interactable = true;
        actionButton.onClick.AddListener(GoToPhase2);
    }

    void GoToPhase2()
    {
        actionBox1.SetActive(false);
        phase2Manager.BeginMarkingPhase(roster, OnPhase2Complete);
    }

    void OnPhase2Complete()
    {
        GameMngr.Instance.hasCompletedFirstPhase2 = true;
        actionBox1.SetActive(true);
        StartNewDay();
    }

    void StartNewDay()
    {
        GameMngr.Instance.currentDay++;
        GameMngr.Instance.hasKickedToday = false;

        RefreshRosterAndDropdowns(); // also resets dropdowns to "--Select--" fresh, accounts for any kicks
        taskUIRoot.SetActive(true);

        actionButtonLabel.text = "Submit";
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnSubmitPressed);
        actionButton.interactable = false;

        kickButton.interactable = GameMngr.Instance.hasCompletedFirstPhase2 && !GameMngr.Instance.hasKickedToday;
    }
}