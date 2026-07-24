using UnityEngine;

[CreateAssetMenu(fileName = "NewWand", menuName = "Magic/Item/Wand")]
public class ItemWandSO : ItemDataSO
{
    [Header("Wand Specifics")]
    public float defaultManaCostMultiplier = 1.0f;
    
    private void Reset()
    {
        type = ItemType.Wand;
    }
}

