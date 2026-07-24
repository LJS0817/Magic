using UnityEngine;

[CreateAssetMenu(fileName = "New Ink Data", menuName = "Magic/Items/Ink Data")]
public class ItemInkSO : ItemDataSO
{
    [Header("Ink Specifics")]
    public float maxAmount;
    public Color inkColor = Color.black; // 그려지는 잉크의 색상
    public SpellElement inkElement = SpellElement.None;

    [Header("Ink Parts (For Assembled Visual)")]
    public Sprite inkBaseSprite;
    public Sprite inkLiquidSprite;
    public Color inkLiquidColorAlpha = Color.white;
    public Sprite inkTopBackSprite;
    public Sprite inkTopSprite;

    private void OnEnable()
    {
        type = ItemType.Ink;
    }
}

