using UnityEngine;
using UnityEngine.UI;

public class HowToPopupMngr : MonoBehaviour
{
    public Button howToButton;
    public GameObject[] panels;   // just the popup panels, in order

    private int currentIndex = -1;

    void Start()
    {
        foreach (var panel in panels)
        {
            SetPanelActive(panel, false);
        }

        howToButton.onClick.AddListener(OpenHowTo);

        foreach (var panel in panels)
        {
            Button panelButton = panel.GetComponent<Button>();
            if (panelButton == null)
                panelButton = panel.AddComponent<Button>();

            panelButton.onClick.AddListener(AdvancePopup);
        }
    }

    void OpenHowTo()
    {
        howToButton.interactable = false;
        currentIndex = 0;
        ShowStep(currentIndex);
    }

    void ShowStep(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            SetPanelActive(panels[i], i == index);
        }
    }

    void AdvancePopup()
    {
        currentIndex++;

        if (currentIndex < panels.Length)
        {
            ShowStep(currentIndex);
        }
        else
        {
            foreach (var panel in panels)
                SetPanelActive(panel, false);

            howToButton.interactable = true;
        }
    }

    void SetPanelActive(GameObject panel, bool isActive)
    {
        panel.SetActive(isActive);

        PopupExtras extras = panel.GetComponent<PopupExtras>();
        if (extras != null)
        {
            foreach (var extra in extras.extraObjects)
                extra.SetActive(isActive);
        }
    }
}