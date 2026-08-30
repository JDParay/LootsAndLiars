using UnityEngine;
using UnityEngine.SceneManagement;

public class Start : MonoBehaviour
{
    [Header("Scene to load on Start")]
    public string gameSceneName;   

    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    public void OnStartClicked()
    {
        SceneManager.LoadScene(gameSceneName);
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void OnOptionsClicked()
    {
        if (optionsPanel.activeSelf)
        {
            optionsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            creditsPanel.SetActive(false);
        }
        else
        {
            mainMenuPanel.SetActive(false);
            optionsPanel.SetActive(true);
            creditsPanel.SetActive(false);
        }
    }

    public void OnCreditsClicked()
    {
        if (creditsPanel.activeSelf)
        {
            optionsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            creditsPanel.SetActive(false);
        }
        else
        {
            mainMenuPanel.SetActive(false);
            optionsPanel.SetActive(false);
            creditsPanel.SetActive(true);
        }
    }
}