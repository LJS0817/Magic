using UnityEngine;

namespace Magic.Inventory
{
    public enum ItemType { Scroll, Ink, Material, Potion, Pen }

    [System.Serializable]
    public class ItemData
    {
        public ItemType type;
        public string itemName;
    }

    [System.Serializable]
    public class Item_Scroll : ItemData
    {
        public bool isEmpty = true;
        public string spellName = "";
        public int currentDurability = 5;
        public float accuracyScore = 1.0f; // 마법 완성 시 평균 점수 (기본 1.0)

        public Item_Scroll(bool empty = true, string spell = "", int durability = 5)
        {
            type = ItemType.Scroll;
            isEmpty = empty;
            spellName = spell;
            currentDurability = durability;
            itemName = isEmpty ? "빈 스크롤" : $"스크롤 [{spellName}]";
        }
    }

    [System.Serializable]
    public class Item_Ink : ItemData
    {
        public float currentAmount;
        public float maxAmount;
        public string inkQuality; // 잉크 품질에 따라 그려지는 선의 색이나 마법 위력이 달라질 수 있음

        public Item_Ink(float amount = 100f, string quality = "Normal")
        {
            type = ItemType.Ink;
            maxAmount = amount;
            currentAmount = amount;
            inkQuality = quality;
            itemName = $"{inkQuality} 잉크";
        }
    }

    [System.Serializable]
    public class Item_Pen : ItemData
    {
        public float maxInkCapacity;
        public float currentInkCapacity;
        public float inkConsumptionRate; // 잉크 소모율 (등급에 따라 다름)
        public string penGrade; // 펜의 등급

        public Item_Pen(float capacity = 50f, float consumptionRate = 5f, string grade = "Normal")
        {
            type = ItemType.Pen;
            maxInkCapacity = capacity;
            currentInkCapacity = 0f; // 처음엔 비어있을 수 있음
            inkConsumptionRate = consumptionRate;
            penGrade = grade;
            itemName = $"{penGrade} 펜";
        }
    }
}