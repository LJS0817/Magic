using UnityEngine;

public enum PotionType { Health, Mana, ElementalResistance }
public enum PotionGrade { Lesser, Medium, Greater }

[CreateAssetMenu(fileName = "New Potion", menuName = "Magic/Items/Potion")]
public class ItemPotionSO : ItemDataSO
{
    public PotionType potionType;
    public PotionGrade potionGrade;
    
    [Header("Recovery Settings")]
    [Tooltip("Amount of Health or Mana to recover")]
    public float recoveryAmount;

    [Header("Resistance Settings")]
    [Tooltip("Target Element for ElementalResistance potions")]
    public SpellElement resistanceElement = SpellElement.None;
    [Tooltip("Duration of resistance in seconds")]
    public float resistanceDuration = 30f;
    [Tooltip("Resistance reduction percentage (e.g., 0.25 for 25% reduction)")]
    public float resistancePercentage = 0.25f;

    private void Awake()
    {
        type = ItemType.Potion;
    }

    private void Reset()
    {
        type = ItemType.Potion;
    }

    private void OnValidate()
    {
        type = ItemType.Potion;
        
        if (potionType == PotionType.Health || potionType == PotionType.Mana)
        {
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
        else if (potionType == PotionType.ElementalResistance)
        {
            if (resistancePercentage == 0)
            {
                switch (potionGrade)
                {
                    case PotionGrade.Lesser: resistancePercentage = 0.15f; resistanceDuration = 20f; break;
                    case PotionGrade.Medium: resistancePercentage = 0.30f; resistanceDuration = 40f; break;
                    case PotionGrade.Greater: resistancePercentage = 0.50f; resistanceDuration = 60f; break;
                }
            }
        }
    }
}

