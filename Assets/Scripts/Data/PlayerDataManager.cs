using UnityEngine;
using System.Collections.Generic;
using Magic.Inventory;

namespace Magic.Data
{
    public enum RecipeUnlockState
    {
        Locked = 0,
        Hinted = 1,
        Unlocked = 2
    }

    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        [Header("Recipe Compendium")]
        public Dictionary<string, RecipeUnlockState> unlockedRecipes = new Dictionary<string, RecipeUnlockState>();

        public void UnlockRecipe(string spellName)
        {
            if (unlockedRecipes.ContainsKey(spellName))
            {
                if (unlockedRecipes[spellName] != RecipeUnlockState.Unlocked)
                {
                    unlockedRecipes[spellName] = RecipeUnlockState.Unlocked;
                    Debug.Log($"[도감] '{spellName}' 레시피가 완전히 해금되었습니다!");
                }
            }
            else
            {
                unlockedRecipes.Add(spellName, RecipeUnlockState.Unlocked);
                Debug.Log($"[도감] '{spellName}' 레시피가 도감에 새롭게 등록되었습니다!");
            }
        }

        public void UnlockHint(string spellName)
        {
            if (!unlockedRecipes.ContainsKey(spellName))
            {
                unlockedRecipes.Add(spellName, RecipeUnlockState.Hinted);
                Debug.Log($"[도감] '{spellName}' 레시피의 힌트를 얻었습니다!");
            }
            else if (unlockedRecipes[spellName] == RecipeUnlockState.Locked)
            {
                unlockedRecipes[spellName] = RecipeUnlockState.Hinted;
                Debug.Log($"[도감] '{spellName}' 레시피의 힌트를 얻었습니다!");
            }
        }

        public RecipeUnlockState GetRecipeState(string spellName)
        {
            if (unlockedRecipes.TryGetValue(spellName, out RecipeUnlockState state))
            {
                return state;
            }
            return RecipeUnlockState.Locked;
        }

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
        public Item_Wand equippedWand;
        public Item_Pouch equippedPouch;

        public void InitInstance()
        {
            Instance = this;
        }

        public bool IsInit => Instance != null;
    }
}
