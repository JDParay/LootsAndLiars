using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Phase1 : MonoBehaviour
{
    public List<TaskSlot> tasks; // 3 entries: Fight(2), Loot(2), Wagon(1)
    public Button submitButton;
    public TMP_Text logBox; // or a scrollable log — TMP_Text appended for prototype

    private List<CharacterData> roster; // teammates + a placeholder "You" entry not needed if player also assignable
    private List<string> allNames;

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
                dd.onValueChanged.AddListener(_ => OnAnySelectionChanged());
            }
        }

        submitButton.onClick.AddListener(OnSubmitPressed);
        RefreshAll();
    }

    void OnAnySelectionChanged()
    {
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
            Log("No more than one name in the tasks.");
            return;
        }

        Log("Tasks submitted. Resolving...");
        // TODO: Phase 1 → Phase 2 resolution (stat-roll, imposter chance, log output)
        // This is the next chunk we'll build.
    }

    void Log(string message)
    {
        logBox.text += "\n" + message;
    }
}