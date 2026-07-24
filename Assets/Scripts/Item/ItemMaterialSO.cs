using UnityEngine;

[CreateAssetMenu(fileName = "New Material", menuName = "Magic/Items/Material")]
public class ItemMaterialSO : ItemDataSO
{
    private void Awake()
    {
        type = ItemType.Material;
    }
}

