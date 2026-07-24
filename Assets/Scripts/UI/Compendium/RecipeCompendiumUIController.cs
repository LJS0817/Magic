using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RecipeCompendiumUIController : PagedUIController<RecipeEntryUI>
{
    [SerializeField] ItemInfoPanel itemInfoPanel;
    [SerializeField] CanvasGroup recipeListGroup;
    [Header("Detail Panel")]
    public CanvasGroup detailPanel;
    public Image detailIcon;
    public Image detailSampleImage;
    public TMP_Text detailNameText;
    public TMP_Text detailTypeText;
    public TMP_Text detailManaText;
    public TMP_Text detailDescText;
    public TMP_Text detailConditionText;
    public TMP_Text detailHintText;
    public TMP_Text detailShapesText;

    bool isOpened = false;
    public bool IsOpened => isOpened;

    private void Start()
    {
        Close();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    private List<SpellRecipeAsset> _visibleRecipes = new List<SpellRecipeAsset>();
    private RecipeEntryUI currentSelectedSlot;

    public override void RefreshList()
    {
        var database = DrawingDatabase.Instance;
        if (database == null || database.recipes == null) return;

        _visibleRecipes.Clear();
        foreach (var recipe in database.recipes)
        {
            PlayerDataManager.Instance.UnlockRecipe(recipe.SpellName);
            RecipeUnlockState state = PlayerDataManager.Instance.GetRecipeState(recipe.SpellName);
            Debug.Log(recipe.SpellName + "     " + state);
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
            slot.Setup(recipe, state, this, OnSlotHoverEnter, OnSlotHoverExit);
        }
    }

    private void OnSlotHoverEnter(RecipeEntryUI slot)
    {
        if (itemInfoPanel != null && slot.UnlockState != RecipeUnlockState.Locked)
        {
            itemInfoPanel.Open();
            string name = slot.UnlockState == RecipeUnlockState.Unlocked ? slot.RecipeData.SpellName : "???";
            itemInfoPanel.SetupRecipeInfo(name);
            itemInfoPanel.ClippingPosition(slot.GetComponent<RectTransform>(), _slotContainer.GetComponent<RectTransform>());
        }
    }

    private void OnSlotHoverExit(RecipeEntryUI slot)
    {
        if (itemInfoPanel != null)
        {
            itemInfoPanel.Close();
        }
    }

    public void SelectRecipe(RecipeEntryUI slot, SpellRecipeAsset recipe, RecipeUnlockState state)
    {
        if (currentSelectedSlot != null)
            currentSelectedSlot.SetSelected(false);

        currentSelectedSlot = slot;

        if (currentSelectedSlot != null)
            currentSelectedSlot.SetSelected(true);

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
        if (currentSelectedSlot != null)
        {
            currentSelectedSlot.SetSelected(false);
            currentSelectedSlot = null;
        }

        detailPanel.alpha = 0f;
        detailPanel.blocksRaycasts = false;
        detailPanel.interactable = false;

        recipeListGroup.alpha = 0f;
        recipeListGroup.blocksRaycasts = false;
        recipeListGroup.interactable = false;
        isOpened = false;
        DrawingManager.IsDrawingBlocked = false;
    }

    public void Open()
    {
        detailPanel.alpha = 1f;
        detailPanel.blocksRaycasts = true;
        detailPanel.interactable = true;

        recipeListGroup.alpha = 1f;
        recipeListGroup.blocksRaycasts = true;
        recipeListGroup.interactable = true;

        DrawingManager.IsDrawingBlocked = true;
        isOpened = true;
        RefreshList();
    }

    public void ToggleWindow()
    {
        if (isOpened) Close();
        else Open();
    }
}

