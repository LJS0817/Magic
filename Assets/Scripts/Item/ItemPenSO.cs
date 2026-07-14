using UnityEngine;

namespace Magic.Inventory
{
    [CreateAssetMenu(fileName = "New Pen Data", menuName = "Magic/Items/Pen Data")]
    public class ItemPenSO : ItemDataSO
    {
        [Header("Pen Specifics")]
        public float maxInkCapacity;
        public float inkConsumptionRate; // 등급에 따라 다름
        public string penGrade; // 펜의 등급
        public bool consumesMana; // 잉크 대신 플레이어 마력을 소모하는지 여부
        public Magic.Combat.SpellElement penElement = Magic.Combat.SpellElement.None;

        private void OnEnable()
        {
            type = ItemType.Pen;
        }
    }
}
