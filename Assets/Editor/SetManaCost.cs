using UnityEngine;
using UnityEditor;
using Magic.Drawing;

[InitializeOnLoad]
public class SetManaCostScript
{
    static SetManaCostScript()
    {
        if (EditorPrefs.GetBool("ManaCostSet", false)) return;
        
        string[] guids = AssetDatabase.FindAssets("t:SpellRecipeAsset");
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpellRecipeAsset asset = AssetDatabase.LoadAssetAtPath<SpellRecipeAsset>(path);
            if (asset != null)
            {
                float[] costs = { 20f, 30f, 40f, 50f, 60f };
                asset.manaCost = costs[UnityEngine.Random.Range(0, costs.Length)];
                EditorUtility.SetDirty(asset);
                count++;
            }
        }
        AssetDatabase.SaveAssets();
        EditorPrefs.SetBool("ManaCostSet", true);
        Debug.Log($"Successfully assigned random mana costs to {count} SpellRecipeAssets!");
    }
}
