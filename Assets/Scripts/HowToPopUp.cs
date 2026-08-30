using UnityEngine;
using UnityEngine.UI;

public class HowToPopupMngr : MonoBehaviour
{
    public Button howToButton;
    public GameObject[] popupPanels;

    private int currentIndex = -1;

    void Start()
    {
        foreach (var panel in popupPanels)
            panel.SetActive(false);

        howToButton.onClick.AddListener(OpenHowTo);

        foreach (var panel in popupPanels)
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
        ShowPopup(currentIndex);
    }

    void ShowPopup(int index)
    {
        for (int i = 0; i < popupPanels.Length; i++)
            popupPanels[i].SetActive(i == index);
    }

    void AdvancePopup()
    {
        currentIndex++;

        if (currentIndex < popupPanels.Length)
        {
            ShowPopup(currentIndex);
        }
        else
        {
            foreach (var panel in popupPanels)
                panel.SetActive(false);

            howToButton.interactable = true;
        }
    }
}