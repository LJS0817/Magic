using UnityEngine;
using System.Collections.Generic;
using Magic.Inventory;

namespace Magic.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Inventory Data")]
        public List<ItemInstance> items = new List<ItemInstance>();

        [Header("Combat Data")]
        public List<ItemInstance> combatLoadout = new List<ItemInstance>();

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
                ItemDatabase db = Resources.Load<ItemDatabase>("Items/ItemDatabase");
                if (db != null)
                {
                    var scroll1 = db.CreateScrollInstance("해진 연습용 양피지");
                    var scroll2 = db.CreateScrollInstance("표준 마법 스크롤");
                    var impactScroll = db.CreateScrollInstance("단단한 가죽 스크롤");
                    if (impactScroll != null)
                    {
                        impactScroll.isEmpty = false;
                        if (impactScroll.ScrollData != null)
                            impactScroll.ScrollData.spellName = "Impact";
                    }

                    var normalInk = db.CreateInkInstance("표준 마법 잉크");
                    var highInk = db.CreateInkInstance("농축된 마력 잉크");
                    var basicPen = db.CreatePenInstance("연습용 나무 펜");

                    if (scroll1 != null) items.Add(scroll1);
                    if (scroll2 != null) items.Add(scroll2);
                    if (impactScroll != null) items.Add(impactScroll);
                    if (normalInk != null) items.Add(normalInk);
                    if (highInk != null) items.Add(highInk);
                    if (basicPen != null) items.Add(basicPen);

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
            ItemDatabase db = Resources.Load<ItemDatabase>("Items/ItemDatabase");
            if (db != null)
            {
                var newPen = db.CreatePenInstance(penName);
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
            ItemDatabase db = Resources.Load<ItemDatabase>("Items/ItemDatabase");
            if (db != null)
            {
                var penData = db.GetRandomPenOfGrade(grade);
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
        }

        public void RemoveItem(ItemInstance item)
        {
            items.Remove(item);
            if (combatLoadout.Contains(item))
            {
                combatLoadout.Remove(item);
            }
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
