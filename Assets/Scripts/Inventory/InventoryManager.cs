using UnityEngine;
using System.Collections.Generic;
using Magic.Inventory;

namespace Magic.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Inventory Data")]
        public List<ItemData> items = new List<ItemData>();

        [Header("Combat Data")]
        public List<ItemData> combatLoadout = new List<ItemData>();

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
                var scroll1 = new Item_Scroll(true, "", 5);
                var scroll2 = new Item_Scroll(true, "", 5);
                var impactScroll = new Item_Scroll(false, "Impact", 5);
                var normalInk = new Item_Ink(100f, "일반");
                var highInk = new Item_Ink(100f, "고급");

                items.Add(scroll1);      // 빈 스크롤
                items.Add(scroll2);      // 빈 스크롤
                items.Add(impactScroll); // 임팩트 스크롤
                items.Add(normalInk);        // 잉크 1병
                items.Add(highInk);        // 잉크 2병

                // 기본 전투 장비로 세팅 (테스트용)
                combatLoadout.Add(scroll1);
                combatLoadout.Add(impactScroll);
                combatLoadout.Add(normalInk);

                Debug.Log("[InventoryManager] 기본 테스트 아이템 및 로드아웃 지급 완료.");
            }
        }

        public void AddToLoadout(ItemData item)
        {
            if (!combatLoadout.Contains(item) && items.Contains(item))
            {
                combatLoadout.Add(item);
            }
        }

        public void RemoveFromLoadout(ItemData item)
        {
            combatLoadout.Remove(item);
        }

        public void ClearLoadout()
        {
            combatLoadout.Clear();
        }

        public void AddItem(ItemData item)
        {
            items.Add(item);
        }

        public void RemoveItem(ItemData item)
        {
            items.Remove(item);
            if (combatLoadout.Contains(item))
            {
                combatLoadout.Remove(item);
            }
        }

        public List<T> GetItemsOfType<T>() where T : ItemData
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
