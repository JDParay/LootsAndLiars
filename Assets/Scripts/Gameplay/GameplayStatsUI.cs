using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameplayStatsUI : MonoBehaviour
{
    public StatBox_Player playerBox;
    public StatBox[] teammateBoxes; // size 4, in the same order as GameManager.teammates

    public Button abandonButton;
    public string shopSceneName = "GameStart";

    void Start()
    {
        playerBox.SetStats(GameMngr.Instance.playerStats);

        var teammates = GameMngr.Instance.teammates;
        for (int i = 0; i < teammateBoxes.Length; i++)
        {
            teammateBoxes[i].SetCharacter(teammates[i]);
        }

        abandonButton.onClick.AddListener(AbandonRun);
    }

    void AbandonRun()
    {
        Destroy(GameMngr.Instance.gameObject);
        SceneManager.LoadScene(shopSceneName);
    }
}