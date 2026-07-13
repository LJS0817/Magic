using UnityEngine;

namespace Magic.Inventory
{
    [CreateAssetMenu(fileName = "New Ink Data", menuName = "Magic/Items/Ink Data")]
    public class ItemInkSO : ItemDataSO
    {
        [Header("Ink Specifics")]
        public float maxAmount;
        public string inkQuality; // 잉크 품질
        public Color inkColor = Color.black; // 그려지는 잉크의 색상

        private void OnEnable()
        {
            type = ItemType.Ink;
        }
    }
}
