using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KickMngr : MonoBehaviour
{
    public GameObject actionBox3;
    public Toggle[] kickToggles; // size 4
    public Button kickConfirmButton;
    public Button kickBackButton;
    public LogBox logBox;

    private List<CharacterData> remaining;
    private Action<bool> onComplete;

    void Awake()
    {
        kickConfirmButton.onClick.AddListener(OnKickConfirmed);
        kickBackButton.onClick.AddListener(OnBackPressed);
    }

    public void BeginKickPhase(List<CharacterData> currentRoster, Action<bool> completeCallback)
    {
        onComplete = completeCallback;
        remaining = currentRoster.Where(c => !GameMngr.Instance.IsKicked(c)).ToList();

        actionBox3.SetActive(true);

        for (int i = 0; i < kickToggles.Length; i++)
        {
            if (i < remaining.Count)
            {
                kickToggles[i].gameObject.SetActive(true);
                var label = kickToggles[i].GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = remaining[i].characterName;

                kickToggles[i].onValueChanged.RemoveAllListeners();
                kickToggles[i].isOn = false;

                int index = i;
                kickToggles[i].onValueChanged.AddListener(_ => OnToggleChanged(index));
            }
            else
            {
                kickToggles[i].gameObject.SetActive(false);
            }
        }

        kickConfirmButton.interactable = false;
    }

    void OnToggleChanged(int changedIndex)
    {
        if (kickToggles[changedIndex].isOn)
        {
            for (int i = 0; i < kickToggles.Length; i++)
            {
                if (i != changedIndex) kickToggles[i].isOn = false;
            }
        }

        kickConfirmButton.interactable = kickToggles.Any(t => t.isOn);
    }

    void OnKickConfirmed()
    {
        int selectedIndex = System.Array.FindIndex(kickToggles, t => t.isOn);

        if (selectedIndex >= 0 && selectedIndex < remaining.Count)
        {
            var target = remaining[selectedIndex];
            GameMngr.Instance.KickMember(target);
            logBox.Log($"{target.characterName} was kicked from the party.");
        }

        Close(kicked: true);
    }

    void OnBackPressed()
    {
        Close(kicked: false);
    }

    void Close(bool kicked)
    {
        actionBox3.SetActive(false);
        onComplete?.Invoke(kicked);
    }
}