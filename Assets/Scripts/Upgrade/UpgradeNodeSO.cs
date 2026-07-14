using System;
using System.Collections.Generic;
using UnityEngine;
using Magic.Inventory;

namespace Magic.Upgrade
{
    [Serializable]
    public struct UpgradeEffect
    {
        public UpgradeType upgradeType;
        public float value;
        [Tooltip("Optional: Specifies which element/shape this applies to (e.g., 'Fire', 'Circle')")]
        public string targetName;
    }

    [CreateAssetMenu(fileName = "NewUpgradeNode", menuName = "Magic/Upgrade/Node")]
    public class UpgradeNodeSO : ScriptableObject
    {
        [Header("Node Identity")]
        public string nodeID;
        public string nodeName;
        [TextArea]
        public string description;

        [Header("Unlock Requirements")]
        public List<UpgradeNodeSO> requiredParents;
        
        [Tooltip("Cost in Gem to unlock this node")]
        public int costAmount;

        [Header("Upgrade Effects")]
        public List<UpgradeEffect> effects;

        [Header("UI Layout")]
        [Tooltip("Position relative to the center (0,0) in the mind-map")]
        public Vector2 uiPosition;
    }
}
