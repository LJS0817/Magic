using UnityEngine;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    /// <summary>
    /// 상점(마켓)에서 아이템 데이터를 기반으로 새 아이템을 구매합니다.
    /// </summary>
    public bool BuyItem(ItemDataSO itemData)
    {
        if (itemData == null) return false;

        int price = itemData.basePriceInCopper;
        
        // Apply ShopDiscount
        if (UpgradeManager.Instance != null)
        {
            float discountPercent = UpgradeManager.Instance.GetTotalUpgradeValue(UpgradeType.ShopDiscount);
            // Cap discount at 90%
            float discountMultiplier = 1f - Mathf.Clamp(discountPercent / 100f, 0f, 0.9f);
            price = Mathf.RoundToInt(price * discountMultiplier);
        }
        
        // Check Inventory Capacity first
        if (InventoryManager.Instance != null && InventoryManager.Instance.items.Count >= InventoryManager.Instance.GetMaxCapacity())
        {
            Debug.LogWarning("[MarketManager] 인벤토리가 가득 차서 아이템을 구매할 수 없습니다.");
            return false;
        }

        // 지갑에 돈이 충분한지 확인 후 차감 (자동 변환 결제 활성화)
        if (CurrencyManager.Instance.SpendCurrency(CurrencyType.Copper, price, autoConvert: true))
        {
            ItemInstance newItem = CreateInstanceFromData(itemData);
            if (newItem != null)
            {
                InventoryManager.Instance.AddItem(newItem);
                Debug.Log($"<color=cyan>[Market] {itemData.itemName}을(를) {price} 동화 가치에 구매했습니다.</color>");
                return true;
            }
            else
            {
                // 생성 실패 시 환불 처리
                CurrencyManager.Instance.AddCurrency(CurrencyType.Copper, price);
                Debug.LogError("[MarketManager] 아이템 생성 실패로 환불되었습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"<color=red>[Market] 돈이 부족하여 {itemData.itemName}을(를) 살 수 없습니다. (필요: {price} 동화 가치)</color>");
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
            case ItemType.Wand:
                return new Item_Wand(itemData as ItemWandSO);
            case ItemType.Potion:
                return new Item_Potion(itemData as ItemPotionSO);
            case ItemType.Pouch:
                return new Item_Pouch(itemData as ItemPouchSO);
            case ItemType.Material:
                return new Item_Material(itemData);
            default:
                return new Item_Material(itemData); // 기본 폴백
        }
    }
}

