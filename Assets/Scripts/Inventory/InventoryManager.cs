using UnityEngine;
using System.Collections.Generic;
using Magic.Inventory;
using Magic.Data;

namespace Magic.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Database")]
        public ItemDatabase itemDatabase;
        [SerializeField] private PlayerDataManager _player;

        public List<ItemInstance> items => _player.items;
        public event System.Action OnInventoryChanged;
        public event System.Action OnLoadoutChanged;

        public void NotifyInventoryChanged()
        {
            OnInventoryChanged?.Invoke();
        }

        public void NotifyLoadoutChanged()
        {
            OnLoadoutChanged?.Invoke();
        }

        public void SortInventory()
        {
            if (PlayerDataManager.Instance == null) return;

            var targetList = items;
            if (targetList == null || targetList.Count == 0) return;

            try
            {
                targetList.Sort((a, b) =>
                {
                    if (a == null && b == null) return 0;
                    if (a == null) return 1;
                    if (b == null) return -1;

                    // 1. 기본 정렬: 카테고리 (ItemType 오름차순)
                    int typeCompare = a.Type.CompareTo(b.Type);
                    if (typeCompare != 0) return typeCompare;

                    // 2. 희귀도 정렬 (내림차순: 높은 등급이 앞)
                    int rarityCompare = b.Rarity.CompareTo(a.Rarity);
                    if (rarityCompare != 0) return rarityCompare;

                    // 3. 신규 획득순 정렬 (내림차순: 가장 최근에 획득한 것이 앞)
                    int acquisitionCompare = b.acquisitionId.CompareTo(a.acquisitionId);
                    if (acquisitionCompare != 0) return acquisitionCompare;

                    // 4. 이름순 정렬 (오름차순)
                    return string.Compare(a.ItemName ?? "", b.ItemName ?? "");
                });

                Debug.Log("[InventoryManager] 인벤토리 정렬 완료 (종류 -> 희귀도 -> 획득순 -> 이름순)");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[InventoryManager] 정렬 중 오류 발생: {e}");
            }

            NotifyInventoryChanged();
        }

        public event System.Action<Item_Scroll> OnScrollEquipped;
        public event System.Action<Item_Ink> OnInkEquipped;
        public event System.Action<Item_Pen> OnPenEquipped;
        public event System.Action<Item_Wand> OnWandEquipped;

        public Item_Scroll EquippedScroll
        {
            get => _player.equippedScroll;
            set
            {
                _player.equippedScroll = value;
                OnScrollEquipped?.Invoke(value);
            }
        }

        public Item_Ink EquippedInk
        {
            get => _player.equippedInk;
            set
            {
                _player.equippedInk = value;
                OnInkEquipped?.Invoke(value);
            }
        }

        public Item_Pen EquippedPen
        {
            get => _player.equippedPen;
            set
            {
                _player.equippedPen = value;
                OnPenEquipped?.Invoke(value);
            }
        }

        public Item_Wand EquippedWand
        {
            get => _player.equippedWand;
            set
            {
                _player.equippedWand = value;
                OnWandEquipped?.Invoke(value);
            }
        }

        public List<ItemInstance> combatLoadout => _player.combatLoadout;

        public void InitInstance()
        {
            Instance = this;
        }

        public void AddToLoadout(ItemInstance item)
        {
            if (!combatLoadout.Contains(item) && items.Contains(item))
            {
                combatLoadout.Add(item);
                NotifyLoadoutChanged();
            }
        }

        public void RemoveFromLoadout(ItemInstance item)
        {
            if (combatLoadout.Contains(item))
            {
                combatLoadout.Remove(item);
                NotifyLoadoutChanged();
            }
        }

        public void ClearLoadout()
        {
            combatLoadout.Clear();
            NotifyLoadoutChanged();
        }

        [Header("Inventory Capacity")]
        public int baseInventoryCapacity = 20;

        public int GetMaxCapacity()
        {
            int upgradeBonus = Magic.Upgrade.UpgradeManager.Instance != null
                ? Mathf.RoundToInt(Magic.Upgrade.UpgradeManager.Instance.GetTotalUpgradeValue(Magic.Upgrade.UpgradeType.InventoryCapacity))
                : 0;
            return baseInventoryCapacity + upgradeBonus;
        }

        public bool AddItem(ItemInstance item)
        {
            if (items.Count >= GetMaxCapacity())
            {
                Debug.LogWarning("[InventoryManager] 인벤토리가 가득 찼습니다! (Inventory is full)");
                return false;
            }
            items.Add(item);
            OnInventoryChanged?.Invoke();
            return true;
        }

        public void RemoveItem(ItemInstance item)
        {
            items.Remove(item);
            OnInventoryChanged?.Invoke();
            if (combatLoadout.Contains(item))
            {
                combatLoadout.Remove(item);
                NotifyLoadoutChanged();
            }

            // 만약 현재 장착 중인 아이템이 지워졌다면 다음 아이템으로 자동 장착
            if (EquippedScroll == item) EquippedScroll = CycleItem<Item_Scroll>(null, 1);
            if (EquippedInk == item) EquippedInk = CycleItem<Item_Ink>(null, 1);
            if (EquippedPen == item) EquippedPen = CycleItem<Item_Pen>(null, 1);
            if (EquippedWand == item) EquippedWand = CycleItem<Item_Wand>(null, 1);
        }

        public void CycleScroll(int dir) { EquippedScroll = CycleItem(EquippedScroll, dir); }
        public void CycleInk(int dir) { EquippedInk = CycleItem(EquippedInk, dir); }
        public void CyclePen(int dir) { EquippedPen = CycleItem(EquippedPen, dir); }
        public void CycleWand(int dir) { EquippedWand = CycleItem(EquippedWand, dir); }

        private T CycleItem<T>(T currentItem, int dir) where T : ItemInstance
        {
            var list = GetItemsOfType<T>();
            if (list.Count == 0) return null;
            if (currentItem == null) return list[0];

            int idx = list.IndexOf(currentItem);
            idx += dir;
            if (idx >= list.Count) idx = 0;
            if (idx < 0) idx = list.Count - 1;

            return list[idx];
        }

        public List<T> GetItemsOfType<T>() where T : ItemInstance
        {
            List<T> result = new List<T>();
            foreach (var item in items)
            {
                if (item is T typedItem)
                {
                    result.Add(typedItem);
                }
            }
            return result;
        }
    }
}
