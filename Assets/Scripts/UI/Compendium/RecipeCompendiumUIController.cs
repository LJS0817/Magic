using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Magic.Drawing;
using Magic.Data;
using System.Collections.Generic;

namespace Magic.UI.Compendium
{
    public class RecipeCompendiumUIController : Magic.UI.PagedUIController<RecipeEntryUI>
    {
        

        [Header("Detail Panel")]
        public GameObject detailPanel;
        public Image detailIcon;
        public Image detailSampleImage;
        public TMP_Text detailNameText;
        public TMP_Text detailTypeText;
        public TMP_Text detailManaText;
        public TMP_Text detailDescText;
        public TMP_Text detailConditionText;
        public TMP_Text detailHintText;
        public TMP_Text detailShapesText;

        private void Start()
        {
            if (detailPanel != null)
                detailPanel.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        private List<SpellRecipeAsset> _visibleRecipes = new List<SpellRecipeAsset>();

        public override void RefreshList()
        {
            var database = DrawingDatabase.Instance;
            if (database == null || database.recipes == null) return;

            _visibleRecipes.Clear();
            foreach (var recipe in database.recipes)
            {
                RecipeUnlockState state = PlayerDataManager.Instance.GetRecipeState(recipe.SpellName);
                if (state != RecipeUnlockState.Locked)
                {
                    _visibleRecipes.Add(recipe);
                }
            }

            base.RefreshList();
        }

        protected override int GetTotalCapacity()
        {
            return _visibleRecipes.Count;
        }

        protected override void UpdateSlot(RecipeEntryUI slot, int dataIndex)
        {
            if (dataIndex < _visibleRecipes.Count)
            {
                var recipe = _visibleRecipes[dataIndex];
                RecipeUnlockState state = PlayerDataManager.Instance.GetRecipeState(recipe.SpellName);
                slot.Setup(recipe, state, this);
            }
        }

        public void SelectRecipe(SpellRecipeAsset recipe, RecipeUnlockState state)
        {
            if (detailPanel != null)
                detailPanel.SetActive(true);

            if (state == RecipeUnlockState.Unlocked)
            {
                if (detailNameText != null) detailNameText.text = recipe.SpellName;
                if (detailSampleImage != null) detailSampleImage.sprite = recipe.drawingSampleImage;
                if (detailTypeText != null) detailTypeText.text = $"타입: {recipe.Type}";
                if (detailManaText != null) detailManaText.text = $"마나 소모: {recipe.manaCost}";
                if (detailDescText != null) detailDescText.text = recipe.Description;
                if (detailConditionText != null) detailConditionText.text = $"조건: {recipe.ActivationCondition}";
                if (detailHintText != null) detailHintText.text = $"힌트: {recipe.ActivationHint}";
                
                if (detailIcon != null) detailIcon.sprite = recipe.icon;

                if (detailShapesText != null)
                {
                    string shapes = "";
                    if (recipe.RequiredShapes != null)
                    {
                        shapes = string.Join(" + ", recipe.RequiredShapes);
                    }
                    detailShapesText.text = $"필요 도형: {shapes}";
                }
            }
            else if (state == RecipeUnlockState.Hinted)
            {
                if (detailNameText != null) detailNameText.text = "???";
                if (detailTypeText != null) detailTypeText.text = "타입: ???";
                if (detailManaText != null) detailManaText.text = "마나 소모: ???";
                if (detailDescText != null) detailDescText.text = "아직 이 마법의 구조를 파악하지 못했습니다.";
                if (detailConditionText != null) detailConditionText.text = "조건: ???";
                
                // 힌트만 제공하는 것으로 기획 (B 조건 반영)
                if (detailHintText != null) detailHintText.text = $"힌트: {recipe.ActivationHint}";
                if (detailShapesText != null) detailShapesText.text = "필요 도형: ???";
                
                if (detailIcon != null) detailIcon.color = Color.black;
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
            Magic.Drawing.DrawingManager.IsDrawingBlocked = false;
        }

        public void Open()
        {
            gameObject.SetActive(true);
            Magic.Drawing.DrawingManager.IsDrawingBlocked = true;
            RefreshList();
        }
    }
}
