using UnityEngine;

namespace Magic.Inventory
{
    public enum ItemType { Scroll, Ink, Material, Potion, Pen }

    [System.Serializable]
    public abstract class ItemInstance
    {
        public ItemDataSO data;

        // 자주 접근하는 데이터에 대한 편의성 프로퍼티
        public string ItemName => data != null ? data.itemName : "Unknown";
        public string ItemDescription => data != null ? data.itemDescription : "";
        public ItemType Type => data != null ? data.type : ItemType.Pen;
        public Sprite ItemIcon => data != null ? data.itemIcon : null;

        public ItemInstance(ItemDataSO data)
        {
            this.data = data;
        }
    }

    [System.Serializable]
    public class Item_Scroll : ItemInstance
    {
        public bool isEmpty = true;
        public int currentDurability;

        public ItemScrollSO ScrollData => data as ItemScrollSO;

        public Item_Scroll(ItemScrollSO scrollData, bool isEmpty = true, int currentDurability = -1) : base(scrollData)
        {
            this.isEmpty = isEmpty;
            this.currentDurability = currentDurability < 0 ? (scrollData != null ? scrollData.maxDurability : 5) : currentDurability;
        }
    }

    [System.Serializable]
    public class Item_Ink : ItemInstance
    {
        public float currentAmount;

        public ItemInkSO InkData => data as ItemInkSO;

        public Item_Ink(ItemInkSO inkData, float currentAmount = -1f) : base(inkData)
        {
            this.currentAmount = currentAmount < 0f ? (inkData != null ? inkData.maxAmount : 100f) : currentAmount;
        }
    }

    [System.Serializable]
    public class Item_Pen : ItemInstance
    {
        public float currentInkCapacity;

        public ItemPenSO PenData => data as ItemPenSO;

        public Item_Pen(ItemPenSO penData, float currentCapacity = -1f) : base(penData)
        {
            if (penData != null)
            {
                // 마나 소모 펜이면 잉크통 0으로 시작, 아니면 최대치(또는 지정값)로 시작
                if (penData.consumesMana)
                {
                    this.currentInkCapacity = 0f;
                }
                else
                {
                    this.currentInkCapacity = currentCapacity < 0f ? penData.maxInkCapacity : currentCapacity;
                }
            }
            else
            {
                this.currentInkCapacity = 0f;
            }
        }
    }

    [System.Serializable]
    public class Item_Material : ItemInstance
    {
        public Item_Material(ItemDataSO data) : base(data)
        {
        }
    }
}