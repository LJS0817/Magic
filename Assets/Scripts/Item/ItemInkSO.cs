using UnityEngine;

namespace Magic.Inventory
{
    [CreateAssetMenu(fileName = "New Ink Data", menuName = "Magic/Items/Ink Data")]
    public class ItemInkSO : ItemDataSO
    {
        [Header("Ink Specifics")]
        public float maxAmount;
        public Color inkColor = Color.black; // 그려지는 잉크의 색상
        public Magic.Combat.SpellElement inkElement = Magic.Combat.SpellElement.None;

        [Header("Ink Parts (For Assembled Visual)")]
        public Sprite inkBaseSprite;
        public Sprite inkLiquidSprite;
        public Sprite inkLabelSprite;
        public Sprite inkCapSprite;

        private void OnEnable()
        {
            type = ItemType.Ink;
        }
    }
}
