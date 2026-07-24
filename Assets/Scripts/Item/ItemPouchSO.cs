using UnityEngine;

[CreateAssetMenu(fileName = "NewPouch", menuName = "Magic/Item/Pouch")]
public class ItemPouchSO : ItemDataSO
{
    [Header("Pouch Specifics")]
    [Tooltip("전투 로드아웃에 장착 시 추가되는 용량")]
    public int loadoutCapacityBonus = 1;
    
    private void Reset()
    {
        type = ItemType.Pouch;
    }
}

