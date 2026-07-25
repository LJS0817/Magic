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
        if (price <= 0) price = 100; // UI(MarketItemSlotUI)의 가격 설정과 동일하게 맞춤
        
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
                return true;z`
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
        if (itemData == null) return null;

        switch (itemData.type)
        {
            case ItemType.Scroll:
                if (itemData is ItemScrollSO scrollSO) return new Item_Scroll(scrollSO);
                break;
            case ItemType.Ink:
                if (itemData is ItemInkSO inkSO) return new Item_Ink(inkSO);
                break;
            case ItemType.Pen:
                if (itemData is ItemPenSO penSO) return new Item_Pen(penSO);
                break;
            case ItemType.Wand:
                if (itemData is ItemWandSO wandSO) return new Item_Wand(wandSO);
                break;
            case ItemType.Potion:
                if (itemData is ItemPotionSO potionSO) return new Item_Potion(potionSO);
                break;
            case ItemType.Pouch:
                if (itemData is ItemPouchSO pouchSO) return new Item_Pouch(pouchSO);
                break;
            case ItemType.Material:
                return new Item_Material(itemData);
            default:
                return new Item_Material(itemData);
        }

        Debug.LogError($"[MarketManager] '{itemData.itemName}'의 ItemType은 {itemData.type}로 설정되어 있지만, ScriptableObject 파일이 해당 SO 자식 클래스로 생성되지 않았습니다! 임시로 Item_Material로 생성합니다.");
        return new Item_Material(itemData);
    }
}

