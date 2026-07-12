using UnityEngine;

namespace Magic.Inventory
{
    public abstract class ItemDataSO : ScriptableObject
    {
        public ItemType type;
        public string itemName;
        [TextArea(2, 5)]
        public string itemDescription;
        public Sprite itemIcon;
        
        [Header("Economy")]
        [Tooltip("아이템의 기본 가치 (동화 기준)")]
        public int basePriceInCopper;
    }
}
