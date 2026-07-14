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

        [Header("Customer Orders")]
        public Magic.Store.CustomerOrder currentOrder;

        private string[] customerNames = { "마을 청년", "지친 용병", "마법 학도", "수상한 상인", "지나가는 행인" };

        private void Start()
        {
            GenerateRandomOrder();
        }

        public void GenerateRandomOrder()
        {
            var db = Magic.Drawing.DrawingDatabase.Instance;
            if (db != null && db.recipes != null && db.recipes.Length > 0)
            {
                var randomRecipe = db.recipes[UnityEngine.Random.Range(0, db.recipes.Length)];
                string name = customerNames[UnityEngine.Random.Range(0, customerNames.Length)];
                
                // Element generation based on Max Mana
                Magic.Combat.SpellElement reqElement = Magic.Combat.SpellElement.None;
                string elementText = "무속성";
                
                float maxMana = Magic.Data.PlayerDataManager.Instance != null ? Magic.Data.PlayerDataManager.Instance.GetMaxMana() : 100f;
                if (maxMana >= 150f && UnityEngine.Random.value > 0.5f)
                {
                    reqElement = (Magic.Combat.SpellElement)UnityEngine.Random.Range(1, 5); // 1~4 (Fire, Ice, Lightning, Earth)
                    switch(reqElement) {
                        case Magic.Combat.SpellElement.Fire: elementText = "화염 속성"; break;
                        case Magic.Combat.SpellElement.Ice: elementText = "빙결 속성"; break;
                        case Magic.Combat.SpellElement.Lightning: elementText = "전격 속성"; break;
                        case Magic.Combat.SpellElement.Earth: elementText = "대지 속성"; break;
                    }
                }
                
                string desc = $"최근 몬스터가 너무 많아서... 혹시 <color=cyan>[{elementText}] {randomRecipe.SpellName}</color> 마법이 담긴 스크롤이 있습니까?";
                
                // Base reward calculation based on recipe mana cost and element
                int baseVal = Mathf.RoundToInt(randomRecipe.manaCost * 5f);
                if (reqElement != Magic.Combat.SpellElement.None)
                {
                    baseVal = Mathf.RoundToInt(baseVal * 1.5f);
                }
                
                // Add Upgrade Multipliers to define the actual "Market Price"
                float marketMultiplier = 1f;
                if (Magic.Upgrade.UpgradeManager.Instance != null)
                {
                    marketMultiplier += Magic.Upgrade.UpgradeManager.Instance.GetTotalUpgradeValue(Magic.Upgrade.UpgradeType.SellPriceMultiplier);
                    if (randomRecipe.RequiredShapes != null)
                    {
                        foreach (var shape in randomRecipe.RequiredShapes)
                        {
                            marketMultiplier += Magic.Upgrade.UpgradeManager.Instance.GetTotalTargetedUpgradeValue(Magic.Upgrade.UpgradeType.ShapeSellBonus, shape) / 100f;
                        }
                    }
                }
                
                int marketPrice = Mathf.RoundToInt(baseVal * marketMultiplier);
                
                // Generate Customer Budget (-3% to +5%)
                float budgetMultiplier = UnityEngine.Random.Range(0.97f, 1.05f);
                int budget = Mathf.RoundToInt(marketPrice * budgetMultiplier);

                currentOrder = new Magic.Store.CustomerOrder(name, desc, randomRecipe.SpellName, reqElement, marketPrice, budget);
                Debug.Log($"<color=cyan>[Store] 새 주문 도착! {name} - {elementText} {randomRecipe.SpellName} (Market: {marketPrice}, Budget: {budget})</color>");
            }
            else
            {
                Debug.LogWarning("[StoreManager] DrawingDatabase에 스펠 레시피가 없어 주문을 생성할 수 없습니다.");
            }
        }

        /// <summary>
        /// 손님의 주문에 맞는 아이템을 제시된 가격(askingPrice)에 판매를 시도합니다.
        /// </summary>
        public bool FulfillOrder(ItemInstance item, int askingPrice)
        {
            if (currentOrder == null)
            {
                Debug.LogWarning("[StoreManager] 현재 대기 중인 주문이 없습니다.");
                return false;
            }

            if (item == null || item.data == null)
            {
                return false;
            }

            // Check if it's a scroll and matches the requested spell and element
            if (item is Item_Scroll scroll && scroll.ScrollData != null)
            {
                if (scroll.ScrollData.spellName != currentOrder.requestedSpellName || scroll.ScrollData.scrollElement != currentOrder.requestedElement)
                {
                    Debug.Log($"<color=red>[Store] 손님이 원하던 물건이 아닙니다! (요구: {currentOrder.requestedElement} {currentOrder.requestedSpellName}, 제출: {scroll.ScrollData.scrollElement} {scroll.ScrollData.spellName})</color>");
                    return false;
                }

                // Check Budget
                if (askingPrice <= currentOrder.customerBudget)
                {
                    InventoryManager.Instance.RemoveItem(item);
                    CurrencyManager.Instance.AddCurrency(CurrencyType.Copper, askingPrice);
                    CurrencyManager.Instance.CompressCurrency();

                    Debug.Log($"<color=green>[Store] 거래 성사! {currentOrder.customerName}에게 {item.ItemName}을(를) {askingPrice} 동화에 판매했습니다.</color>");
                    
                    GenerateRandomOrder();
                    return true;
                }
                else
                {
                    Debug.Log($"<color=orange>[Store] 너무 비쌉니다! (손님 예산 초과)</color>");
                    // Reduce budget as penalty
                    currentOrder.customerBudget = Mathf.RoundToInt(currentOrder.customerBudget * 0.95f);
                    Debug.Log($"<color=gray>[Store] 손님의 예산이 깎였습니다. 현재 예산 추정치: 약 {currentOrder.customerBudget}</color>");
                    return false;
                }
            }
            
            return false;
        }

        /// <summary>
        /// 상점에서 아이템 데이터를 기반으로 새 아이템을 구매합니다.
        /// </summary>
        public bool BuyItem(ItemDataSO itemData)
        {
            if (itemData == null) return false;

            int price = itemData.basePriceInCopper;
            
            // Apply ShopDiscount
            if (Magic.Upgrade.UpgradeManager.Instance != null)
            {
                float discountPercent = Magic.Upgrade.UpgradeManager.Instance.GetTotalUpgradeValue(Magic.Upgrade.UpgradeType.ShopDiscount);
                // Cap discount at 90%
                float discountMultiplier = 1f - Mathf.Clamp(discountPercent / 100f, 0f, 0.9f);
                price = Mathf.RoundToInt(price * discountMultiplier);
            }
            
            // Check Inventory Capacity first
            if (InventoryManager.Instance != null && InventoryManager.Instance.items.Count >= InventoryManager.Instance.GetMaxCapacity())
            {
                Debug.LogWarning("[StoreManager] 인벤토리가 가득 차서 아이템을 구매할 수 없습니다.");
                return false;
            }

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
