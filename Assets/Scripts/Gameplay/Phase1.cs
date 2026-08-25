using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Phase1 : MonoBehaviour
{
    public List<TaskSlot> tasks; // 3 entries: Fight(2), Loot(2), Wagon(1)
    public Button submitButton;
    public LogBox logBox;
    public GameObject taskUIRoot;    
    public GameObject doingTaskText;   
    public TaskTimerUI timerUI;        

    string[] flavorLines = {
        "The wagon creaks forward...",
        "Someone mutters under their breath.",
        "A distant howl echoes through the trees.",
        "Footsteps crunch over gravel.",
        "The fire crackles behind you."
    };

    private List<CharacterData> roster;
    private List<string> allNames;

    public enum TaskTier { Success, Mediocre, Fail }
    private int successCount = 0;
    private int failCount = 0;

    void Start()
    {
        roster = GameMngr.Instance.teammates;

        allNames = roster.Select(c => c.characterName).ToList();
        allNames.Add("You");

        foreach (var task in tasks)
        {
            foreach (var dd in task.dropdowns)
            {
                dd.ClearOptions();
                dd.AddOptions(new List<string> { "--Select--" }.Concat(allNames).ToList());

                var capturedDropdown = dd; // capture for closure
                dd.onValueChanged.AddListener(_ => OnAnySelectionChanged(capturedDropdown));
            }
        }

        submitButton.onClick.AddListener(OnSubmitPressed);
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

        submitButton.interactable = IsValidAssignment();
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

    // ---- Validation ----
    bool IsValidAssignment()
    {
        var allSelected = new List<string>();

        foreach (var task in tasks)
        {
            var names = task.dropdowns.Select(dd => dd.options[dd.value].text).ToList();

            if (names.Any(n => n == "--Select--")) return false; // must be fully filled
            allSelected.AddRange(names);
        }

        // no repeats anywhere, and every name in the roster+player must appear exactly once
        if (allSelected.Distinct().Count() != allSelected.Count) return false;
        if (allSelected.Count != allNames.Count) return false;

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

        // Sprinkle flavor lines while the timer runs
        StartCoroutine(FlavorLineLoop());

        yield return StartCoroutine(timerUI.RunTimer(displaySeconds: 20, realSeconds: 10));

        StopCoroutine(FlavorLineLoop()); // stop spawning new flavor once timer ends
        doingTaskText.SetActive(false);

        ResolveAllTasks(); // next chunk — the actual tier/log math
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

        if (reduction >= 0.5f) return TaskTier.Success;   // finished at or under half the base time
        if (reduction >= 0.25f) return TaskTier.Mediocre; // finished at or under 75% of base time
        return TaskTier.Fail;                              // anything worse, including 0% or negative
    }

    // === Resolving Tasks ===

    void ResolveAllTasks()
    {
        successCount = 0;
        failCount = 0;

        foreach (var task in tasks)
        {
            var names = task.dropdowns
                .Select(dd => dd.options[dd.value].text)
                .ToList();

            switch (task.type)
            {
                case TaskType.FightMonsters:
                    ResolveFight(task, names);
                    break;
                case TaskType.CollectLoot:
                    ResolveLoot(task, names);
                    break;
                case TaskType.FixWagon:
                    ResolveWagon(task, names);
                    break;
            }
        }

        ApplyDayTrust();

        // TODO: gold total → GameManager
    }

    void ResolveFight(TaskSlot task, List<string> names)
    {
        float totalStr = names.Sum(n => GetEffectiveStat(n, StatType.Strength));
        float finished = Mathf.Max(0f, task.baseSeconds - totalStr);
        var tier = GetTier(task.baseSeconds, finished);

        TrackTier(tier);

        logBox.Log($"Fighting Monsters ({task.baseSeconds}s): time finished - {finished:0}s");
    }

    void ResolveLoot(TaskSlot task, List<string> names)
    {
        float totalWis = names.Sum(n => GetEffectiveStat(n, StatType.Wisdom));
        float totalScv = names.Sum(n => GetEffectiveStat(n, StatType.Scavenge));
        float finished = Mathf.Max(0f, task.baseSeconds - totalWis);
        var tier = GetTier(task.baseSeconds, finished);

        TrackTier(tier);

        float chestChance = 0.5f + (totalScv / 100f);
        int gold = 0;
        if (Random.value < chestChance)
        {
            gold = Random.Range(200, 601);
        }

        // fold gold into gameplay total
        GameMngr.Instance.AddGold(gold);

        logBox.Log($"Collecting Loot/Supplies ({task.baseSeconds}s): time finished - {finished:0}s, Loot: {gold} worth of GOLD");
    }

    void ResolveWagon(TaskSlot task, List<string> names)
    {
        string name = names[0];
        float haste = GetEffectiveStat(name, StatType.Haste);
        float finished = Mathf.Max(0f, task.baseSeconds - haste);
        var tier = GetTier(task.baseSeconds, finished);

        TrackTier(tier);

        string condition;
        if (haste >= 4) condition = "fixed (100%)";
        else if (haste >= 2) condition = $"slightly fixed ({Random.Range(60,91)}%)";
        else if (haste >= 1) condition = $"slightly damaged ({Random.Range(30,51)}%)";
        else condition = $"badly damaged ({Random.Range(5,21)}%)";

        logBox.Log($"Wagon - {condition}");
    }

    void TrackTier(TaskTier tier)
    {
        if (tier == TaskTier.Success || tier == TaskTier.Mediocre)
            successCount++;
        else
            failCount++;
    }

    void ApplyDayTrust()
    {
        int trustChange = 0;
        trustChange += successCount * 1;
        trustChange -= failCount * 1;

        if (successCount == 3) trustChange += 3; // all 3 succeeded bonus
        if (failCount == 3) trustChange -= 1;    // all 3 failed extra penalty

        GameMngr.Instance.AdjustTrust(trustChange);

        logBox.Log($"Trust {(trustChange >= 0 ? "+" : "")}{trustChange}");
    }
}