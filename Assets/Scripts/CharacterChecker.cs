using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterChecker : MonoBehaviour
{
    public CharacterData[] characters;

    [Header("UI References")]
    public Image portraitImage;
    public TMP_Text strLabel;
    public TMP_Text hstLabel;
    public TMP_Text wisLabel;
    public TMP_Text scvLabel;
    public TMP_Text titleLabel;
    public Button leftButton;
    public Button rightButton;

    private int currentIndex = 0;

    void Start()
    {
        leftButton.onClick.AddListener(ShowPrevious);
        rightButton.onClick.AddListener(ShowNext);
    }

    void ShowPrevious()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = characters.Length - 1;
        RefreshDisplay();
    }

    void ShowNext()
    {
        currentIndex++;
        if (currentIndex >= characters.Length) currentIndex = 0; 
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        CharacterData current = characters[currentIndex];

        portraitImage.sprite = current.portrait;
        portraitImage.enabled = current.portrait != null;

        strLabel.text = $"STR:\t{current.runtimeStats.strength}";
        hstLabel.text = $"HST:\t{current.runtimeStats.haste}";
        wisLabel.text = $"WIS:\t{current.runtimeStats.wisdom}";
        scvLabel.text = $"SCV:\t{current.runtimeStats.scavenge}";

        titleLabel.text = current.title;
    }
}