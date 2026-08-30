using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndingSceneUI : MonoBehaviour
{
    public TMP_Text endingMessage;
    public TMP_Text goldTallyText;
    public Image imposterPortrait1;
    public Image imposterPortrait2;
    public TMP_Text imposterNamesText;
    public string menuSceneName = "Menu"; 

    void Start()
    {
        goldTallyText.text = $"{GameMngr.Instance.gold} GOLD";

        var imposters = GameMngr.Instance.imposters;
        if (imposters.Count > 0)
        {
            imposterPortrait1.sprite = imposters[0].portrait;
            imposterPortrait1.enabled = true;
        }
        if (imposters.Count > 1)
        {
            imposterPortrait2.sprite = imposters[1].portrait;
            imposterPortrait2.enabled = true;
        }

        if (imposterNamesText != null)
        {
            imposterNamesText.text = string.Join(" & ", imposters.ConvertAll(c => c.characterName));
        }
    }

    public void OnBackToMenuClicked()   
    {
        SceneFader.Instance.FadeToScene(menuSceneName);
    }
}