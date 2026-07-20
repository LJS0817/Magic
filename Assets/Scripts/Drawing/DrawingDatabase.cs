using UnityEngine;

namespace Magic.Drawing
{
    public class DrawingDatabase : MonoBehaviour
    {
        public static DrawingDatabase Instance { get; private set; }

        [Header("Global Spell Recipes")]
        [Tooltip("게임 전역에서 사용할 마법 레시피 에셋들을 넣어주세요.")]
        public SpellRecipeAsset[] recipes;
        
        [Header("Settings")]
        public GameObject linePrefab;

        public void InitInstance()
        {
            Instance = this;
        }

        public SpellRecipeAsset GetRecipeByName(string name)
        {
            if (recipes == null) return null;
            foreach (var recipe in recipes)
            {
                if (recipe != null && recipe.SpellName == name)
                {
                    return recipe;
                }
            }
            return null;
        }
    }
}
