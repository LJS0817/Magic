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
    }
}
