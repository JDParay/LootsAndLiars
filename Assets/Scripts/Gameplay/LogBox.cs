using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogBox : MonoBehaviour
{
    public ScrollRect scrollRect;
    public TMP_Text logText;

    public void Log(string message)
    {
        logText.text += (logText.text.Length > 0 ? "\n" : "") + message;
        Canvas.ForceUpdateCanvases(); 
        scrollRect.verticalNormalizedPosition = 0f; 
    }
}