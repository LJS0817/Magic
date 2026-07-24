using UnityEngine;
using UnityEditor;

public class EnemyDataGenerator
{
    [MenuItem("Magic/Generate Sample Enemies")]
    public static void GenerateEnemies()
    {
        string folderPath = "Assets/Resources/Enemies";
        
        // Ensure directories exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Enemies");
        }

        EnemyDataSO slime = CreateEnemyData(folderPath, "Slime", "Slime", 50f, 10f, SpellElement.None);
        EnemyDataSO goblin = CreateEnemyData(folderPath, "Goblin", "Goblin", 80f, 20f, SpellElement.Earth);
        EnemyDataSO fireSprite = CreateEnemyData(folderPath, "FireSprite", "Fire Sprite", 60f, 25f, SpellElement.Fire);
        EnemyDataSO iceGolem = CreateEnemyData(folderPath, "IceGolem", "Ice Golem", 150f, 15f, SpellElement.Ice);
        EnemyDataSO thunderWisp = CreateEnemyData(folderPath, "ThunderWisp", "Thunder Wisp", 40f, 30f, SpellElement.Lightning);
        EnemyDataSO demonLord = CreateEnemyData(folderPath, "DemonLord", "Demon Lord (Boss)", 500f, 50f, SpellElement.None);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Sample enemies generated successfully in {folderPath}");

        // Auto-assign to CombatManager if it exists in the scene
        CombatManager combatManager = GameObject.FindObjectOfType<CombatManager>();
        if (combatManager != null)
        {
            if (combatManager.normalEnemies == null) combatManager.normalEnemies = new System.Collections.Generic.List<EnemyDataSO>();
            if (combatManager.bossEnemies == null) combatManager.bossEnemies = new System.Collections.Generic.List<EnemyDataSO>();
            
            combatManager.normalEnemies.Clear();
            combatManager.bossEnemies.Clear();

            if (slime != null) combatManager.normalEnemies.Add(slime);
            if (goblin != null) combatManager.normalEnemies.Add(goblin);
            if (fireSprite != null) combatManager.normalEnemies.Add(fireSprite);
            if (iceGolem != null) combatManager.normalEnemies.Add(iceGolem);
            if (thunderWisp != null) combatManager.normalEnemies.Add(thunderWisp);
            if (demonLord != null) combatManager.bossEnemies.Add(demonLord);

            EditorUtility.SetDirty(combatManager);
            Debug.Log("Successfully auto-assigned enemies to CombatManager in the scene.");
        }
    }

    private static EnemyDataSO CreateEnemyData(string folderPath, string fileName, string enemyName, float hp, float dmg, SpellElement element)
    {
        string assetPath = $"{folderPath}/{fileName}.asset";
        
        EnemyDataSO existingData = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(assetPath);
        if (existingData != null)
        {
            Debug.LogWarning($"Enemy data {fileName} already exists. Skipping.");
            return existingData;
        }

        EnemyDataSO newData = ScriptableObject.CreateInstance<EnemyDataSO>();
        newData.enemyName = enemyName;
        newData.maxHealth = hp;
        newData.attackDamage = dmg;
        newData.defaultAttackElement = element;

        AssetDatabase.CreateAsset(newData, assetPath);
        return newData;
    }
}
