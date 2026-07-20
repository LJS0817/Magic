using System.Collections.Generic;
using UnityEngine;
using Magic.Data;
using Magic.Inventory;
using System.Linq;

namespace Magic.Upgrade
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [Header("Data")]
        [Tooltip("All available upgrade nodes in the game")]
        public List<UpgradeNodeSO> allNodes;

        // In a real game, this would be saved and loaded from a save file
        public List<string> unlockedNodeIDs = new List<string>();

        public event System.Action OnUpgradeUnlocked;


        public void InitInstance()
        {
            Instance = this;
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

        public float GetTotalUpgradeValue(UpgradeType type)
        {
            float total = 0f;
            foreach (var nodeID in unlockedNodeIDs)
            {
                var node = allNodes.FirstOrDefault(n => n.nodeID == nodeID);
                if (node != null)
                {
                    foreach (var effect in node.effects)
                    {
                        if (effect.upgradeType == type)
                        {
                            total += effect.value;
                        }
                    }
                }
            }
            return total;
        }

        public float GetTotalTargetedUpgradeValue(UpgradeType type, string targetName)
        {
            float total = 0f;
            foreach (var nodeID in unlockedNodeIDs)
            {
                var node = allNodes.FirstOrDefault(n => n.nodeID == nodeID);
                if (node != null)
                {
                    foreach (var effect in node.effects)
                    {
                        // Check type AND targetName (case-insensitive)
                        if (effect.upgradeType == type && 
                            string.Equals(effect.targetName, targetName, System.StringComparison.OrdinalIgnoreCase))
                        {
                            total += effect.value;
                        }
                    }
                }
            }
            return total;
        }
    }
}
