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
    private Dictionary<string, CustomerOrder> _fallbackOrders = new Dictionary<string, CustomerOrder>();
    public Dictionary<string, CustomerOrder> activeOrders => 
        PlayerDataManager.Instance != null ? PlayerDataManager.Instance.activeOrders : _fallbackOrders;

    [SerializeField] private int _poolingCount = 8;
    public int PoolingCount => _poolingCount;
    
    [SerializeField] StoreUIController _controller;
    public bool IsOpened => _controller.IsOpened;

    private void Start()
    {

        
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDayChanged += HandleDayChanged;
            // PlayerDataManager.Instance.OnSatisfactionChanged += UpdateSatisfactionUI;


            if (!PlayerDataManager.Instance.hasGeneratedInitialOrders)
            {
                PlayerDataManager.Instance.hasGeneratedInitialOrders = true;
                GenerateRandomOrders();
            }
            else
            {
                // 씬을 전환하고 돌아온 경우: 이미 생성되어 유지 중인 주문 목록을 UI로 복원
                Debug.Log($"[StoreManager] 씬 전환 전 유지되던 {activeOrders.Count}개의 주문을 복원합니다.");
                foreach (var order in activeOrders.Values)
                {
                    if (_controller != null)
                    {
                        _controller.SpawnOrder(order);
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDayChanged -= HandleDayChanged;
            // PlayerDataManager.Instance.OnSatisfactionChanged -= UpdateSatisfactionUI;
        }
    }


    private void HandleDayChanged(int newDay)
    {
        List<string> expiredOrderIds = new List<string>();

        foreach (var kvp in activeOrders)
        {
            var order = kvp.Value;
            if (order.state == OrderState.Received || order.state == OrderState.Haggling || order.state == OrderState.Pending)
            {
                order.deadlineDaysLeft--;

                if (order.deadlineDaysLeft <= 0)
                {
                    expiredOrderIds.Add(kvp.Key);
                }
                else
                {
                    // Update UI if the order is still active
                    order.OnStateChanged?.Invoke(order.state);
                }
            }
        }

        foreach (var id in expiredOrderIds)
        {
            if (activeOrders.TryGetValue(id, out var expiredOrder))
            {
                expiredOrder.state = OrderState.Failed;
                expiredOrder.chatHistory.Add($"[시스템] 주문 기한이 만료되었습니다. 상점 만족도가 하락합니다.");
                activeOrders.Remove(id);
                Debug.Log($"<color=red>[Store] 주문 {id} 기한 만료!</color>");
                PlayerDataManager.Instance.ModifySatisfaction(-5f); // 만료 시 페널티
            }
        }

    }

    public void GenerateRandomOrders()
    {
        int orderCount = PlayerDataManager.Instance.GetOrderCount(_poolingCount);

        for (int i = 0; i < orderCount; i++)
        {
            GenerateOrder();
        }
    }

    public void GenerateOrder()
    {
        var db = DrawingDatabase.Instance;
        if (db != null && db.recipes != null && db.recipes.Length > 0)
        {
            List<SpellRecipeAsset> availableRecipes = null;
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.unlockedRecipeObjects.Count > 0)
            {
                availableRecipes = PlayerDataManager.Instance.unlockedRecipeObjects;
            }

            // 만약 해금된 마법이 전혀 없다면, 임시로 전체 레시피 중 하나를 선택하도록 폴백 처리
            if (availableRecipes == null || availableRecipes.Count == 0)
            {
                availableRecipes = new List<SpellRecipeAsset>(db.recipes);
            }

            var randomRecipe = availableRecipes[UnityEngine.Random.Range(0, availableRecipes.Count)];
            
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
            
            // Add Guild Rank Premium
            if (PlayerDataManager.Instance != null)
            {
                switch (PlayerDataManager.Instance.CurrentGuildRank)
                {
                    case GuildRank.Silver: marketMultiplier += 0.05f; break;
                    case GuildRank.Gold: marketMultiplier += 0.10f; break;
                    case GuildRank.Mithril: marketMultiplier += 0.20f; break;
                    case GuildRank.Orichalcum: marketMultiplier += 0.30f; break;
                }
            }
            
            long marketPrice = (long)Mathf.Round(baseVal * marketMultiplier);
            
            // Generate Faction and Budget
            float factionRoll = UnityEngine.Random.value;
            CustomerFaction faction = CustomerFaction.Peasant;
            
            if (PlayerDataManager.Instance != null)
            {
                GuildRank rank = PlayerDataManager.Instance.CurrentGuildRank;
                // Higher ranks have higher chance for MageGuild and Noble, lower chance for Peasant/Mercenary
                if (rank >= GuildRank.Gold)
                {
                    if (factionRoll < 0.1f) faction = CustomerFaction.Peasant;
                    else if (factionRoll < 0.2f) faction = CustomerFaction.Mercenary;
                    else if (factionRoll < 0.5f) faction = CustomerFaction.Noble;
                    else if (factionRoll < 0.8f) faction = CustomerFaction.MageGuild;
                    else faction = CustomerFaction.Cultist;
                }
                else if (rank >= GuildRank.Silver)
                {
                    if (factionRoll < 0.2f) faction = CustomerFaction.Peasant;
                    else if (factionRoll < 0.5f) faction = CustomerFaction.Mercenary;
                    else if (factionRoll < 0.7f) faction = CustomerFaction.Noble;
                    else if (factionRoll < 0.9f) faction = CustomerFaction.MageGuild;
                    else faction = CustomerFaction.Cultist;
                }
                else
                {
                    if (factionRoll < 0.4f) faction = CustomerFaction.Peasant;
                    else if (factionRoll < 0.7f) faction = CustomerFaction.Mercenary;
                    else if (factionRoll < 0.8f) faction = CustomerFaction.Noble;
                    else if (factionRoll < 0.9f) faction = CustomerFaction.MageGuild;
                    else faction = CustomerFaction.Cultist;
                }
            }
            else
            {
                faction = (CustomerFaction)UnityEngine.Random.Range(0, 5);
            }

            float trueBudgetMultiplier = 1f;
            bool isBluffing = false;
            
            switch(faction)
            {
                case CustomerFaction.Peasant: 
                    trueBudgetMultiplier = UnityEngine.Random.Range(0.7f, 0.9f); 
                    isBluffing = UnityEngine.Random.value < 0.2f; 
                    break;
                case CustomerFaction.Mercenary: 
                    trueBudgetMultiplier = UnityEngine.Random.Range(0.8f, 1.0f); 
                    isBluffing = UnityEngine.Random.value < 0.6f; // 용병은 자주 후려침
                    break;
                case CustomerFaction.Noble: 
                    trueBudgetMultiplier = 1.0f; // 귀족은 정가 선호
                    isBluffing = UnityEngine.Random.value < 0.1f; 
                    break;
                case CustomerFaction.MageGuild: 
                    trueBudgetMultiplier = 1.0f; // 마법사 길드는 시장가를 정확히 앎
                    isBluffing = false; 
                    break;
                case CustomerFaction.Cultist: 
                    trueBudgetMultiplier = UnityEngine.Random.Range(0.9f, 1.0f); 
                    isBluffing = UnityEngine.Random.value < 0.3f; 
                    break;
            }

            // Reduce bluff chance based on high rank
            if (PlayerDataManager.Instance != null && isBluffing)
            {
                GuildRank rank = PlayerDataManager.Instance.CurrentGuildRank;
                if (rank == GuildRank.Mithril && UnityEngine.Random.value < 0.5f) isBluffing = false;
                if (rank == GuildRank.Orichalcum && UnityEngine.Random.value < 0.8f) isBluffing = false;
            }

            long trueBudget = (long)Mathf.Round(marketPrice * trueBudgetMultiplier);
            long claimedBudget = trueBudget;

            if (isBluffing)
            {
                // 거짓말을 하면 진짜 예산보다 낮게 부름 (50% ~ 80% 수준)
                claimedBudget = (long)Mathf.Round(trueBudget * UnityEngine.Random.Range(0.5f, 0.8f));
            }

            var order = new CustomerOrder(desc, randomRecipe.SpellName, reqElement, marketPrice, trueBudget, claimedBudget, isBluffing, faction);
            activeOrders.Add(order.orderID, order);
            
            order.chatHistory.Add($"[{faction}] {desc} (제시 가격: {CurrencyManager.FormatCurrency(claimedBudget)})");

            Debug.Log($"<color=cyan>[Store] 새 주문 도착! ID:{order.orderID} {elementText} {randomRecipe.SpellName} (Market: {marketPrice}, TrueBudget: {trueBudget}, Claimed: {claimedBudget}, Bluff: {isBluffing})</color>");
            
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

            // 맞게 판매하기만 하면 제값(agreedPrice)을 줌
            long finalPayment = order.agreedPrice;
            order.chatHistory.Add($"손님: 주문한 마법이 맞군요. 거래 감사합니다.");

            InventoryManager.Instance.RemoveItem(item);
            CurrencyManager.Instance.AddCurrency(CurrencyType.Copper, finalPayment);
            CurrencyManager.Instance.CompressCurrency();

            order.state = OrderState.Completed;
            activeOrders.Remove(orderId);
            
            // 주문 성공 시 만족도 상승
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.ModifySatisfaction(2f);
            }

            Debug.Log($"<color=green>[Store] 거래 성사! 최종 입금액: {finalPayment} 동화</color>");
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

