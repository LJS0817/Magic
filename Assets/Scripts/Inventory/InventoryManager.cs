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
                items.Add(new Item_Scroll(true, "", 5));      // 빈 스크롤
                items.Add(new Item_Scroll(true, "", 5));      // 빈 스크롤
                items.Add(new Item_Scroll(false, "Meteor", 5)); // 메테오 스크롤
                items.Add(new Item_Ink(100f, "일반"));        // 잉크 1병
                items.Add(new Item_Ink(100f, "고급"));        // 잉크 2병
                Debug.Log("[InventoryManager] 기본 테스트 아이템 지급 완료.");
            }
        }

        public void AddItem(ItemData item)
        {
            items.Add(item);
        }

        public void RemoveItem(ItemData item)
        {
            items.Remove(item);
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
