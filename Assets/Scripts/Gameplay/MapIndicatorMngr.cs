using UnityEngine;

public class MapIndicatorMngr : MonoBehaviour
{
    public GameObject[] indicators;

    void OnEnable()
    {
        if (DayCycleMngr.Instance != null)
            DayCycleMngr.Instance.OnCycleChanged.AddListener(UpdateIndicator);
    }

    void OnDisable()
    {
        if (DayCycleMngr.Instance != null)
            DayCycleMngr.Instance.OnCycleChanged.RemoveListener(UpdateIndicator);
    }

    void Start()
    {
        if (DayCycleMngr.Instance != null)
            UpdateIndicator(DayCycleMngr.Instance.currentDay, DayCycleMngr.Instance.currentPhase);
    }

    void UpdateIndicator(int day, DayCycleMngr.CyclePhase phase)
    {
        int activeIndex = (phase == DayCycleMngr.CyclePhase.Day) ? day : day + 1;

        for (int i = 0; i < indicators.Length; i++)
        {
            indicators[i].SetActive((i + 1) == activeIndex);
        }
    }
}