using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class RecipeEntryUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image iconImage;
    public GameObject selectObj;

    public SpellRecipeAsset RecipeData { get; private set; }
    public RecipeUnlockState UnlockState { get; private set; }

    private RecipeCompendiumUIController controller;
    private System.Action<RecipeEntryUI> onHoverEnter;
    private System.Action<RecipeEntryUI> onHoverExit;

    public void Setup(SpellRecipeAsset recipe, RecipeUnlockState state, RecipeCompendiumUIController ctrl, System.Action<RecipeEntryUI> onEnter = null, System.Action<RecipeEntryUI> onExit = null)
    {
        RecipeData = recipe;
        UnlockState = state;
        controller = ctrl;
        onHoverEnter = onEnter;
        onHoverExit = onExit;

        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        if (UnlockState == RecipeUnlockState.Unlocked)
        {
            if (iconImage != null)
            {
                if (RecipeData.icon != null)
                {
                    iconImage.sprite = RecipeData.icon;
                    iconImage.color = Color.white;
                }
                else
                {
                    // Placeholder
                    iconImage.color = Color.gray; 
                }
            }
        }
        else if (UnlockState == RecipeUnlockState.Hinted)
        {
            if (iconImage != null) iconImage.color = Color.black; // Silhouette or hidden
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        controller.SelectRecipe(this, RecipeData, UnlockState);
    }

    public void SetSelected(bool isSelected)
    {
        if (selectObj != null) selectObj.SetActive(isSelected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverEnter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverExit?.Invoke(this);
    }
}

