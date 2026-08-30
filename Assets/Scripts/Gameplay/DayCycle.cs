using UnityEngine;
using UnityEngine.Events;
using TMPro;   // add this using

public class DayCycleMngr : MonoBehaviour
{
    public static DayCycleMngr Instance;

    public enum CyclePhase { Day, Night }

    public int currentDay = 1;
    public CyclePhase currentPhase = CyclePhase.Day;

    public TMP_Text cycleDisplayText;

    public UnityEvent<int, CyclePhase> OnCycleChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        RaiseCycleChanged();
    }

    public void CompletePhase1()
    {
        currentPhase = CyclePhase.Night;
        RaiseCycleChanged();
    }

    public void CompletePhase2()
    {
        if (currentDay >= 5)
        {
            return;
        }

        currentDay++;
        currentPhase = CyclePhase.Day;
        RaiseCycleChanged();
    }

    void RaiseCycleChanged()  
    {
        if (cycleDisplayText != null)
            cycleDisplayText.text = $"{currentPhase} {currentDay}";

        OnCycleChanged?.Invoke(currentDay, currentPhase);
    }

    public bool IsFinalDay => currentDay >= 5;
}