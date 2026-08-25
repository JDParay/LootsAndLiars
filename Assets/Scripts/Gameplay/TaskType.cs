public enum TaskType { FightMonsters, CollectLoot, FixWagon }

[System.Serializable]
public class TaskSlot
{
    public TaskType type;
    public float baseSeconds;
    public TMPro.TMP_Dropdown[] dropdowns; 
    public TMPro.TMP_Text timeLabel;
}