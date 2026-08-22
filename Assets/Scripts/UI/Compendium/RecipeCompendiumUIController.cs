using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class RecipeCompendiumUIController : PagedUIController<RecipeEntryUI>
{
    [SerializeField] CanvasGroup recipeListGroup;
    [SerializeField] Image _arrowImage;
    [SerializeField] CanvasGroup _recipeDefault;
    [SerializeField] CanvasGroup _recipeInfo;
    [Header("Detail Panel")]
    public CanvasGroup detailPanel;
    RectTransform _detailPanelRect;
    public Image detailIcon;
    public Image detailSampleImage;
    public TMP_Text detailNameText;
    public TMP_Text detailTypeText;
    public TMP_Text detailManaText;
    public TMP_Text detailDescText;
    public TMP_Text detailConditionText;
    public TMP_Text detailHintText;
    public TMP_Text detailShapesText;

    [Tooltip("Ease.OutBack provides a single bounce (overshoot) effect.")]
    [SerializeField] private float animationDuration = 0.2f;

    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;

    bool isOpened = false;
    public bool IsOpened => isOpened;

    private void Start()
    {
        _detailPanelRect = detailPanel.GetComponent<RectTransform>();
        Close(true);
        PlayerDataManager.Instance.UnlockRecipe("Orb");

        _detailPanelRect.anchoredPosition = new Vector2(_detailPanelRect.anchoredPosition.x, 200f);
        
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnRecipeUpdated += RefreshList;
        }
        RefreshList();
    }

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnRecipeUpdated -= RefreshList;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    private List<SpellRecipeAsset> _visibleRecipes = new List<SpellRecipeAsset>();
    private RecipeEntryUI currentSelectedSlot;

    public override void RefreshList()
    {
        _visibleRecipes.Clear();
        if (PlayerDataManager.Instance != null)
        {
            _visibleRecipes.AddRange(PlayerDataManager.Instance.visibleRecipeObjects);
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
        if (_infoPanel != null && slot.UnlockState != RecipeUnlockState.Locked)
        {
            _infoPanel.Open();
            string name = slot.UnlockState == RecipeUnlockState.Unlocked ? slot.RecipeData.SpellName : "???";
            _infoPanel.SetupRecipeInfo(name);

        }
    }

    private void OnSlotHoverExit(RecipeEntryUI slot)
    {
        if (_infoPanel != null)
        {
            _infoPanel.Close();
        }
    }

    public void SelectRecipe(RecipeEntryUI slot, SpellRecipeAsset recipe, RecipeUnlockState state)
    {
        if (_recipeDefault.alpha == 1f)
        {
            _recipeDefault.alpha = 0f;
            _recipeInfo.alpha = 1f;
        }

        if (currentSelectedSlot != null)
            currentSelectedSlot.SetSelected(false);

        currentSelectedSlot = slot;

        if (currentSelectedSlot != null)
            currentSelectedSlot.SetSelected(true);

        if (state == RecipeUnlockState.Unlocked)
        {
            if (detailNameText != null) detailNameText.text = recipe.SpellName;
            if (detailSampleImage != null) detailSampleImage.sprite = recipe.drawingSampleImage != null ? recipe.drawingSampleImage : recipe.icon;
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

        if (detailConditionText != null) LayoutRebuilder.ForceRebuildLayoutImmediate(detailConditionText.rectTransform);
        if (detailHintText != null) LayoutRebuilder.ForceRebuildLayoutImmediate(detailHintText.rectTransform);
        if (detailShapesText != null) LayoutRebuilder.ForceRebuildLayoutImmediate(detailShapesText.rectTransform);
    }

    public void Close(bool forceClose = false, bool hideArrow = false)
    {
        if (currentSelectedSlot != null)
        {
            currentSelectedSlot.SetSelected(false);
            currentSelectedSlot = null;
        }

        if(forceClose)
        {
            detailPanel.alpha = 0f;
            recipeListGroup.alpha = 0f;
        } else
        {
            detailPanel.DOKill();
            detailPanel.DOFade(0f, animationDuration).SetEase(closeEase);

            recipeListGroup.DOKill();
            recipeListGroup.DOFade(0f, animationDuration).SetEase(closeEase);

            _arrowImage.DOKill();
            _arrowImage.DOFade(hideArrow ? 0f : 1f, animationDuration).SetEase(openEase);
        }
        
        detailPanel.blocksRaycasts = false;
        detailPanel.interactable = false;

        recipeListGroup.blocksRaycasts = false;
        recipeListGroup.interactable = false;

        isOpened = false;
        DrawingManager.IsDrawingBlocked = false;
    }

    public void Open()
    {
        detailPanel.DOKill();
        detailPanel.DOFade(1f, animationDuration).SetEase(openEase);
        detailPanel.blocksRaycasts = true;
        detailPanel.interactable = true;

        recipeListGroup.DOKill();
        recipeListGroup.DOFade(1f, animationDuration).SetEase(openEase);
        recipeListGroup.blocksRaycasts = true;
        recipeListGroup.interactable = true;
        
        _arrowImage.DOKill();
        _arrowImage.DOFade(0f, animationDuration).SetEase(Ease.Linear);

        DrawingManager.IsDrawingBlocked = true;
        isOpened = true;

        _recipeDefault.alpha = 1f;
        _recipeInfo.alpha = 0f;
    }

    public void ToggleWindow()
    {
        if (isOpened) Close();
        else Open();
    
        _detailPanelRect.DOKill();
        _detailPanelRect.DOAnchorPosY(isOpened ? 0f : 200f, animationDuration).SetEase(isOpened ? openEase : closeEase);
    }
}

