using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Phase2 : MonoBehaviour
{
    public GameObject phase2Panel;
    public Button actionButton;
    public TMP_Text actionButtonLabel;
    public Toggle[] suspectToggles; // size 4
    public Toggle skipToggle;
    public LogBox logBox;

    private List<CharacterData> roster;
    private Action onComplete;

    public void BeginMarkingPhase(List<CharacterData> teamRoster, Action completeCallback)
    {
        roster = teamRoster;
        onComplete = completeCallback;

        phase2Panel.SetActive(true);
        actionButtonLabel.text = "Confirm";
        actionButton.interactable = false;

        for (int i = 0; i < suspectToggles.Length; i++)
        {
            var teammate = roster[i];
            var label = suspectToggles[i].GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = teammate.characterName;

            suspectToggles[i].onValueChanged.RemoveAllListeners();
            suspectToggles[i].isOn = false;
            suspectToggles[i].onValueChanged.AddListener(_ => OnMarkToggleChanged());
        }

        skipToggle.onValueChanged.RemoveAllListeners();
        skipToggle.isOn = false;
        skipToggle.onValueChanged.AddListener(_ => OnSkipToggleChanged());

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(ConfirmMarking);
    }

    void OnSkipToggleChanged()
    {
        if (skipToggle.isOn)
        {
            foreach (var t in suspectToggles) t.isOn = false;
        }
        RefreshButton();
    }

    void OnMarkToggleChanged()
    {
        int checkedCount = suspectToggles.Count(t => t.isOn);

        if (checkedCount > 0 && skipToggle.isOn)
        {
            skipToggle.isOn = false;
        }

        // Once 2 are checked, disable the rest so a 3rd can't be picked
        bool atLimit = checkedCount >= 2;
        foreach (var t in suspectToggles)
        {
            if (!t.isOn) t.interactable = !atLimit;
        }

        RefreshButton();
    }

    void RefreshButton()
    {
        int checkedCount = suspectToggles.Count(t => t.isOn);
        actionButton.interactable = (checkedCount == 2) || skipToggle.isOn;
    }

    void ConfirmMarking()
    {
        GameMngr.Instance.ClearMarksForNewDay();

        if (!skipToggle.isOn)
        {
            var marked = new List<CharacterData>();
            for (int i = 0; i < suspectToggles.Length; i++)
            {
                if (suspectToggles[i].isOn)
                    marked.Add(roster[i]);
            }

            foreach (var character in marked)
            {
                GameMngr.Instance.MarkSuspect(character);
            }

            GameMngr.Instance.AdjustTrust(-1);
            logBox.Log($"Marked: {string.Join(", ", marked.Select(c => c.characterName))} (-1 Trust)");
        }
        else
        {
            logBox.Log("No one was marked tonight.");
        }

        // Re-enable toggles for next time this phase runs
        foreach (var t in suspectToggles) t.interactable = true;

        phase2Panel.SetActive(false);
        onComplete?.Invoke();
    }
}