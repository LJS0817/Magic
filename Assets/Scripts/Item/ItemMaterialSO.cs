using UnityEngine;

[CreateAssetMenu(fileName = "New Material", menuName = "Magic/Items/Material")]
public class ItemMaterialSO : ItemDataSO
{
    private void Awake()
    {
        type = ItemType.Material;
    }

    private void Reset()
    {
        type = ItemType.Material;
    }

    private void OnValidate()
    {
        type = ItemType.Material;
    }
}

