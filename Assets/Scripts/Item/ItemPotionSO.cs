using UnityEngine;

namespace Magic.Inventory
{
    public enum PotionType { Health, Mana }
    public enum PotionGrade { Lesser, Medium, Greater }

    [CreateAssetMenu(fileName = "New Potion", menuName = "Magic/Items/Potion")]
    public class ItemPotionSO : ItemDataSO
    {
        public PotionType potionType;
        public PotionGrade potionGrade;
        
        [Header("Recovery Settings")]
        [Tooltip("Amount of Health or Mana to recover")]
        public float recoveryAmount;

        private void Awake()
        {
            type = ItemType.Potion;
        }

        private void OnValidate()
        {
            type = ItemType.Potion;
            
            // Set default recovery amounts based on grade if it's currently 0
            if (recoveryAmount == 0)
            {
                switch (potionGrade)
                {
                    case PotionGrade.Lesser: recoveryAmount = 25f; break;
                    case PotionGrade.Medium: recoveryAmount = 50f; break;
                    case PotionGrade.Greater: recoveryAmount = 100f; break;
                }
            }
        }
    }
}
