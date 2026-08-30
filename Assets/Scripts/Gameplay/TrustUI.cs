using UnityEngine;
using UnityEngine.UI;

public class TrustBarUI : MonoBehaviour
{
    public Image[] pips;
    public Sprite filledPip;
    public Sprite emptyPip;

    void OnEnable()
    {
        if (GameMngr.Instance != null)
            GameMngr.Instance.OnTrustChanged.AddListener(UpdateBar);
    }

    void OnDisable()
    {
        if (GameMngr.Instance != null)
            GameMngr.Instance.OnTrustChanged.RemoveListener(UpdateBar);
    }

    void UpdateBar(int current, int max)
    {
        int pipValue = max / pips.Length;
        int filledCount = Mathf.CeilToInt((float)current / pipValue);

        for (int i = 0; i < pips.Length; i++)
            pips[i].sprite = i < filledCount ? filledPip : emptyPip;
    }
}