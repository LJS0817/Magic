using UnityEngine;
using System.Collections.Generic;
using Magic.Inventory;

namespace Magic.Inventory
{
    [CreateAssetMenu(fileName = "PenDatabase", menuName = "Magic/Pen Database")]
    public class PenDatabase : ScriptableObject
    {
        [Header("Pen Data Preset List")]
        public List<Item_Pen> pens = new List<Item_Pen>();

        /// <summary>
        /// 특정 이름의 펜을 데이터베이스에서 찾아 새로 생성하여 반환합니다.
        /// </summary>
        public Item_Pen CreatePenInstance(string name)
        {
            var template = pens.Find(p => p.itemName == name);
            if (template != null)
            {
                // 인스턴스로 생성하여 반환 (currentInkCapacity는 가득 찬 상태로 시작)
                var newPen = new Item_Pen(template.maxInkCapacity, template.inkConsumptionRate, template.penGrade, template.itemName, template.itemDescription);
                newPen.currentInkCapacity = template.maxInkCapacity;
                return newPen;
            }
            return null;
        }

        /// <summary>
        /// 특정 등급의 임의의 펜을 반환합니다.
        /// </summary>
        public Item_Pen CreateRandomPenOfGrade(string grade)
        {
            var filtered = pens.FindAll(p => p.penGrade == grade);
            if (filtered.Count > 0)
            {
                var template = filtered[Random.Range(0, filtered.Count)];
                var newPen = new Item_Pen(template.maxInkCapacity, template.inkConsumptionRate, template.penGrade, template.itemName, template.itemDescription);
                newPen.currentInkCapacity = template.maxInkCapacity;
                return newPen;
            }
            return null;
        }
    }
}
