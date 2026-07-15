using UnityEngine;

namespace Magic.Store
{
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
        public string customerName;
        public string orderDescription;
        public string requestedSpellName;
        public Magic.Combat.SpellElement requestedElement;
        
        public int marketPrice;
        public int customerBudget; // This is now used as claimedBudget when initially shown? Wait, let's add specific fields.
        
        public int trueBudget;
        public int claimedBudget;
        public bool isBluffing;
        
        public OrderState state;
        public CustomerFaction faction;
        public float patience;
        public int agreedPrice;

        public System.Collections.Generic.List<string> chatHistory;

        // Use this to display the base reward if needed, or we can just use marketPrice/budget
        public int offeredReward; 

        public CustomerOrder(string name, string desc, string spell, Magic.Combat.SpellElement element, int marketPrice, int tBudget, int cBudget, bool bluff, CustomerFaction fac)
        {
            orderID = System.Guid.NewGuid().ToString();
            customerName = name;
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
        }
    }
}
