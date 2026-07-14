using UnityEngine;

namespace Magic.Store
{
    [System.Serializable]
    public class CustomerOrder
    {
        public string customerName;
        public string orderDescription;
        public string requestedSpellName;
        public Magic.Combat.SpellElement requestedElement;
        
        public int marketPrice;
        public int customerBudget;
        
        // Use this to display the base reward if needed, or we can just use marketPrice/budget
        public int offeredReward; 

        public CustomerOrder(string name, string desc, string spell, Magic.Combat.SpellElement element, int marketPrice, int budget)
        {
            customerName = name;
            orderDescription = desc;
            requestedSpellName = spell;
            requestedElement = element;
            this.marketPrice = marketPrice;
            this.customerBudget = budget;
        }
    }
}
