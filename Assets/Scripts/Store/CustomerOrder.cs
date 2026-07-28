using UnityEngine;

public enum OrderState
{
    Received,
    Haggling,
    Pending,
    Completed,
    Failed
}

public enum CustomerFaction
{
    Peasant,
    Mercenary,
    Noble,
    MageGuild,
    Cultist
}

[System.Serializable]
public class CustomerOrder
{
    public string orderID;
    public string orderDescription;
    public string requestedSpellName;
    public SpellElement requestedElement;
    
    public long marketPrice;
    public long customerBudget; // This is now used as claimedBudget when initially shown? Wait, let's add specific fields.
    
    public long trueBudget;
    public long claimedBudget;
    public bool isBluffing;
    
    [SerializeField] private OrderState _state;
    public OrderState state
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                OnStateChanged?.Invoke(_state);
            }
        }
    }
    public System.Action<OrderState> OnStateChanged;
    public CustomerFaction faction;
    public float patience;
    public long agreedPrice;

    public int deadlineDaysLeft; // 남은 마감일

    public System.Collections.Generic.List<string> chatHistory;

    // Use this to display the base reward if needed, or we can just use marketPrice/budget
    public long offeredReward; 

    public CustomerOrder(string desc, string spell, SpellElement element, long marketPrice, long tBudget, long cBudget, bool bluff, CustomerFaction fac)
    {
        orderID = System.Guid.NewGuid().ToString();
        orderDescription = desc;
        requestedSpellName = spell;
        requestedElement = element;
        this.marketPrice = marketPrice;
        
        this.trueBudget = tBudget;
        this.claimedBudget = cBudget;
        this.isBluffing = bluff;
        this.faction = fac;
        
        this.state = OrderState.Received;
        this.patience = 100f; // default patience
        this.agreedPrice = 0;
        this.chatHistory = new System.Collections.Generic.List<string>();

        // 3일에서 7일 사이의 랜덤한 마감 기한 설정
        this.deadlineDaysLeft = UnityEngine.Random.Range(3, 8);
    }
}

