using UnityEngine;

public enum ItemRarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4
}

public abstract class ItemDataSO : ScriptableObject
{
    public ItemType type;
    public ItemRarity rarity = ItemRarity.Common;
    public string itemName;
    [TextArea(2, 5)]
    public string itemDescription;
    public Sprite itemIcon;
    
    [Header("Economy")]
    [Tooltip("아이템의 기본 가치 (동화 기준)")]
    public int basePriceInCopper;

    [Header("Stacking")]
    [Tooltip("한 슬롯에 최대로 겹칠 수 있는 개수")]
    public int maxStack = 99;
}

