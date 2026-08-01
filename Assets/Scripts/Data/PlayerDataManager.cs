using UnityEngine;
using System.Collections.Generic;

public enum RecipeUnlockState
{
    Locked = 0,
    Hinted = 1,
    Unlocked = 2
}

public enum GuildRank
{
    Iron,       // 0 ~ 19
    Bronze,     // 20 ~ 39
    Silver,     // 40 ~ 69
    Gold,       // 70 ~ 89
    Mithril,    // 90 ~ 99
    Orichalcum  // 100
}

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("Store Orders Persistence")]
    public Dictionary<string, CustomerOrder> activeOrders = new Dictionary<string, CustomerOrder>();
    public bool hasGeneratedInitialOrders = false;

    [Header("Time and Store Management")]   
    public int currentDay = 1;
    public event System.Action<int> OnDayChanged;

    [SerializeField, Range(0f, 100f)] 
    private float _storeSatisfaction = 0f;
    public float StoreSatisfaction
    {
        get => _storeSatisfaction;
        set
        {
            var oldRank = CurrentGuildRank;
            _storeSatisfaction = Mathf.Clamp(value, 0f, 100f);
            OnSatisfactionChanged?.Invoke(_storeSatisfaction);
            var newRank = CurrentGuildRank;
            if (oldRank != newRank)
            {
                OnGuildRankChanged?.Invoke(newRank);
            }
        }
    }
    public event System.Action<float> OnSatisfactionChanged;

    public GuildRank CurrentGuildRank
    {
        get
        {
            if (_storeSatisfaction >= 100f) return GuildRank.Orichalcum;
            if (_storeSatisfaction >= 90f) return GuildRank.Mithril;
            if (_storeSatisfaction >= 70f) return GuildRank.Gold;
            if (_storeSatisfaction >= 40f) return GuildRank.Silver;
            if (_storeSatisfaction >= 20f) return GuildRank.Bronze;
            return GuildRank.Iron;
        }
    }
    public event System.Action<GuildRank> OnGuildRankChanged;

    public void AdvanceDay()
    {
        currentDay++;
        Debug.Log($"<color=yellow>[시간] 다음 날이 되었습니다. 현재 일차: {currentDay}일</color>");
        OnDayChanged?.Invoke(currentDay);
    }

    public void ModifySatisfaction(float amount)
    {
        StoreSatisfaction += amount;
        Debug.Log($"[상점 만족도] {(amount > 0 ? "상승" : "하락")}: {amount}. 현재 만족도: {_storeSatisfaction:F1}/100");
    }

    [Header("Recipe Compendium")]
    public Dictionary<string, RecipeUnlockState> unlockedRecipes = new Dictionary<string, RecipeUnlockState>();
    
    [Header("Recipe Objects Cache")]
    public List<SpellRecipeAsset> unlockedRecipeObjects = new List<SpellRecipeAsset>();
    public List<SpellRecipeAsset> visibleRecipeObjects = new List<SpellRecipeAsset>();

    public event System.Action OnRecipeUpdated;

    public void RebuildRecipeCache()
    {
        unlockedRecipeObjects.Clear();
        visibleRecipeObjects.Clear();

        var db = DrawingDatabase.Instance;
        if (db != null && db.recipes != null)
        {
            foreach (var recipe in db.recipes)
            {
                RecipeUnlockState state = GetRecipeState(recipe.SpellName);
                if (state == RecipeUnlockState.Unlocked)
                {
                    unlockedRecipeObjects.Add(recipe);
                    visibleRecipeObjects.Add(recipe);
                }
                else if (state == RecipeUnlockState.Hinted)
                {
                    visibleRecipeObjects.Add(recipe);
                }
            }
        }

        OnRecipeUpdated?.Invoke();
    }

    public void UnlockRecipe(string spellName)
    {
        bool changed = false;
        if (unlockedRecipes.ContainsKey(spellName))
        {
            if (unlockedRecipes[spellName] != RecipeUnlockState.Unlocked)
            {
                unlockedRecipes[spellName] = RecipeUnlockState.Unlocked;
                changed = true;
                Debug.Log($"[도감] '{spellName}' 레시피가 완전히 해금되었습니다!");
            }
        }
        else
        {
            unlockedRecipes.Add(spellName, RecipeUnlockState.Unlocked);
            changed = true;
            Debug.Log($"[도감] '{spellName}' 레시피가 도감에 새롭게 등록되었습니다!");
        }

        if (changed) RebuildRecipeCache();
    }

    public void UnlockHint(string spellName)
    {
        bool changed = false;
        if (!unlockedRecipes.ContainsKey(spellName))
        {
            unlockedRecipes.Add(spellName, RecipeUnlockState.Hinted);
            changed = true;
            Debug.Log($"[도감] '{spellName}' 레시피의 힌트를 얻었습니다!");
        }
        else if (unlockedRecipes[spellName] == RecipeUnlockState.Locked)
        {
            unlockedRecipes[spellName] = RecipeUnlockState.Hinted;
            changed = true;
            Debug.Log($"[도감] '{spellName}' 레시피의 힌트를 얻었습니다!");
        }
        
        if (changed) RebuildRecipeCache();
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
    public float baseMaxMana = 100f;
    public float currentMana = 100f;


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

    public event System.Action<float, float> OnManaChanged;

    public void NotifyManaChanged()
    {
        OnManaChanged?.Invoke(currentMana, GetMaxMana());
    }

    public void InitInstance()
    {
        Instance = this;
        RebuildRecipeCache();
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
        
        float mpRegen = UpgradeManager.Instance.GetTotalUpgradeValue(UpgradeType.ManaRegeneration);

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

    public int GetOrderCount(int poolCount)
    {
        switch (CurrentGuildRank)
        {
            case GuildRank.Iron: return poolCount - 5;
            case GuildRank.Bronze: return poolCount - 4;
            case GuildRank.Silver: return poolCount - 3;
            case GuildRank.Gold: return poolCount - 2;
            case GuildRank.Mithril: return poolCount - 1;
            default: return poolCount;
        }
    }
}

