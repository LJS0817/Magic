using UnityEngine;
using DG.Tweening;

public class WindowController : MonoBehaviour
{
    [Header("Windows")]
    [SerializeField] CanvasGroup _drawing;
    [SerializeField] RecipeCompendiumUIController _recipe;
    [SerializeField] CanvasGroup _inventory;
    [SerializeField] CombatPreparationManager _preparation;
    [SerializeField] StoreUIController _store;

    [SerializeField] UpgradeUIController _upgrade;
    [SerializeField] CanvasGroup _menu;
    //[SerializeField] CanvasGroup _order;
    [SerializeField] RectTransform _frame;

    [SerializeField] MapUIController _map;
    RectTransform _frameParent;

    [Header("Animation")]
    [SerializeField] float _animDuration = 0.3f;

    Vector2 _originAnchorMin;
    Vector2 _originAnchorMax;
    Vector2 _originOffsetMin;
    Vector2 _originOffsetMax;

    void Awake()
    {
        _originAnchorMin = _frame.anchorMin;
        _originAnchorMax = _frame.anchorMax;
        _originOffsetMin = _frame.offsetMin;
        _originOffsetMax = _frame.offsetMax;
    }

    void Update()
    {
        if (_recipe != null)
        {
            if ((_store.IsOpened || !RecipeDeveloper.IsDrawingBlocked) && Input.GetKeyDown(KeyCode.A)) ToggleStoreUI();
            if (!RecipeDeveloper.IsDrawingBlocked && Input.GetKeyDown(KeyCode.D)) OpenUpgrade();
            if ((_recipe.IsOpened || !RecipeDeveloper.IsDrawingBlocked) && Input.GetKeyDown(KeyCode.W)) ToggleRecipeUI();
        }
    }
    
    public void ShowDrawingArea() {
        if (_drawing == null) return;
        _drawing.DOKill();
        _drawing.DOFade(1f, _animDuration);
        _drawing.blocksRaycasts = true;
        _drawing.interactable = true;
        RecipeDeveloper.IsDrawingBlocked = false;
    }

    public void HideDrawingArea()
    {
        if (_drawing == null) return;
        _drawing.DOKill();
        _drawing.DOFade(0f, _animDuration);
        _drawing.blocksRaycasts = false;
        _drawing.interactable = false;
        RecipeDeveloper.IsDrawingBlocked = true;
    }

    public void CloseStoreUI()
    {
        if (_store == null) return;
        _store.Close();
        ShowDrawingArea();
    }

    public void ToggleStoreUI()
    {
        if (_store == null) return;
        _store.ToggleUI();
        if (_store.IsOpened) HideDrawingArea();
        else ShowDrawingArea();
        _recipe.Close(false, _store.IsOpened);
        _upgrade.SetVisibleArrow(!_store.IsOpened);
    }

    public void ToggleRecipeUI()
    {
        if (_recipe == null) return;
        _recipe.ToggleWindow();
        
        if(_recipe.IsOpened) HideDrawingArea();
        else ShowDrawingArea();

        _inventory.DOKill();
        _inventory.DOFade(_recipe.IsOpened ? 0f : 1f, _animDuration);
        _inventory.blocksRaycasts = !_recipe.IsOpened;
        _inventory.interactable = !_recipe.IsOpened;
        if (_store.IsOpened) ToggleStoreUI();
        _store.SetVisibleArrow(!_recipe.IsOpened);
        _upgrade.SetVisibleArrow(!_recipe.IsOpened);
        //_drawing.blocksRaycasts = !_recipe.IsOpened;
        //_drawing.interactable = !_recipe.IsOpened;
    }

    public void SetFullFrame()
    {
        if (_frame == null) return;
        _frame.DOKill();
        DOTween.To(() => _frame.anchorMin, x => _frame.anchorMin = x, Vector2.zero, _animDuration).SetTarget(_frame);
        DOTween.To(() => _frame.anchorMax, x => _frame.anchorMax = x, Vector2.one, _animDuration).SetTarget(_frame);
        DOTween.To(() => _frame.offsetMin, x => _frame.offsetMin = x, Vector2.zero, _animDuration).SetTarget(_frame);
        DOTween.To(() => _frame.offsetMax, x => _frame.offsetMax = x, Vector2.zero, _animDuration).SetTarget(_frame);

        SetMenuVisible(false);
        _recipe.Close(false, true);
        _upgrade.SetVisibleArrow(false);
        _store.SetVisibleArrow(false);
        HideDrawingArea();
    }

    public void SetOriginFrame()
    {
        if (_frame == null) return;
        _frame.DOKill();
        DOTween.To(() => _frame.anchorMin, x => _frame.anchorMin = x, _originAnchorMin, _animDuration).SetTarget(_frame);
        DOTween.To(() => _frame.anchorMax, x => _frame.anchorMax = x, _originAnchorMax, _animDuration).SetTarget(_frame);
        DOTween.To(() => _frame.offsetMin, x => _frame.offsetMin = x, _originOffsetMin, _animDuration).SetTarget(_frame);
        DOTween.To(() => _frame.offsetMax, x => _frame.offsetMax = x, _originOffsetMax, _animDuration).SetTarget(_frame);

        SetMenuVisible(true);
        _recipe.Close(false, false);
        _upgrade.SetVisibleArrow(true);
        _store.SetVisibleArrow(true);
        ShowDrawingArea();
    }

    void SetMenuVisible(bool show)
    {
        if (_menu == null) return;
        _menu.DOKill();
        _menu.DOFade(show ? 1f : 0f, _animDuration);
        _menu.blocksRaycasts = show;
        _menu.interactable = show;
    }

    public void OpenUpgrade()
    {
        if (_upgrade == null) return;
        _upgrade.Open();
        HideDrawingArea();
        SetFullFrame();
    }

    public void CloseUpgrade()
    {
        if (_upgrade == null) return;
        _upgrade.Close();
        ShowDrawingArea();
        SetOriginFrame();
    }

    public void OpenMap()
    {
        _map.Open();
        CloseUpgrade();
        HideDrawingArea();
        if (_recipe != null && _recipe.IsOpened) ToggleRecipeUI();
        if (_store != null && _store.IsOpened) ToggleStoreUI();
    }

    public void CloseMap()
    {
        _map.Close();
        ShowDrawingArea();
    }

    public void OpenLoadout()
    {
        _preparation.OpenPreparation();
        _map.Close(true);
    }

    public void CloseLoadout()
    {
        _preparation.Close();
        _map.Open(true);
    }
}

