using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class UpgradeTreeGenerator : UnityEditor.Editor
{
    private class BranchDef
    {
        public string branchName;
        public List<UpgradeType> allowedTypes;
        public float baseAngle; // The central angle for this branch in the UI
        public List<UpgradeNodeSO> currentLeaves = new List<UpgradeNodeSO>();
    }

    [MenuItem("Magic/Generate 60 Random Nodes")]
    public static void GenerateLogicalTree()
    {
        string folderPath = "Assets/Resources/Upgrades/Generated";

        if (!AssetDatabase.IsValidFolder("Assets/Resources/Upgrades"))
            AssetDatabase.CreateFolder("Assets/Resources", "Upgrades");
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets/Resources/Upgrades", "Generated");

        // Clear existing generated nodes to avoid duplicates
        string[] existingAssets = AssetDatabase.FindAssets("t:UpgradeNodeSO", new[] { folderPath });
        foreach (var guid in existingAssets)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
        }

        // Keep track of levels to avoid duplicate nodes
        Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>();
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            upgradeLevels[type] = 0;
        }

        List<UpgradeNodeSO> allGeneratedNodes = new List<UpgradeNodeSO>();

        // Define Branches (Logical Grouping)
        BranchDef[] branches = new BranchDef[]
        {
            new BranchDef() { 
                branchName = "Fortitude", 
                allowedTypes = new List<UpgradeType> { UpgradeType.MaxHealth, UpgradeType.HealthRegeneration, UpgradeType.DropRateBonus },
                baseAngle = 45f
            },
            new BranchDef() { 
                branchName = "Wisdom", 
                allowedTypes = new List<UpgradeType> { UpgradeType.MaxMana, UpgradeType.ManaRegeneration, UpgradeType.FreeInkChance },
                baseAngle = 135f
            },
            new BranchDef() { 
                branchName = "Destruction", 
                allowedTypes = new List<UpgradeType> { UpgradeType.SpellDamageMultiplier, UpgradeType.ElementDamageBonus, UpgradeType.ShapeDamageBonus },
                baseAngle = 225f
            },
            new BranchDef() { 
                branchName = "Utility", 
                allowedTypes = new List<UpgradeType> { UpgradeType.InventoryCapacity, UpgradeType.SellPriceMultiplier, UpgradeType.ShopDiscount, UpgradeType.ShapeSellBonus },
                baseAngle = 315f
            }
        };

        // Root Node
        UpgradeNodeSO rootNode = CreateInstance<UpgradeNodeSO>();
        rootNode.nodeID = "RootNode";
        rootNode.nodeName = "Core Mastery";
        rootNode.description = "The beginning of your magical journey.";
        rootNode.uiPosition = Vector2.zero;
        rootNode.costAmount = 0;
        rootNode.requiredParents = new List<UpgradeNodeSO>();
        AssetDatabase.CreateAsset(rootNode, $"{folderPath}/Node_00_Root.asset");
        allGeneratedNodes.Add(rootNode);

        float radiusStep = 180f;
        int nodeCounter = 1;

        // Sub-Roots
        foreach (var branch in branches)
        {
            UpgradeNodeSO subRoot = CreateInstance<UpgradeNodeSO>();
            subRoot.nodeID = System.Guid.NewGuid().ToString();
            subRoot.nodeName = $"Path of {branch.branchName}";
            subRoot.description = $"Unlocks the {branch.branchName} upgrade tree.";
            subRoot.costAmount = 10;
            
            // Add a small thematic effect to the sub-root to make it worth buying
            subRoot.effects = new List<UpgradeEffect>();
            UpgradeEffect subRootEffect = new UpgradeEffect();
            if (branch.branchName == "Fortitude") {
                subRootEffect.upgradeType = UpgradeType.MaxHealth;
                subRootEffect.value = 10f;
            } else if (branch.branchName == "Wisdom") {
                subRootEffect.upgradeType = UpgradeType.MaxMana;
                subRootEffect.value = 10f;
            } else if (branch.branchName == "Destruction") {
                subRootEffect.upgradeType = UpgradeType.SpellDamageMultiplier;
                subRootEffect.value = 0.05f;
            } else if (branch.branchName == "Utility") {
                subRootEffect.upgradeType = UpgradeType.InventoryCapacity;
                subRootEffect.value = 5f;
            }
            subRoot.effects.Add(subRootEffect);

            float rad = branch.baseAngle * Mathf.Deg2Rad;
            subRoot.uiPosition = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radiusStep;
            
            subRoot.requiredParents = new List<UpgradeNodeSO> { rootNode };
            
            AssetDatabase.CreateAsset(subRoot, $"{folderPath}/Node_{nodeCounter:D2}_{branch.branchName}Root.asset");
            allGeneratedNodes.Add(subRoot);
            branch.currentLeaves.Add(subRoot);
            nodeCounter++;
        }

        // Distribute 60 total nodes (1 root + 4 sub-roots + 55 nodes)
        int remainingNodes = 60 - nodeCounter; 

        string[] elements = new string[] { "Fire", "Water", "Earth", "Wind" };
        string[] shapes = new string[] { "Circle", "Triangle", "Square" };

        while (remainingNodes > 0)
        {
            foreach (var branch in branches)
            {
                if (remainingNodes <= 0) break;

                UpgradeType selectedType = branch.allowedTypes[Random.Range(0, branch.allowedTypes.Count)];
                
                // Logical Dependencies
                if (selectedType == UpgradeType.HealthRegeneration && upgradeLevels[UpgradeType.MaxHealth] == 0)
                    selectedType = UpgradeType.MaxHealth;
                if (selectedType == UpgradeType.ManaRegeneration && upgradeLevels[UpgradeType.MaxMana] == 0)
                    selectedType = UpgradeType.MaxMana;

                upgradeLevels[selectedType]++;
                int currentLv = upgradeLevels[selectedType];

                UpgradeNodeSO newNode = CreateInstance<UpgradeNodeSO>();
                newNode.nodeID = System.Guid.NewGuid().ToString();
                newNode.nodeName = $"{selectedType} Lv.{currentLv}";
                newNode.description = $"Increases your {selectedType}.";
                newNode.costAmount = currentLv * 15;

                newNode.effects = new List<UpgradeEffect>();
                UpgradeEffect effect = new UpgradeEffect();
                effect.upgradeType = selectedType;
                
                // Set Specific Values and Limits
                switch (selectedType)
                {
                    case UpgradeType.ShopDiscount:
                        effect.value = 5f; // Max 20%, so 4 levels will hit max
                        newNode.description = $"상점 구매 가격을 {effect.value}% 할인받습니다.";
                        break;
                    case UpgradeType.ElementDamageBonus:
                        effect.value = 5f; // Increments of 5% to reach up to 30%
                        effect.targetName = elements[Random.Range(0, elements.Length)];
                        newNode.nodeName = $"{effect.targetName} {selectedType} Lv.{currentLv}";
                        newNode.description = $"{effect.targetName} 원소 마법의 대미지가 {effect.value}% 증가합니다.";
                        break;
                    case UpgradeType.ShapeDamageBonus:
                        effect.value = 3f; // 5 levels to reach 15%
                        effect.targetName = shapes[Random.Range(0, shapes.Length)];
                        newNode.nodeName = $"{effect.targetName} Dmg Lv.{currentLv}";
                        newNode.description = $"{effect.targetName} 도형 마법의 대미지가 {effect.value}% 증가합니다.";
                        break;
                    case UpgradeType.ShapeSellBonus:
                        effect.value = 10f; // 10% bonus sell price for a shape
                        effect.targetName = shapes[Random.Range(0, shapes.Length)];
                        newNode.nodeName = $"{effect.targetName} Sell Lv.{currentLv}";
                        newNode.description = $"{effect.targetName} 도형이 포함된 스크롤의 판매 가격이 {effect.value}% 증가합니다.";
                        break;
                    case UpgradeType.DropRateBonus:
                        effect.value = 2f; // 2% drop rate per level
                        newNode.description = $"아이템 드롭 확률이 {effect.value}% 증가합니다.";
                        break;
                    case UpgradeType.FreeInkChance:
                        effect.value = 1f; // 1% chance not to use ink per level
                        newNode.description = $"마법을 그릴 때 {effect.value}% 확률로 잉크를 소모하지 않습니다.";
                        break;
                    case UpgradeType.SpellDamageMultiplier:
                        effect.value = 0.05f; // 5% per level
                        newNode.description = $"모든 주문의 대미지가 {effect.value * 100}% 증가합니다.";
                        break;
                    case UpgradeType.SellPriceMultiplier:
                        effect.value = 0.05f; // 5% per level
                        newNode.description = $"모든 아이템의 판매 가격이 {effect.value * 100}% 증가합니다.";
                        break;
                    case UpgradeType.InventoryCapacity:
                        effect.value = 5f; // +5 slots per level
                        newNode.description = $"인벤토리 최대 칸 수가 {effect.value}칸 증가합니다.";
                        break;
                    default:
                        effect.value = currentLv * 10f; // Flat +10/20/30
                        newNode.description = $"{selectedType} 수치가 {effect.value} 증가합니다.";
                        break;
                }

                newNode.effects.Add(effect);

                // Parent Connection
                newNode.requiredParents = new List<UpgradeNodeSO>();
                UpgradeNodeSO parent = branch.currentLeaves[Random.Range(0, branch.currentLeaves.Count)];
                newNode.requiredParents.Add(parent);

                // Position
                float currentRadius = parent.uiPosition.magnitude + radiusStep * Random.Range(0.7f, 1.3f);
                float angleOffset = Random.Range(-30f, 30f);
                float finalAngle = branch.baseAngle + angleOffset;
                float rad = finalAngle * Mathf.Deg2Rad;

                newNode.uiPosition = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * currentRadius;

                AssetDatabase.CreateAsset(newNode, $"{folderPath}/Node_{nodeCounter:D2}_{selectedType}.asset");
                allGeneratedNodes.Add(newNode);
                
                branch.currentLeaves.Add(newNode);
                if (Random.value > 0.4f) branch.currentLeaves.Remove(parent);

                nodeCounter++;
                remainingNodes--;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>Successfully generated 60 logically grouped Upgrade Nodes in {folderPath}!</color>");
    }

    [MenuItem("Magic/Auto Assign Nodes to Manager")]
    public static void AutoAssignNodesToManager()
    {
        UpgradeManager manager = FindObjectOfType<UpgradeManager>();
        if (manager == null)
        {
            Debug.LogWarning("Cannot find an UpgradeManager in the current scene. Please open the scene with the UpgradeManager and try again.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:UpgradeNodeSO");
        manager.allNodes = new List<UpgradeNodeSO>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UpgradeNodeSO node = AssetDatabase.LoadAssetAtPath<UpgradeNodeSO>(path);
            if (node != null)
            {
                manager.allNodes.Add(node);
            }
        }

        EditorUtility.SetDirty(manager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        
        Debug.Log($"<color=cyan>Successfully auto-assigned {manager.allNodes.Count} Upgrade Nodes to the UpgradeManager in the scene!</color>");
    }
}

