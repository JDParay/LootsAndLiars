using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Phase1 : MonoBehaviour
{
    [Header("Tasks")]
    public List<TaskSlot> tasks;

    [Header("Action Box")]
    public GameObject actionBox1;
    public Button actionButton;
    public TMP_Text actionButtonLabel;
    public Button kickButton;

    [Header("Next Button Box")]
    public GameObject nextButtonBox;
    public Button nextButton; 

    [Header("Task Sequence UI")]
    public GameObject taskUIRoot;
    public GameObject doingTaskText;
    public TMP_Text doingTaskLabel;
    public TaskTimerUI timerUI;

    [Header("Log")]
    public LogBox logBox;

    [Header("Cross-Phase References")]
    public Phase2 phase2Manager;
    public KickMngr kickManager;

    // ---- Private / internal state below ----

    string[] flavorLines = {
        "The heavy wooden wagon creaks softly as it rolls forward...",
        "Someone mutters an anxious prayer under their breath.",
        "A distant howl echoes through the fog-laden trees.",
        "Heavy footsteps crunch over damp gravel.",
        "The ember glow of the campfire dies down behind you.",
        "An uncomfortable silence falls over the party.",
        "The iron rims of the wheels scrape against sharp stones.",
        "Eyes watch from the shadowy treeline beyond the road."
    };
    private Coroutine flavorLoopHandle;
    private Coroutine dotsAnimHandle;
    private string baseDoingTaskPhrase = "Ongoing tasks";

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

        nextButton.onClick.AddListener(GoToPhase2);   
        nextButtonBox.SetActive(false);
        nextButton.interactable = false;               

        logBox.Log($"---- Day {GameMngr.Instance.currentDay + 1} ----");

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

    float GetEstimatedTime(TaskSlot task, List<string> selectedNames)
    {
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

        return preview;
    }

    void UpdateTaskTimeLabel(TaskSlot task)
    {
        var selectedNames = task.dropdowns
            .Select(dd => dd.options[dd.value].text)
            .Where(n => n != "--Select--")
            .ToList();

        float estimated = GetEstimatedTime(task, selectedNames);
        task.timeLabel.text = $"{TaskDisplayName(task.type)} ({task.baseSeconds}s -> {estimated:0}s)";
    }

    string TaskDisplayName(TaskType type)
    {
        switch (type)
        {
            case TaskType.FightMonsters: return "Fight Monsters";
            case TaskType.CollectLoot: return "Collect Supplies";
            case TaskType.FixWagon: return "Fix Wagon";
        }
        return type.ToString();
    }

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

        if (filledSelections.Distinct().Count() != filledSelections.Count) return false;
        if (filledSelections.Count != allNames.Count) return false;

        return true;
    }

    void OnSubmitPressed()
    {
        if (!IsValidAssignment())
        {
            logBox.Log("Assign each member to a task slot before proceeding.");
            return;
        }

        StartCoroutine(RunTaskSequence());
    }

    IEnumerator RunTaskSequence()
    {
        actionBox1.SetActive(false);
        taskUIRoot.SetActive(false);
        doingTaskText.SetActive(true);

        nextButtonBox.SetActive(true);
        nextButton.interactable = false;

        logBox.Log("Tasks submitted. Resolving assignments...");

        flavorLoopHandle = StartCoroutine(FlavorLineLoop());
        dotsAnimHandle = StartCoroutine(AnimateDots());  

        yield return StartCoroutine(timerUI.RunTimer(displaySeconds: 20, realSeconds: 10));

        if (flavorLoopHandle != null)
        {
            StopCoroutine(flavorLoopHandle);
            flavorLoopHandle = null;
        }

        if (dotsAnimHandle != null)
        {
            StopCoroutine(dotsAnimHandle);
            dotsAnimHandle = null;
        }

        doingTaskText.SetActive(false);

        ResolveAllTasks();
        nextButton.interactable = true;
    }

    IEnumerator FlavorLineLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1.5f, 3f));
            logBox.Log(flavorLines[Random.Range(0, flavorLines.Length)]);
        }
    }

    IEnumerator AnimateDots()
    {
        string[] dotStates = { ".", "..", "..." };
        int index = 0;

        while (true)
        {
            doingTaskLabel.text = baseDoingTaskPhrase + dotStates[index];
            index = (index + 1) % dotStates.Length;
            yield return new WaitForSeconds(0.5f);
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
            var rawNames = task.dropdowns.Select(dd => dd.options[dd.value].text).ToList();
            var validNames = rawNames.Where(n => n != "--Select--").ToList();

            switch (task.type)
            {
                case TaskType.FightMonsters: ResolveFight(task, validNames); break;
                case TaskType.CollectLoot: ResolveLoot(task, validNames); break;
                case TaskType.FixWagon: ResolveWagon(task, validNames); break;
            }
        }

        ApplyDayTrust();
    }

    string FormatAssignedMembers(List<string> names)
    {
        if (names.Count == 0) return "[ Unassigned ]";
        return $"[ {string.Join(" | ", names)} ]";
    }

    void ResolveFight(TaskSlot task, List<string> names)
{
    float estimated = GetEstimatedTime(task, names);
    float totalStr = names.Sum(n => GetEffectiveStat(n, StatType.Strength));
    float finished = Mathf.Max(0f, task.baseSeconds - totalStr);
    TaskTier tier = GetTier(task.baseSeconds, finished);
    TrackTier(tier);

    int goldChange = 0;
    if (tier == TaskTier.Fail)
    {
        goldChange = -240;
        GameMngr.Instance.AddGold(goldChange);
    }
    else
    {
        goldChange = 895;
        GameMngr.Instance.AddGold(goldChange);
    }

    logBox.Log($"Fight Monsters ({task.baseSeconds}s -> {estimated:0}s)");
    logBox.Log($"└ Time spent: {finished:0}s | Assigned: {FormatAssignedMembers(names)}");
    logBox.Log($"└ Outcome: {GetTierFeedback(tier)} ({(goldChange >= 0 ? "+" : "")}{goldChange} Gold)");
}

    void ResolveLoot(TaskSlot task, List<string> names)
    {
        float estimated = GetEstimatedTime(task, names);
        float totalWis = names.Sum(n => GetEffectiveStat(n, StatType.Wisdom));
        float totalScv = names.Sum(n => GetEffectiveStat(n, StatType.Scavenge));
        float finished = Mathf.Max(0f, task.baseSeconds - totalWis);
        TaskTier tier = GetTier(task.baseSeconds, finished);
        TrackTier(tier);

        int baseGold = (tier == TaskTier.Fail) ? -240 : 895;
        GameMngr.Instance.AddGold(baseGold);

        float chestChance = 0.5f + (totalScv / 100f);
        int bonusGold = 0;
        if (Random.value < chestChance) bonusGold = Random.Range(200, 601);

        if (bonusGold > 0) GameMngr.Instance.AddGold(bonusGold);

        logBox.Log($"Collect Supplies ({task.baseSeconds}s -> {estimated:0}s)");
        logBox.Log($"└ Time spent: {finished:0}s | Assigned: {FormatAssignedMembers(names)}");
        logBox.Log($"└ Outcome: {GetTierFeedback(tier)} ({(baseGold >= 0 ? "+" : "")}{baseGold} Gold)");
        if (bonusGold > 0)
        {
            logBox.Log($"└ Bonus Chest: +{bonusGold} Gold");
        }
    }

    void ResolveWagon(TaskSlot task, List<string> names)
    {
        float estimated = GetEstimatedTime(task, names);
        string primaryWorker = names.Count > 0 ? names[0] : "";
        float haste = string.IsNullOrEmpty(primaryWorker) ? 0 : GetEffectiveStat(primaryWorker, StatType.Haste);
        float finished = Mathf.Max(0f, task.baseSeconds - haste);
        TaskTier tier = GetTier(task.baseSeconds, finished);
        TrackTier(tier);

        if (tier == TaskTier.Fail)
            GameMngr.Instance.AddGold(-240);

        string condition;
        if (haste >= 4) condition = "fixed (100%)";
        else if (haste >= 2) condition = $"partially repaired ({Random.Range(60, 91)}%)";
        else if (haste >= 1) condition = $"slightly damaged ({Random.Range(30, 51)}%)";
        else condition = $"badly damaged ({Random.Range(5, 21)}%)";

        logBox.Log($"Fix Wagon ({task.baseSeconds}s -> {estimated:0}s)");
        logBox.Log($"└ Time spent: {finished:0}s | Assigned: {FormatAssignedMembers(names)}");
        logBox.Log($"└ Outcome: {GetTierFeedback(tier)} | Condition: {condition}");
    }

    // Helper method for feedback strings
    string GetTierFeedback(TaskTier tier)
    {
        switch (tier)
        {
            case TaskTier.Success: return "Task Successful!";
            case TaskTier.Mediocre: return "Task Completed (Barely).";
            case TaskTier.Fail: return "Task Failed!";
        }
        return "";
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
        logBox.Log($"Party Trust Rating: {(trustChange >= 0 ? "+" : "")}{trustChange}");
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
            RefreshRosterAndDropdowns();
        }
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
        logBox.Log($"---- Night {GameMngr.Instance.currentDay + 1} ----");
        DayCycleMngr.Instance.CompletePhase1();

        nextButtonBox.SetActive(false); 
        phase2Manager.BeginMarkingPhase(roster, OnPhase2Complete);
    }

    void OnPhase2Complete()
    {
        GameMngr.Instance.hasCompletedFirstPhase2 = true;
        DayCycleMngr.Instance.CompletePhase2();

        int stolen = GameMngr.Instance.ApplyImposterTheft();
        if (stolen > 0)
            logBox.Log($"Overnight, {stolen} Gold was stolen from the party's supplies.");

        actionBox1.SetActive(true);
        StartNewDay();
    }

    void StartNewDay()
    {
        GameMngr.Instance.currentDay++;
        GameMngr.Instance.hasKickedToday = false;

        logBox.Log($"---- Day {GameMngr.Instance.currentDay + 1} ----");

        RefreshRosterAndDropdowns();
        taskUIRoot.SetActive(true);
        actionBox1.SetActive(true);   

        actionButton.interactable = false;

        kickButton.interactable = GameMngr.Instance.hasCompletedFirstPhase2 && !GameMngr.Instance.hasKickedToday;
    }
}