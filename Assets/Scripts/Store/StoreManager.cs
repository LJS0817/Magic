using UnityEngine;
using System.Collections.Generic;
using System;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public Action<string> OnOrderCompleted;
    public Action<string> OnOrderFailed;

    public bool IsOrderContainerOpen { get; set; }
    public ItemInstance SelectedOrderItem { get; set; }

    [Header("Customer Orders")]
    public Dictionary<string, CustomerOrder> activeOrders = new Dictionary<string, CustomerOrder>();
    
    private string[] customerNames = { "마을 청년", "지친 용병", "마법 학도", "수상한 상인", "지나가는 행인" };

    [SerializeField] StoreUIController _controller;
    public bool IsOpened => _controller.IsOpened;

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            GenerateRandomOrder();
        }
    }

    public void GenerateRandomOrder()
    {
        var db = DrawingDatabase.Instance;
        if (db != null && db.recipes != null && db.recipes.Length > 0)
        {
            var randomRecipe = db.recipes[UnityEngine.Random.Range(0, db.recipes.Length)];
            string name = customerNames[UnityEngine.Random.Range(0, customerNames.Length)];
            
            // Element generation based on Max Mana
            SpellElement reqElement = SpellElement.None;
            string elementText = "무속성";
            
            float maxMana = PlayerDataManager.Instance != null ? PlayerDataManager.Instance.GetMaxMana() : 100f;
            if (maxMana >= 150f && UnityEngine.Random.value > 0.5f)
            {
                reqElement = (SpellElement)UnityEngine.Random.Range(1, 5); // 1~4 (Fire, Ice, Lightning, Earth)
                switch(reqElement) {
                    case SpellElement.Fire: elementText = "화염 속성"; break;
                    case SpellElement.Ice: elementText = "빙결 속성"; break;
                    case SpellElement.Lightning: elementText = "전격 속성"; break;
                    case SpellElement.Earth: elementText = "대지 속성"; break;
                }
            }
            string desc = $"최근 몬스터가 너무 많아서... 혹시 <color=#37a689>[{elementText}] {randomRecipe.SpellName}</color> 마법이 담긴 스크롤이 있습니까?";
            
            // Base reward calculation based on recipe mana cost and element
            long baseVal = (long)Mathf.Round(randomRecipe.manaCost * 5f);
            if (reqElement != SpellElement.None)
            {
                baseVal = (long)Mathf.Round(baseVal * 1.5f);
            }
            
            // Add Upgrade Multipliers to define the actual "Market Price"
            float marketMultiplier = 1f;
            if (UpgradeManager.Instance != null)
            {
                marketMultiplier += UpgradeManager.Instance.GetTotalUpgradeValue(UpgradeType.SellPriceMultiplier);
                if (randomRecipe.RequiredShapes != null)
                {
                    foreach (var shape in randomRecipe.RequiredShapes)
                    {
                        marketMultiplier += UpgradeManager.Instance.GetTotalTargetedUpgradeValue(UpgradeType.ShapeSellBonus, shape) / 100f;
                    }
                }
            }
            
            long marketPrice = (long)Mathf.Round(baseVal * marketMultiplier);
            
            // Generate Faction and Budget
            CustomerFaction faction = (CustomerFaction)UnityEngine.Random.Range(0, 5);
            float trueBudgetMultiplier = 1f;
            bool isBluffing = false;
            
            switch(faction)
            {
                case CustomerFaction.Peasant: trueBudgetMultiplier = UnityEngine.Random.Range(0.8f, 1.1f); isBluffing = UnityEngine.Random.value < 0.2f; break;
                case CustomerFaction.Mercenary: trueBudgetMultiplier = UnityEngine.Random.Range(0.9f, 1.3f); isBluffing = UnityEngine.Random.value < 0.8f; break; // 거짓말쟁이
                case CustomerFaction.Noble: trueBudgetMultiplier = UnityEngine.Random.Range(1.5f, 3.0f); isBluffing = UnityEngine.Random.value < 0.4f; break;
                case CustomerFaction.MageGuild: trueBudgetMultiplier = UnityEngine.Random.Range(1.2f, 2.0f); isBluffing = UnityEngine.Random.value < 0.1f; break; // 거의 거짓말 안함
                case CustomerFaction.Cultist: trueBudgetMultiplier = UnityEngine.Random.Range(1.0f, 1.5f); isBluffing = UnityEngine.Random.value < 0.5f; break;
            }

            long trueBudget = (long)Mathf.Round(marketPrice * trueBudgetMultiplier);
            long claimedBudget = trueBudget;

            if (isBluffing)
            {
                // 거짓말을 하면 진짜 예산보다 낮게 부름 (30% ~ 70% 수준)
                claimedBudget = (long)Mathf.Round(trueBudget * UnityEngine.Random.Range(0.3f, 0.7f));
            }

            var order = new CustomerOrder(name, desc, randomRecipe.SpellName, reqElement, marketPrice, trueBudget, claimedBudget, isBluffing, faction);
            activeOrders.Add(order.orderID, order);
            
            order.chatHistory.Add($"[{faction}] {name}: {desc} (예산: 약 {CurrencyManager.FormatCurrency(claimedBudget)} 생각합니다.)");

            Debug.Log($"<color=cyan>[Store] 새 주문 도착! ID:{order.orderID} {name} - {elementText} {randomRecipe.SpellName} (Market: {marketPrice}, TrueBudget: {trueBudget}, Claimed: {claimedBudget}, Bluff: {isBluffing})</color>");
            
            _controller.SpawnOrder(order);
        }
        else
        {
            Debug.LogWarning("[StoreManager] DrawingDatabase에 스펠 레시피가 없어 주문을 생성할 수 없습니다.");
        }
    }

    /// <summary>
    /// 손님과 가격을 흥정합니다.
    /// </summary>
    public bool HaggleOrder(string orderId, long askingPrice)
    {
        if (!activeOrders.TryGetValue(orderId, out var order)) return false;
        
        if (order.state != OrderState.Received && order.state != OrderState.Haggling) return false;

        order.chatHistory.Add($"플레이어: {CurrencyManager.FormatCurrency(askingPrice)}");

        if (askingPrice <= order.trueBudget)
        {
            order.agreedPrice = askingPrice;
            order.state = OrderState.Pending;
            order.chatHistory.Add($"손님: 좋습니다. 스크롤을 전송해 주십시오.\n<color=#fd7225>*주의* 다른 속성이나 마법을 배송하면 사기로 간주되어 거래액의 두 배인 {CurrencyManager.FormatCurrency(askingPrice * 2)}의 위약금을 뭅니다.\n우측 인벤토리에서 스크롤을 더블 클릭하여 배송해주세요.</color>");
            Debug.Log($"[Store] 주문 {orderId} 흥정 성공! 타결 가격: {askingPrice}");
            return true;
        }
        else
        {
            order.patience -= 34f; // 100 -> 66 -> 32 -> -2 (Exactly 3 haggles)
            order.chatHistory.Add($"손님: 너무 비쌉니다. 깎아주지 않으면 거래하지 않겠습니다.");
            order.state = OrderState.Haggling;

            if (order.patience <= 0)
            {
                order.state = OrderState.Failed;
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
        
        if (order.state != OrderState.Pending)
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
                long penalty = order.agreedPrice * 2;
                Debug.Log($"<color=red>[Store] 사기 적발! 위약금 {penalty} 차감 및 아이템 파괴됨.</color>");
                
                CurrencyManager.Instance.SpendCurrency(CurrencyType.Copper, penalty, autoConvert: true, allowNegative: true);
                CurrencyManager.Instance.CompressCurrency();
                
                InventoryManager.Instance.RemoveItem(item); // 스크롤 소각

                order.state = OrderState.Failed;
                order.chatHistory.Add($"[시스템] 계약 위반! 마법사 길드에 의해 위약금 {CurrencyManager.FormatCurrency(penalty)}가 압류되고 스크롤은 소각되었습니다.");
                
                activeOrders.Remove(orderId);
                return false;
            }

            // A. 초과 달성 및 B. 부분 성공 판정
            bool isOverachievement = scroll.ScrollData.accuracyScore >= 1.2f; 
            bool isPartialSuccess = scroll.ScrollData.accuracyScore < 1.0f;

            long finalPayment = order.agreedPrice;

            if (isOverachievement)
            {
                long maxTip = order.trueBudget - order.agreedPrice;
                long actualTip = (long)Mathf.Round(order.agreedPrice * 0.2f); // 20% 팁
                if (actualTip > maxTip) actualTip = maxTip;
                
                finalPayment += actualTip;
                order.chatHistory.Add($"손님: 기대 이상의 퀄리티군요! 팁을 {CurrencyManager.FormatCurrency(actualTip)} 더 얹어 드리겠습니다.");
            }
            else if (isPartialSuccess)
            {
                long discount = (long)Mathf.Round(order.agreedPrice * 0.3f); // 30% 강제 할인
                finalPayment -= discount;
                order.chatHistory.Add($"손님: 품질이 생각보다 별로네요. {CurrencyManager.FormatCurrency(discount)}만큼은 빼고 드리겠습니다.");
            }
            else
            {
                order.chatHistory.Add($"손님: 거래 감사합니다.");
            }

            InventoryManager.Instance.RemoveItem(item);
            CurrencyManager.Instance.AddCurrency(CurrencyType.Copper, finalPayment);
            CurrencyManager.Instance.CompressCurrency();

            order.state = OrderState.Completed;
            activeOrders.Remove(orderId);

            Debug.Log($"<color=green>[Store] 거래 성사! 최종 입금액: {finalPayment} 동화</color>");
            GenerateRandomOrder(); // 새로운 주문 보충
            return true;
        }
        
        return false;
    }


    public bool SelectItemForOrder(ItemInstance item)
    {
        if (_controller != null)
        {
            return _controller.SelectItem(item);
        }
        return false;
    }
}

