using UnityEngine;

namespace Magic.Inventory
{
    public class StoreManager : MonoBehaviour
    {
        public static StoreManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 인벤토리의 아이템을 상점에 판매하여 동화(Copper)로 교환합니다.
        /// </summary>
        public bool SellItem(ItemInstance item)
        {
            if (item == null || item.data == null)
            {
                Debug.LogWarning("[StoreManager] 판매할 아이템 데이터가 없습니다.");
                return false;
            }

            int price = item.data.basePriceInCopper;
            if (price <= 0)
            {
                Debug.LogWarning($"[StoreManager] {item.ItemName}은(는) 상점에 팔 수 없습니다 (가격이 0 이하).");
                return false;
            }

            // 인벤토리에서 제거
            InventoryManager.Instance.RemoveItem(item);

            // 돈 지급
            CurrencyManager.Instance.AddCurrency(CurrencyType.Copper, price);
            
            // 지갑 자동 정리(동화 -> 은화 등)를 호출하여 깔끔하게 만듭니다.
            CurrencyManager.Instance.CompressCurrency();

            Debug.Log($"<color=cyan>[Store] {item.ItemName}을(를) {price} 동화에 판매했습니다.</color>");
            return true;
        }

        /// <summary>
        /// 상점에서 아이템 데이터를 기반으로 새 아이템을 구매합니다.
        /// </summary>
        public bool BuyItem(ItemDataSO itemData)
        {
            if (itemData == null) return false;

            int price = itemData.basePriceInCopper;
            
            // 지갑에 돈이 충분한지 확인 후 차감 (자동 변환 결제 활성화)
            if (CurrencyManager.Instance.SpendCurrency(CurrencyType.Copper, price, autoConvert: true))
            {
                ItemInstance newItem = CreateInstanceFromData(itemData);
                if (newItem != null)
                {
                    InventoryManager.Instance.AddItem(newItem);
                    Debug.Log($"<color=cyan>[Store] {itemData.itemName}을(를) {price} 동화 가치에 구매했습니다.</color>");
                    return true;
                }
                else
                {
                    // 생성 실패 시 환불 처리
                    CurrencyManager.Instance.AddCurrency(CurrencyType.Copper, price);
                    Debug.LogError("[StoreManager] 아이템 생성 실패로 환불되었습니다.");
                }
            }
            else
            {
                Debug.LogWarning($"<color=red>[Store] 돈이 부족하여 {itemData.itemName}을(를) 살 수 없습니다. (필요: {price} 동화 가치)</color>");
            }

            return false;
        }

        /// <summary>
        /// ItemDataSO를 기반으로 올바른 ItemInstance 자식 클래스를 생성해주는 팩토리 헬퍼
        /// </summary>
        private ItemInstance CreateInstanceFromData(ItemDataSO itemData)
        {
            switch (itemData.type)
            {
                case ItemType.Scroll:
                    return new Item_Scroll(itemData as ItemScrollSO);
                case ItemType.Ink:
                    return new Item_Ink(itemData as ItemInkSO);
                case ItemType.Pen:
                    return new Item_Pen(itemData as ItemPenSO);
                case ItemType.Material:
                    return new Item_Material(itemData);
                default:
                    return new Item_Material(itemData); // 기본 폴백
            }
        }
    }
}
