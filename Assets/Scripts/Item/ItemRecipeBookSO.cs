using UnityEngine;

namespace Magic.Inventory
{
    [CreateAssetMenu(fileName = "NewRecipeBook", menuName = "Magic/Items/Recipe Book")]
    public class ItemRecipeBookSO : ItemDataSO
    {
        [Header("Recipe Book")]
        [Tooltip("해금할 마법 레시피의 이름 (예: Fireball)")]
        public string targetSpellName;

        [Tooltip("true면 힌트만 해금, false면 레시피 전체 해금")]
        public bool isHintOnly = false;
    }
}
