using UnityEngine;

[System.Serializable]
public class StatBlock
{
    public int strength;
    public int haste;
    public int wisdom;
    public int scavenge;

    public int Get(StatType type)
    {
        switch (type)
        {
            case StatType.Strength: return strength;
            case StatType.Haste: return haste;
            case StatType.Wisdom: return wisdom;
            case StatType.Scavenge: return scavenge;
            default: return 0;
        }
    }

    public void Add(StatType type, int amount)
    {
        switch (type)
        {
            case StatType.Strength: strength += amount; break;
            case StatType.Haste: haste += amount; break;
            case StatType.Wisdom: wisdom += amount; break;
            case StatType.Scavenge: scavenge += amount; break;
        }
    }
}

public enum StatType { Strength, Haste, Wisdom, Scavenge }