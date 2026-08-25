using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskTimerUI : MonoBehaviour
{
    public TMP_Text timerLabel;
    public Button skipButton;

    private bool skipRequested;

    public IEnumerator RunTimer(float displaySeconds, float realSeconds)
    {
        skipRequested = false;
        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(() => skipRequested = true);

        gameObject.SetActive(true);

        float elapsed = 0f;
        float speedMultiplier = displaySeconds / realSeconds;

        while (elapsed < realSeconds && !skipRequested)
        {
            elapsed += Time.deltaTime;
            float displayValue = Mathf.Max(0, displaySeconds - (elapsed * speedMultiplier));
            timerLabel.text = Mathf.CeilToInt(displayValue).ToString();
            yield return null;
        }

        timerLabel.text = "0";
        gameObject.SetActive(false);
    }
}