using UnityEngine;
using System.Collections.Generic;
using Magic.Inventory;

namespace Magic.Data
{
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        [Header("Player Status")]
        public float baseMaxHealth = 100f;
        public float currentHealth = 100f;

        public float baseMaxMana = 100f;
        public float currentMana = 100f;

        public float GetMaxHealth()
        {
            float upgradeBonus = Magic.Upgrade.UpgradeManager.Instance != null 
                ? Magic.Upgrade.UpgradeManager.Instance.GetTotalUpgradeValue(Magic.Upgrade.UpgradeType.MaxHealth) 
                : 0f;
            return baseMaxHealth + upgradeBonus;
        }

        public float GetMaxMana()
        {
            float upgradeBonus = Magic.Upgrade.UpgradeManager.Instance != null 
                ? Magic.Upgrade.UpgradeManager.Instance.GetTotalUpgradeValue(Magic.Upgrade.UpgradeType.MaxMana) 
                : 0f;
            return baseMaxMana + upgradeBonus;
        }

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
