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
        public System.Collections.Generic.Dictionary<string, Magic.Store.CustomerOrder> activeOrders = new System.Collections.Generic.Dictionary<string, Magic.Store.CustomerOrder>();

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
                
                // Generate Faction and Budget
                Magic.Store.CustomerFaction faction = (Magic.Store.CustomerFaction)UnityEngine.Random.Range(0, 5);
                float trueBudgetMultiplier = 1f;
                bool isBluffing = false;
                
                switch(faction)
                {
                    case Magic.Store.CustomerFaction.Peasant: trueBudgetMultiplier = UnityEngine.Random.Range(0.8f, 1.1f); isBluffing = UnityEngine.Random.value < 0.2f; break;
                    case Magic.Store.CustomerFaction.Mercenary: trueBudgetMultiplier = UnityEngine.Random.Range(0.9f, 1.3f); isBluffing = UnityEngine.Random.value < 0.8f; break; // 거짓말쟁이
                    case Magic.Store.CustomerFaction.Noble: trueBudgetMultiplier = UnityEngine.Random.Range(1.5f, 3.0f); isBluffing = UnityEngine.Random.value < 0.4f; break;
                    case Magic.Store.CustomerFaction.MageGuild: trueBudgetMultiplier = UnityEngine.Random.Range(1.2f, 2.0f); isBluffing = UnityEngine.Random.value < 0.1f; break; // 거의 거짓말 안함
                    case Magic.Store.CustomerFaction.Cultist: trueBudgetMultiplier = UnityEngine.Random.Range(1.0f, 1.5f); isBluffing = UnityEngine.Random.value < 0.5f; break;
                }

                int trueBudget = Mathf.RoundToInt(marketPrice * trueBudgetMultiplier);
                int claimedBudget = trueBudget;

                if (isBluffing)
                {
                    // 거짓말을 하면 진짜 예산보다 낮게 부름 (30% ~ 70% 수준)
                    claimedBudget = Mathf.RoundToInt(trueBudget * UnityEngine.Random.Range(0.3f, 0.7f));
                }

                var order = new Magic.Store.CustomerOrder(name, desc, randomRecipe.SpellName, reqElement, marketPrice, trueBudget, claimedBudget, isBluffing, faction);
                activeOrders.Add(order.orderID, order);
                
                order.chatHistory.Add($"[{faction}] {name}: {desc} (예산: 약 {claimedBudget} 동화 생각합니다.)");

                Debug.Log($"<color=cyan>[Store] 새 주문 도착! ID:{order.orderID} {name} - {elementText} {randomRecipe.SpellName} (Market: {marketPrice}, TrueBudget: {trueBudget}, Claimed: {claimedBudget}, Bluff: {isBluffing})</color>");
            }
            else
            {
                Debug.LogWarning("[StoreManager] DrawingDatabase에 스펠 레시피가 없어 주문을 생성할 수 없습니다.");
            }
        }

        /// <summary>
        /// 손님과 가격을 흥정합니다.
        /// </summary>
        public bool HaggleOrder(string orderId, int askingPrice)
        {
            if (!activeOrders.TryGetValue(orderId, out var order)) return false;
            
            if (order.state != Magic.Store.OrderState.Received && order.state != Magic.Store.OrderState.Haggling) return false;

            order.chatHistory.Add($"플레이어: {askingPrice} 동화에 드리겠습니다.");

            if (askingPrice <= order.trueBudget)
            {
                order.agreedPrice = askingPrice;
                order.state = Magic.Store.OrderState.Pending;
                order.chatHistory.Add($"손님: 좋습니다. 스크롤을 전송해 주십시오.");
                Debug.Log($"[Store] 주문 {orderId} 흥정 성공! 타결 가격: {askingPrice}");
                return true;
            }
            else
            {
                order.patience -= 30f;
                order.chatHistory.Add($"손님: 너무 비쌉니다. 깎아주지 않으면 거래하지 않겠습니다.");
                order.state = Magic.Store.OrderState.Haggling;

                if (order.patience <= 0)
                {
                    order.state = Magic.Store.OrderState.Failed;
                    order.chatHistory.Add($"손님: 인내심의 한계입니다. 거래를 취소하겠습니다!");
                    activeOrders.Remove(orderId);
                    Debug.Log($"[Store] 손님이 화를 내며 나갔습니다. (주문 삭제됨)");
                }
                return false;
            }
        }

        /// <summary>
        /// 손님의 주문에 맞는 아이템을 배송하고 에스크로 판정을 진행합니다.
        /// </summary>
        public bool DeliverOrder(string orderId, ItemInstance item)
        {
            if (!activeOrders.TryGetValue(orderId, out var order)) return false;
            
            if (order.state != Magic.Store.OrderState.Pending)
            {
                Debug.LogWarning("[StoreManager] 가격이 아직 타결되지 않았거나 거래가 종료된 주문입니다.");
                return false;
            }

            if (item == null || item.data == null)
            {
                return false;
            }

            if (item is Item_Scroll scroll && scroll.ScrollData != null)
            {
                // C. 단순 실패 및 부정행위 판정
                if (scroll.ScrollData.spellName != order.requestedSpellName || scroll.ScrollData.scrollElement != order.requestedElement)
                {
                    int penalty = order.agreedPrice * 2;
                    Debug.Log($"<color=red>[Store] 사기 적발! 위약금 {penalty} 차감 및 아이템 파괴됨.</color>");
                    
                    CurrencyManager.Instance.SpendCurrency(CurrencyType.Copper, penalty, autoConvert: true, allowNegative: true);
                    CurrencyManager.Instance.CompressCurrency();
                    
                    InventoryManager.Instance.RemoveItem(item); // 스크롤 소각

                    order.state = Magic.Store.OrderState.Failed;
                    order.chatHistory.Add($"[시스템] 계약 위반! 마법사 길드에 의해 위약금 {penalty} 동화가 압류되고 스크롤은 소각되었습니다.");
                    
                    activeOrders.Remove(orderId);
                    return false;
                }

                // A. 초과 달성 및 B. 부분 성공 판정
                bool isOverachievement = scroll.ScrollData.accuracyScore >= 1.2f; 
                bool isPartialSuccess = scroll.ScrollData.accuracyScore < 1.0f;

                int finalPayment = order.agreedPrice;

                if (isOverachievement)
                {
                    int maxTip = order.trueBudget - order.agreedPrice;
                    int actualTip = Mathf.RoundToInt(order.agreedPrice * 0.2f); // 20% 팁
                    if (actualTip > maxTip) actualTip = maxTip;
                    
                    finalPayment += actualTip;
                    order.chatHistory.Add($"손님: 기대 이상의 퀄리티군요! 팁을 {actualTip} 동화 더 얹어 드리겠습니다.");
                }
                else if (isPartialSuccess)
                {
                    int discount = Mathf.RoundToInt(order.agreedPrice * 0.3f); // 30% 강제 할인
                    finalPayment -= discount;
                    order.chatHistory.Add($"손님: 품질이 생각보다 별로네요. {discount} 동화만큼은 빼고 드리겠습니다.");
                }
                else
                {
                    order.chatHistory.Add($"손님: 거래 감사합니다.");
                }

                InventoryManager.Instance.RemoveItem(item);
                CurrencyManager.Instance.AddCurrency(CurrencyType.Copper, finalPayment);
                CurrencyManager.Instance.CompressCurrency();

                order.state = Magic.Store.OrderState.Completed;
                activeOrders.Remove(orderId);

                Debug.Log($"<color=green>[Store] 거래 성사! 최종 입금액: {finalPayment} 동화</color>");
                GenerateRandomOrder(); // 새로운 주문 보충
                return true;
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
