using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "LL/Character")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public string title;
    public StatBlock baseStats;
    public Sprite portrait;
    public Sprite gameplayPortrait;

    [System.NonSerialized]
    public StatBlock runtimeStats;
    public void ResetRuntimeStats()
    {
        runtimeStats = new StatBlock
        {
            strength = baseStats.strength,
            haste = baseStats.haste,
            wisdom = baseStats.wisdom,
            scavenge = baseStats.scavenge
        };
    }
}