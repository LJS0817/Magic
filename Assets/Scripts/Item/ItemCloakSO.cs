using UnityEngine;

[CreateAssetMenu(fileName = "New Cloak", menuName = "Magic/Item/Cloak")]
public class ItemCloakSO : ItemDataSO
{
    [Header("Cloak Stats")]
    [Tooltip("방어력 보너스")]
    public int bonusDefense = 3;
    [Tooltip("공격력 보너스")]
    public int bonusAttack = 2;
    [Tooltip("최대 마나 보너스")]
    public float bonusMaxMana = 10f;
    
    [Header("Elemental Protection")]
    public SpellElement cloakElement = SpellElement.None;
    [Tooltip("해당 속성에 대한 내성 보너스 (예: 0.10 = 10%)")]
    public float elementResistanceBonus = 0f;

    private void Reset()
    {
        type = ItemType.Cloak;
    }

    private void OnValidate()
    {
        type = ItemType.Cloak;
    }
}
