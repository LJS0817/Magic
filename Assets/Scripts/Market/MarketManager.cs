using UnityEngine;
using System.Collections.Generic;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance { get; private set; }
    
    public List<ItemQuestSO> activeQuests = new List<ItemQuestSO>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        GenerateDailyQuests(1);
    }

    private void Start()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDayChanged += GenerateDailyQuests;
        }
    }

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDayChanged -= GenerateDailyQuests;
        }
    }

    public void GenerateDailyQuests(int day)
    {
        activeQuests.Clear();
        var db = InventoryManager.Instance != null ? InventoryManager.Instance.itemDatabase : null;
        if (db == null || db.questTemplates.Count == 0 || db.materials.Count == 0) return;

        // 매일 3개의 랜덤 퀘스트 생성
        for(int i = 0; i < 3; i++)
        {
            var template = db.questTemplates[Random.Range(0, db.questTemplates.Count)];
            var targetItem = db.materials[Random.Range(0, db.materials.Count)];
            int amount = Random.Range(3, 15);
            long reward = (long)Mathf.Round(targetItem.basePriceInCopper * amount * Random.Range(1.5f, 3.0f));
            int deadline = Random.Range(2, 6);
            
            var dynamicQuest = template.CreateDynamicQuest(targetItem, amount, reward, deadline);
            activeQuests.Add(dynamicQuest);
        }
        
        Debug.Log($"<color=yellow>[MarketManager] {day}일차 새로운 길드 퀘스트 3개가 갱신되었습니다.</color>");
        
        // 갱신된 퀘스트를 UI에 반영하고 싶다면, MarketUIController의 Refresh 기능 호출이 필요할 수 있습니다.
    }


    /// <summary>
    /// 아이템의 기본 가격에 상점 할인율을 적용한 최종 1개당 단가를 반환합니다.
    /// </summary>
    public long GetItemPrice(ItemDataSO itemData)
    {
        if (itemData == null) return 0;

        long price = itemData.basePriceInCopper;
        if (price <= 0) price = 100; // UI(MarketItemSlotUI)의 가격 설정과 동일하게 맞춤
        
        // Apply ShopDiscount
        if (UpgradeManager.Instance != null)
        {
            float discountPercent = UpgradeManager.Instance.GetTotalUpgradeValue(UpgradeType.ShopDiscount);
            // Cap discount at 90%
            float discountMultiplier = 1f - Mathf.Clamp(discountPercent / 100f, 0f, 0.9f);
            price = (long)Mathf.Round(price * discountMultiplier);
        }

        return price;
    }

    /// <summary>
    /// 상점(마켓)에서 아이템 데이터를 기반으로 새 아이템을 지정한 수량만큼 구매합니다.
    /// </summary>
    public bool BuyItem(ItemDataSO itemData, int amount = 1)
    {
        if (itemData == null || amount <= 0) return false;

        long unitPrice = GetItemPrice(itemData);
        long totalPrice = unitPrice * amount;
        
        bool isRecipeBook = itemData.type == ItemType.RecipeBook;

        // Check Inventory Capacity first
        if (!isRecipeBook && InventoryManager.Instance != null && (InventoryManager.Instance.items.Count + amount) > InventoryManager.Instance.GetMaxCapacity())
        {
            Debug.LogWarning("[MarketManager] 인벤토리 여유 공간이 부족하여 아이템을 구매할 수 없습니다.");
            return false;
        }

        // 지갑에 돈이 충분한지 확인 후 차감 (자동 변환 결제 활성화)
        if (CurrencyManager.Instance.SpendCurrency(CurrencyType.Copper, totalPrice, autoConvert: true))
        {
            int successCount = 0;
            for (int i = 0; i < amount; i++)
            {
                if (isRecipeBook)
                {
                    if (itemData is ItemRecipeBookSO recipeBookSO)
                    {
                        if (recipeBookSO.isHintOnly)
                        {
                            PlayerDataManager.Instance.UnlockHint(recipeBookSO.targetSpellName);
                        }
                        else
                        {
                            PlayerDataManager.Instance.UnlockRecipe(recipeBookSO.targetSpellName);
                        }
                        successCount++;
                    }
                }
                else
                {
                    ItemInstance newItem = CreateInstanceFromData(itemData);
                    if (newItem != null)
                    {
                        InventoryManager.Instance.AddItem(newItem);
                        successCount++;
                    }
                }
            }

            if (successCount == amount)
            {
                Debug.Log($"<color=cyan>[Market] {itemData.itemName} {amount}개를 {totalPrice} 동화 가치에 구매했습니다.</color>");
                return true;
            }
            else
            {
                // 생성 실패 시 환불 처리
                long refund = unitPrice * (amount - successCount);
                if (refund > 0)
                {
                    CurrencyManager.Instance.AddCurrency(CurrencyType.Copper, refund);
                    Debug.LogError($"[MarketManager] 아이템 {amount - successCount}개 생성 실패로 환불되었습니다.");
                }
                return successCount > 0;
            }
        }
        else
        {
            Debug.LogWarning($"<color=red>[Market] 돈이 부족하여 {itemData.itemName}을(를) 살 수 없습니다. (필요: {totalPrice} 동화 가치)</color>");
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
            case ItemType.Quest:
                if (itemData is ItemQuestSO questSO) return new Item_Quest(questSO);
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

