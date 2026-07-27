using UnityEngine;

[CreateAssetMenu(fileName = "New Robe", menuName = "Magic/Item/Robe")]
public class ItemRobeSO : ItemDataSO
{
    [Header("Robe Stats")]
    [Tooltip("방어력 보너스")]
    public int bonusDefense = 5;
    [Tooltip("공격력 보너스")]
    public int bonusAttack = 0;
    [Tooltip("최대 마나 보너스")]
    public float bonusMaxMana = 20f;
    
    [Header("Elemental Protection")]
    public SpellElement robeElement = SpellElement.None;
    [Tooltip("해당 속성에 대한 내성 보너스 (예: 0.15 = 15%)")]
    public float elementResistanceBonus = 0f;

    private void Reset()
    {
        type = ItemType.Robe;
    }

    private void OnValidate()
    {
        type = ItemType.Robe;
    }
}
