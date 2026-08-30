using UnityEngine;
using TMPro;

public class GoldDisplayUI : MonoBehaviour
{
    public TMP_Text goldText;

    void OnEnable()
    {
        if (GameMngr.Instance != null)
            GameMngr.Instance.OnGoldChanged.AddListener(UpdateDisplay);
    }

    void OnDisable()
    {
        if (GameMngr.Instance != null)
            GameMngr.Instance.OnGoldChanged.RemoveListener(UpdateDisplay);
    }

    void Start()
    {
        if (GameMngr.Instance != null)
            UpdateDisplay(GameMngr.Instance.gold);
    }

    void UpdateDisplay(int currentGold)
    {
        goldText.text = $"{currentGold} GOLD";
    }
}