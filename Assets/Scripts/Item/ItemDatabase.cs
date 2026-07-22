using UnityEngine;
using System.Collections.Generic;

namespace Magic.Inventory
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Magic/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [Header("Item Lists")]
        public List<ItemPenSO> pens = new List<ItemPenSO>();
        public List<ItemInkSO> inks = new List<ItemInkSO>();
        public List<ItemScrollSO> scrolls = new List<ItemScrollSO>();
        public List<ItemWandSO> wands = new List<ItemWandSO>();
        public List<ItemPotionSO> potions = new List<ItemPotionSO>();
        public List<ItemPouchSO> pouches = new List<ItemPouchSO>();

        private Dictionary<string, ItemDataSO> _itemCache;
        private Dictionary<ItemRarity, List<ItemPenSO>> _penGradeCache;

        public void InitializeCache()
        {
            if (_itemCache != null && _itemCache.Count == (pens.Count + inks.Count + scrolls.Count + wands.Count + potions.Count + pouches.Count)) return;

            _itemCache = new Dictionary<string, ItemDataSO>();
            _penGradeCache = new Dictionary<ItemRarity, List<ItemPenSO>>();

            foreach (var pen in pens)
            {
                if (pen != null && !_itemCache.ContainsKey(pen.itemName))
                {
                    _itemCache.Add(pen.itemName, pen);

                    if (!_penGradeCache.ContainsKey(pen.rarity))
                        _penGradeCache[pen.rarity] = new List<ItemPenSO>();
                    _penGradeCache[pen.rarity].Add(pen);
                }
            }

            foreach (var ink in inks)
            {
                if (ink != null && !_itemCache.ContainsKey(ink.itemName))
                    _itemCache.Add(ink.itemName, ink);
            }

            foreach (var scroll in scrolls)
            {
                if (scroll != null && !_itemCache.ContainsKey(scroll.itemName))
                    _itemCache.Add(scroll.itemName, scroll);
            }

            foreach (var wand in wands)
            {
                if (wand != null && !_itemCache.ContainsKey(wand.itemName))
                    _itemCache.Add(wand.itemName, wand);
            }

            foreach (var potion in potions)
            {
                if (potion != null && !_itemCache.ContainsKey(potion.itemName))
                    _itemCache.Add(potion.itemName, potion);
            }

            foreach (var pouch in pouches)
            {
                if (pouch != null && !_itemCache.ContainsKey(pouch.itemName))
                    _itemCache.Add(pouch.itemName, pouch);
            }
        }

        public T GetItemData<T>(string itemName) where T : ItemDataSO
        {
            InitializeCache();
            if (_itemCache.TryGetValue(itemName, out var item))
            {
                return item as T;
            }
            return null;
        }

        public ItemPenSO GetRandomPenOfGrade(ItemRarity rarity)
        {
            InitializeCache();
            if (_penGradeCache.TryGetValue(rarity, out var filtered) && filtered.Count > 0)
            {
                return filtered[Random.Range(0, filtered.Count)];
            }
            return null;
        }

        // 인스턴스화 헬퍼 메서드들
        public Item_Pen CreatePenInstance(string itemName)
        {
            var penData = GetItemData<ItemPenSO>(itemName);
            return penData != null ? new Item_Pen(penData) : null;
        }

        public Item_Ink CreateInkInstance(string itemName)
        {
            var inkData = GetItemData<ItemInkSO>(itemName);
            return inkData != null ? new Item_Ink(inkData) : null;
        }

        public Item_Scroll CreateScrollInstance(string itemName)
        {
            var scrollData = GetItemData<ItemScrollSO>(itemName);
            return scrollData != null ? new Item_Scroll(scrollData) : null;
        }

        public Item_Wand CreateWandInstance(string itemName)
        {
            var wandData = GetItemData<ItemWandSO>(itemName);
            return wandData != null ? new Item_Wand(wandData) : null;
        }
    }
}
