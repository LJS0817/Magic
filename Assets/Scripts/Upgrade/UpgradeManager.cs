using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Data")]
    [Tooltip("All available upgrade nodes in the game")]
    public List<UpgradeNodeSO> allNodes;

    // In a real game, this would be saved and loaded from a save file
    public List<string> unlockedNodeIDs = new List<string>();

    public event System.Action OnUpgradeUnlocked;

    private Dictionary<UpgradeType, float> _cachedUpgradeValues = new Dictionary<UpgradeType, float>();
    private Dictionary<string, float> _cachedTargetedValues = new Dictionary<string, float>();


    public void InitInstance()
    {
        Instance = this;
        if (allNodes != null)
        {
            foreach (var node in allNodes)
            {
                if (node != null && string.IsNullOrEmpty(node.nodeID))
                {
                    node.nodeID = node.name;
                }
            }
        }
        InitializeCache();
    }

    public bool IsNodeUnlocked(UpgradeNodeSO node)
    {
        if (node == null) return false;
        return unlockedNodeIDs.Contains(node.nodeID);
    }

    public bool IsNodeUnlockable(UpgradeNodeSO node)
    {
        if (node == null) return false;
        
        // Already unlocked
        if (IsNodeUnlocked(node)) return false;

        // If it has no required parents, it's unlockable (root node)
        if (node.requiredParents == null || node.requiredParents.Count == 0) return true;

        // Must have at least ONE required parent unlocked to branch out
        // (Or ALL parents? Usually in mind maps, one is enough to bridge, but sometimes all are required. 
        // We'll require AT LEAST ONE parent to be unlocked to proceed, making it true branching)
        foreach (var parent in node.requiredParents)
        {
            if (IsNodeUnlocked(parent))
            {
                return true;
            }
        }
        return false;
    }

    public bool TryUnlockNode(UpgradeNodeSO node)
    {
        if (!IsNodeUnlockable(node)) return false;

        // Check if player has enough gems
        if (CurrencyManager.Instance.HasEnoughCurrency(CurrencyType.Gem, node.costAmount))
        {
            // Spend Gem
            CurrencyManager.Instance.SpendCurrency(CurrencyType.Gem, node.costAmount);
            
            // Unlock
            unlockedNodeIDs.Add(node.nodeID);

            // Update caches with only the newly unlocked node's effects
            AddNodeEffectsToCache(node);

            // Instantly apply some effects like filling health/mana
            ApplyInstantEffects(node);

            OnUpgradeUnlocked?.Invoke();
            return true;
        }

        return false;
    }

    private void ApplyInstantEffects(UpgradeNodeSO node)
    {
        if (PlayerDataManager.Instance == null) return;

        foreach (var effect in node.effects)
        {
            if (effect.upgradeType == UpgradeType.MaxHealth)
            {
                PlayerDataManager.Instance.currentHealth = PlayerDataManager.Instance.GetMaxHealth();
            }
            else if (effect.upgradeType == UpgradeType.MaxMana)
            {
                PlayerDataManager.Instance.currentMana = PlayerDataManager.Instance.GetMaxMana();
            }
        }
    }

    private void InitializeCache()
    {
        _cachedUpgradeValues.Clear();
        _cachedTargetedValues.Clear();

        foreach (var nodeID in unlockedNodeIDs)
        {
            var node = allNodes.FirstOrDefault(n => n != null && n.nodeID == nodeID);
            if (node != null)
            {
                AddNodeEffectsToCache(node);
            }
        }
    }

    private void AddNodeEffectsToCache(UpgradeNodeSO node)
    {
        if (node == null || node.effects == null) return;

        foreach (var effect in node.effects)
        {
            // Add to general cache
            if (!_cachedUpgradeValues.ContainsKey(effect.upgradeType))
                _cachedUpgradeValues[effect.upgradeType] = 0f;
            
            _cachedUpgradeValues[effect.upgradeType] += effect.value;

            // Add to targeted cache if targetName exists
            if (!string.IsNullOrEmpty(effect.targetName))
            {
                string key = $"{effect.upgradeType}_{effect.targetName.ToLower()}";
                if (!_cachedTargetedValues.ContainsKey(key))
                    _cachedTargetedValues[key] = 0f;
                
                _cachedTargetedValues[key] += effect.value;
            }
        }
    }

    public float GetTotalUpgradeValue(UpgradeType type)
    {
        return _cachedUpgradeValues.TryGetValue(type, out float val) ? val : 0f;
    }

    public float GetTotalTargetedUpgradeValue(UpgradeType type, string targetName)
    {
        if (string.IsNullOrEmpty(targetName)) return 0f;
        string key = $"{type}_{targetName.ToLower()}";
        return _cachedTargetedValues.TryGetValue(key, out float val) ? val : 0f;
    }
}

