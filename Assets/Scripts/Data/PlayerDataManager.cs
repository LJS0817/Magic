using UnityEngine;
using System.Collections.Generic;
using Magic.Inventory;

namespace Magic.Data
{
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        [Header("Player Status")]
        public float maxHealth = 100f;
        public float currentHealth = 100f;

        public float maxMana = 100f;
        public float currentMana = 100f;

        [Header("Inventory Data")]
        public List<ItemInstance> items = new List<ItemInstance>();
        public List<ItemInstance> combatLoadout = new List<ItemInstance>();

        [Header("Equipped Data")]
        public Item_Scroll equippedScroll;
        public Item_Ink equippedInk;
        public Item_Pen equippedPen;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
