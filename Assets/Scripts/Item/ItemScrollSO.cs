using UnityEngine;

namespace Magic.Inventory
{
    [CreateAssetMenu(fileName = "New Scroll Data", menuName = "Magic/Items/Scroll Data")]
    public class ItemScrollSO : ItemDataSO
    {
        [Header("Scroll Specifics")]
        public string spellName = "";
        public int maxDurability = 5;
        public float accuracyScore = 1.0f; // 마법 완성 시 평균 점수 (기본 1.0)
        public string scrollGrade = "Normal";
        public Magic.Combat.SpellElement scrollElement = Magic.Combat.SpellElement.None;

        private void OnEnable()
        {
            type = ItemType.Scroll;
        }
    }
}
