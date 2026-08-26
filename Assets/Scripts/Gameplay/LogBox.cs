using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogBox : MonoBehaviour
{
    public ScrollRect scrollRect;
    public TMP_Text logText;
    public RectTransform contentRect;

    [SerializeField] private float autoScrollThreshold = 0.05f;

    public void Log(string message)
    {
        bool isAtBottom = scrollRect.verticalNormalizedPosition <= autoScrollThreshold;

        logText.text += (logText.text.Length > 0 ? "\n" : "") + message;
        logText.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

        if (isAtBottom)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}