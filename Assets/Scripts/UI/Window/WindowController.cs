using Magic.Store;
using Magic.UI.Compendium;
using UnityEngine;
using UnityEngine.UI;

namespace Magic.UI
{
    public class WindowController : MonoBehaviour
    {
        [Header("Windows")]
        [SerializeField] CanvasGroup _drawing;
        [SerializeField] RecipeCompendiumUIController _recipe;
        [SerializeField] CanvasGroup _inventory;
        
        public void ShowDrawingArea() {
            _drawing.alpha = 1f;
            _drawing.blocksRaycasts = true;
            _drawing.interactable = true;
        }

        public void HideDrawingArea() {
            _drawing.alpha = 0f;
            _drawing.blocksRaycasts = false;
            _drawing.interactable = false;
        }

        public void ToggleRecipeUI()
        {
            _recipe.ToggleWindow();
            _inventory.alpha = _recipe.IsOpened ? 0f : 1f;
            _drawing.blocksRaycasts = !_recipe.IsOpened;
            _drawing.interactable = !_recipe.IsOpened;
        }
    }
}
