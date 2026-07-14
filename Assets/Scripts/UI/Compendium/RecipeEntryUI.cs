using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Magic.Drawing;
using Magic.Data;

namespace Magic.UI.Compendium
{
    public class RecipeEntryUI : MonoBehaviour
    {
        [Header("UI References")]
        public Image iconImage;
        public TMP_Text nameText;
        public Button button;

        private SpellRecipeAsset recipeData;
        private RecipeUnlockState unlockState;
        private RecipeCompendiumUIController controller;

        public void Setup(SpellRecipeAsset recipe, RecipeUnlockState state, RecipeCompendiumUIController ctrl)
        {
            recipeData = recipe;
            unlockState = state;
            controller = ctrl;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClick);
            }

            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            if (unlockState == RecipeUnlockState.Unlocked)
            {
                if (nameText != null) nameText.text = recipeData.SpellName;
                
                if (iconImage != null)
                {
                    if (recipeData.icon != null)
                    {
                        iconImage.sprite = recipeData.icon;
                        iconImage.color = Color.white;
                    }
                    else
                    {
                        // Placeholder
                        iconImage.color = Color.gray; 
                    }
                }
            }
            else if (unlockState == RecipeUnlockState.Hinted)
            {
                if (nameText != null) nameText.text = "???";
                if (iconImage != null) iconImage.color = Color.black; // Silhouette or hidden
            }
        }

        private void OnClick()
        {
            if (controller != null && recipeData != null)
            {
                controller.SelectRecipe(recipeData, unlockState);
            }
        }
    }
}
