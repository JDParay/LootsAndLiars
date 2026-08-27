using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "LL/Item")]
public class ShopItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public int cost;
    public StatType[] statsAffected;
    public int amountPerStat = 1;
    public Sprite icon;
    public Sprite sparkleIcon;
}