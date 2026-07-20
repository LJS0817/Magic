using UnityEngine;

namespace Magic.Inventory
{
    public enum ItemType { Scroll, Ink, Material, Potion, Pen, RecipeBook, Wand }

    [System.Serializable]
    public abstract class ItemInstance
    {
        public ItemDataSO data;
        public long acquisitionId;

        // 자주 접근하는 데이터에 대한 편의성 프로퍼티
        public string ItemName => data != null ? data.itemName : "Unknown";
        public string ItemDescription => data != null ? data.itemDescription : "";
        public ItemType Type => data != null ? data.type : ItemType.Pen;
        public ItemRarity Rarity => data != null ? data.rarity : ItemRarity.Common;
        public Sprite ItemIcon => data != null ? data.itemIcon : null;

        private static long _lastAcquisitionTicks = 0;
        private static long GetUniqueAcquisitionId()
        {
            long ticks = System.DateTime.UtcNow.Ticks;
            if (ticks <= _lastAcquisitionTicks) ticks = _lastAcquisitionTicks + 1;
            _lastAcquisitionTicks = ticks;
            return ticks;
        }

        public ItemInstance(ItemDataSO data)
        {
            this.data = data;
            this.acquisitionId = GetUniqueAcquisitionId();
        }

        public override string ToString()
        {
            return $"{ItemName}[{Type} | {Rarity}] : {ItemDescription}";
        }

        public static bool operator ==(ItemInstance a, ItemInstance b)
        {
            bool aIsNull = ReferenceEquals(a, null) || a.data == null;
            bool bIsNull = ReferenceEquals(b, null) || b.data == null;

            if (aIsNull && bIsNull) return true;
            if (aIsNull || bIsNull) return false;

            return ReferenceEquals(a, b);
        }

        public static bool operator !=(ItemInstance a, ItemInstance b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is ItemInstance other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            // ReferenceEquals(this, null) 은 GetHashCode 안에서는 보통 불리지 않지만 방어적으로 처리
            return data != null ? data.GetHashCode() : base.GetHashCode();
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

    [System.Serializable]
    public class Item_RecipeBook : ItemInstance
    {
        public ItemRecipeBookSO RecipeBookData => data as ItemRecipeBookSO;

        public Item_RecipeBook(ItemRecipeBookSO data) : base(data)
        {
        }

        public void Consume()
        {
            if (RecipeBookData == null) return;

            if (RecipeBookData.isHintOnly)
            {
                Magic.Data.PlayerDataManager.Instance.UnlockHint(RecipeBookData.targetSpellName);
            }
            else
            {
                Magic.Data.PlayerDataManager.Instance.UnlockRecipe(RecipeBookData.targetSpellName);
            }
        }
    }

    [System.Serializable]
    public class Item_Wand : ItemInstance
    {
        public ItemWandSO WandData => data as ItemWandSO;

        public Item_Wand(ItemWandSO wandData) : base(wandData)
        {
        }
    }
}