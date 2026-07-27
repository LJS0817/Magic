using UnityEngine;
using System.Collections.Generic;

public enum RecipeUnlockState
{
    Locked = 0,
    Hinted = 1,
    Unlocked = 2
}

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("Store Orders Persistence")]
    public Dictionary<string, CustomerOrder> activeOrders = new Dictionary<string, CustomerOrder>();
    public bool hasGeneratedInitialOrders = false;

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
        float upgradeBonus = UpgradeManager.Instance != null 
            ? UpgradeManager.Instance.GetTotalUpgradeValue(UpgradeType.MaxHealth) 
            : 0f;
        return baseMaxHealth + upgradeBonus;
    }

    public float GetMaxMana()
    {
        float upgradeBonus = UpgradeManager.Instance != null 
            ? UpgradeManager.Instance.GetTotalUpgradeValue(UpgradeType.MaxMana) 
            : 0f;
        float robeBonus = (equippedRobe != null && equippedRobe.RobeData != null) ? equippedRobe.RobeData.bonusMaxMana : 0f;
        float cloakBonus = (equippedCloak != null && equippedCloak.CloakData != null) ? equippedCloak.CloakData.bonusMaxMana : 0f;
        return baseMaxMana + upgradeBonus + robeBonus + cloakBonus;
    }

    public int GetTotalDefense()
    {
        int def = 0;
        if (equippedRobe != null && equippedRobe.RobeData != null) def += equippedRobe.RobeData.bonusDefense;
        if (equippedCloak != null && equippedCloak.CloakData != null) def += equippedCloak.CloakData.bonusDefense;
        return def;
    }

    public int GetTotalBonusAttack()
    {
        int atk = 0;
        if (equippedRobe != null && equippedRobe.RobeData != null) atk += equippedRobe.RobeData.bonusAttack;
        if (equippedCloak != null && equippedCloak.CloakData != null) atk += equippedCloak.CloakData.bonusAttack;
        return atk;
    }

    private Dictionary<SpellElement, float> _activeResistancePotions = new Dictionary<SpellElement, float>();
    private Dictionary<SpellElement, float> _resistanceTimers = new Dictionary<SpellElement, float>();

    public void ApplyElementalResistancePotion(SpellElement element, float percentage, float duration)
    {
        if (element == SpellElement.None) return;
        _activeResistancePotions[element] = percentage;
        _resistanceTimers[element] = duration;
    }

    public float GetElementalResistance(SpellElement element)
    {
        if (element == SpellElement.None) return 0f;
        float res = 0f;
        if (equippedRobe != null && equippedRobe.RobeData != null && equippedRobe.RobeData.robeElement == element)
            res += equippedRobe.RobeData.elementResistanceBonus;
        if (equippedCloak != null && equippedCloak.CloakData != null && equippedCloak.CloakData.cloakElement == element)
            res += equippedCloak.CloakData.elementResistanceBonus;
        if (_activeResistancePotions.TryGetValue(element, out float potionRes))
            res += potionRes;
        return Mathf.Clamp01(res);
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
    public Item_Robe equippedRobe;
    public Item_Cloak equippedCloak;
    public Item_DrawingTool equippedDrawingTool;

    public event System.Action<float, float> OnManaChanged;

    public void NotifyManaChanged()
    {
        OnManaChanged?.Invoke(currentMana, GetMaxMana());
    }

    public void InitInstance()
    {
        Instance = this;
    }

    public bool IsInit => Instance != null;

    private void Start()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeUnlocked += NotifyManaChanged;
        }
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeUnlocked -= NotifyManaChanged;
        }
    }

    private void Update()
    {
        if (UpgradeManager.Instance == null) return;
        
        float hpRegen = UpgradeManager.Instance.GetTotalUpgradeValue(UpgradeType.HealthRegeneration);
        float mpRegen = UpgradeManager.Instance.GetTotalUpgradeValue(UpgradeType.ManaRegeneration);

        if (hpRegen > 0f && currentHealth < GetMaxHealth())
        {
            currentHealth = Mathf.Min(GetMaxHealth(), currentHealth + hpRegen * Time.deltaTime);
        }

        if (mpRegen > 0f && currentMana < GetMaxMana())
        {
            currentMana = Mathf.Min(GetMaxMana(), currentMana + mpRegen * Time.deltaTime);
            NotifyManaChanged();
        }

        if (_resistanceTimers.Count > 0)
        {
            List<SpellElement> keys = new List<SpellElement>(_resistanceTimers.Keys);
            foreach (var key in keys)
            {
                _resistanceTimers[key] -= Time.deltaTime;
                if (_resistanceTimers[key] <= 0f)
                {
                    _resistanceTimers.Remove(key);
                    _activeResistancePotions.Remove(key);
                    Debug.Log($"<color=gray>[물약] {key} 속성 내성 효과가 종료되었습니다.</color>");
                }
            }
        }
    }
}

