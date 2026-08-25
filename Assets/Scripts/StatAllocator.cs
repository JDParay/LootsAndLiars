using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatAllocator : MonoBehaviour
{
    [System.Serializable]
    public class StatRow
    {
        public StatType type;
        public TMP_Text valueLabel;
        public Button plusButton;
        public Button minusButton;
    }

    public StatRow[] rows;
    public TMP_Text pelletsRemainingLabel;
    public int totalPellets = 10;
    public int maxPerStat = 5;

    private StatBlock playerStats = new StatBlock();
    private int pelletsUsed = 0;
    public bool AllPelletsSpent() => pelletsUsed >= totalPellets;

    void Start()
    {
        foreach (var row in rows)
        {
            var capturedType = row.type;
            row.plusButton.onClick.AddListener(() => TryAdd(capturedType));
            row.minusButton.onClick.AddListener(() => TryRemove(capturedType));
        }
        RefreshUI();
    }

    void TryAdd(StatType type)
    {
        if (pelletsUsed >= totalPellets) return;
        if (playerStats.Get(type) >= maxPerStat) return;

        playerStats.Add(type, 1);
        pelletsUsed++;
        RefreshUI();
    }

    void TryRemove(StatType type)
    {
        if (playerStats.Get(type) <= 0) return;

        playerStats.Add(type, -1);
        pelletsUsed--;
        RefreshUI();
    }

    void RefreshUI()
    {
        foreach (var row in rows)
        {
            row.valueLabel.text = playerStats.Get(row.type).ToString();
            row.plusButton.interactable = pelletsUsed < totalPellets && playerStats.Get(row.type) < maxPerStat;
            row.minusButton.interactable = playerStats.Get(row.type) > 0;
        }
        pelletsRemainingLabel.text = $"[{totalPellets - pelletsUsed} left]";
    }

    public StatBlock GetFinalStats() => playerStats;
}