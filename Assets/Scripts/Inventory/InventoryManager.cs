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

        public List<ItemInstance> items => PlayerDataManager.Instance != null ? PlayerDataManager.Instance.items : new List<ItemInstance>();
        public event System.Action OnInventoryChanged;

        public void NotifyInventoryChanged()
        {
            OnInventoryChanged?.Invoke();
        }

        public event System.Action<Item_Scroll> OnScrollEquipped;
        public event System.Action<Item_Ink> OnInkEquipped;
        public event System.Action<Item_Pen> OnPenEquipped;

        public Item_Scroll EquippedScroll
        {
            get => PlayerDataManager.Instance != null ? PlayerDataManager.Instance.equippedScroll : null;
            set
            {
                if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.equippedScroll = value;
                OnScrollEquipped?.Invoke(value);
            }
        }

        public Item_Ink EquippedInk
        {
            get => PlayerDataManager.Instance != null ? PlayerDataManager.Instance.equippedInk : null;
            set
            {
                if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.equippedInk = value;
                OnInkEquipped?.Invoke(value);
            }
        }

        public Item_Pen EquippedPen
        {
            get => PlayerDataManager.Instance != null ? PlayerDataManager.Instance.equippedPen : null;
            set
            {
                if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.equippedPen = value;
                OnPenEquipped?.Invoke(value);
            }
        }

        public List<ItemInstance> combatLoadout => PlayerDataManager.Instance != null ? PlayerDataManager.Instance.combatLoadout : new List<ItemInstance>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDefaultItems();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeDefaultItems()
        {
            // 게임 시작 시 인벤토리가 비어있다면 테스트용 아이템 지급
            if (items.Count == 0)
            {
                if (itemDatabase == null)
                {
                    itemDatabase = Resources.Load<ItemDatabase>("Items/ItemDatabase");
                }
                
                if (itemDatabase != null)
                {
                    var scroll1 = itemDatabase.CreateScrollInstance("해진 연습용 양피지");
                    var scroll2 = itemDatabase.CreateScrollInstance("표준 마법 스크롤");
                    var impactScroll = itemDatabase.CreateScrollInstance("단단한 가죽 스크롤");
                    if (impactScroll != null)
                    {
                        impactScroll.isEmpty = false;
                        if (impactScroll.ScrollData != null)
                            impactScroll.ScrollData.spellName = "Impact";
                    }

                    var normalInk = itemDatabase.CreateInkInstance("표준 마법 잉크");
                    var highInk = itemDatabase.CreateInkInstance("농축된 마력 잉크");
                    var basicPen = itemDatabase.CreatePenInstance("연습용 나무 펜");

                    if (scroll1 != null) { items.Add(scroll1); if (EquippedScroll == null) EquippedScroll = scroll1; }
                    if (scroll2 != null) items.Add(scroll2);
                    if (impactScroll != null) items.Add(impactScroll);
                    if (normalInk != null) { items.Add(normalInk); if (EquippedInk == null) EquippedInk = normalInk; }
                    if (highInk != null) items.Add(highInk);
                    if (basicPen != null) { items.Add(basicPen); if (EquippedPen == null) EquippedPen = basicPen; }

                    // 기본 전투 장비로 세팅 (테스트용)
                    if (scroll1 != null) combatLoadout.Add(scroll1);
                    if (impactScroll != null) combatLoadout.Add(impactScroll);
                    if (normalInk != null) combatLoadout.Add(normalInk);
                    if (basicPen != null) combatLoadout.Add(basicPen);

                    Debug.Log("[InventoryManager] 기본 테스트 아이템 및 로드아웃 지급 완료.");
                }
                else
                {
                    Debug.LogWarning("[InventoryManager] Items/ItemDatabase 에셋을 Resources에서 찾을 수 없습니다.");
                }
            }
        }

        /// <summary>
        /// 특정 펜 이름으로 펜을 로드하여 인벤토리에 추가합니다.
        /// </summary>
        public void AddPenByName(string penName)
        {
            if (itemDatabase == null) itemDatabase = Resources.Load<ItemDatabase>("Items/ItemDatabase");
            if (itemDatabase != null)
            {
                var newPen = itemDatabase.CreatePenInstance(penName);
                if (newPen != null)
                {
                    AddItem(newPen);
                    Debug.Log($"[InventoryManager] 펜 '{penName}'이 인벤토리에 추가되었습니다.");
                }
                else
                {
                    Debug.LogWarning($"[InventoryManager] 펜 '{penName}'을 데이터베이스에서 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("[InventoryManager] ItemDatabase 에셋을 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// 특정 등급의 임의의 펜을 로드하여 인벤토리에 추가합니다.
        /// </summary>
        public void AddRandomPenOfGrade(string grade)
        {
            if (itemDatabase == null) itemDatabase = Resources.Load<ItemDatabase>("Items/ItemDatabase");
            if (itemDatabase != null)
            {
                var penData = itemDatabase.GetRandomPenOfGrade(grade);
                if (penData != null)
                {
                    var newPen = new Item_Pen(penData);
                    AddItem(newPen);
                    Debug.Log($"[InventoryManager] 등급 '{grade}'의 펜 '{newPen.ItemName}'이 인벤토리에 추가되었습니다.");
                }
                else
                {
                    Debug.LogWarning($"[InventoryManager] 등급 '{grade}'의 펜을 데이터베이스에서 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("[InventoryManager] ItemDatabase 에셋을 찾을 수 없습니다.");
            }
        }

        public void AddToLoadout(ItemInstance item)
        {
            if (!combatLoadout.Contains(item) && items.Contains(item))
            {
                combatLoadout.Add(item);
            }
        }

        public void RemoveFromLoadout(ItemInstance item)
        {
            combatLoadout.Remove(item);
        }

        public void ClearLoadout()
        {
            combatLoadout.Clear();
        }

        public void AddItem(ItemInstance item)
        {
            items.Add(item);
            OnInventoryChanged?.Invoke();
        }

        public void RemoveItem(ItemInstance item)
        {
            items.Remove(item);
            OnInventoryChanged?.Invoke();
            if (combatLoadout.Contains(item))
            {
                combatLoadout.Remove(item);
            }

            // 만약 현재 장착 중인 아이템이 지워졌다면 다음 아이템으로 자동 장착
            if (EquippedScroll == item) EquippedScroll = CycleItem<Item_Scroll>(null, 1);
            if (EquippedInk == item) EquippedInk = CycleItem<Item_Ink>(null, 1);
            if (EquippedPen == item) EquippedPen = CycleItem<Item_Pen>(null, 1);
        }

        public void CycleScroll(int dir) { EquippedScroll = CycleItem(EquippedScroll, dir); }
        public void CycleInk(int dir) { EquippedInk = CycleItem(EquippedInk, dir); }
        public void CyclePen(int dir) { EquippedPen = CycleItem(EquippedPen, dir); }

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
