using Magic.Store;
using Magic.UI.Compendium;
using Magic.Upgrade;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;
using DG.Tweening;

namespace Magic.UI
{
    public class WindowController : MonoBehaviour
    {
        [Header("Windows")]
        [SerializeField] CanvasGroup _drawing;
        [SerializeField] RecipeCompendiumUIController _recipe;
        [SerializeField] CanvasGroup _inventory;

        [SerializeField] UpgradeUIController _upgrade;
        [SerializeField] CanvasGroup _menu;
        [SerializeField] CanvasGroup _order;
        [SerializeField] RectTransform _frame;
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
        
        public void ShowDrawingArea() {
            _drawing.DOKill();
            _drawing.DOFade(1f, _animDuration);
            _drawing.blocksRaycasts = true;
            _drawing.interactable = true;
        }

        public void HideDrawingArea() {
            _drawing.DOKill();
            _drawing.DOFade(0f, _animDuration);
            _drawing.blocksRaycasts = false;
            _drawing.interactable = false;
        }

        public void ToggleRecipeUI()
        {
            _recipe.ToggleWindow();
            _inventory.DOKill();
            _inventory.DOFade(_recipe.IsOpened ? 0f : 1f, _animDuration);
            _drawing.blocksRaycasts = !_recipe.IsOpened;
            _drawing.interactable = !_recipe.IsOpened;
        }

        public void SetFullFrame()
        {
            _frame.DOKill();
            DOTween.To(() => _frame.anchorMin, x => _frame.anchorMin = x, Vector2.zero, _animDuration).SetTarget(_frame);
            DOTween.To(() => _frame.anchorMax, x => _frame.anchorMax = x, Vector2.one, _animDuration).SetTarget(_frame);
            DOTween.To(() => _frame.offsetMin, x => _frame.offsetMin = x, Vector2.zero, _animDuration).SetTarget(_frame);
            DOTween.To(() => _frame.offsetMax, x => _frame.offsetMax = x, Vector2.zero, _animDuration).SetTarget(_frame);

            SetMenuVisible(false);
        }

        public void SetOriginFrame()
        {
            _frame.DOKill();
            DOTween.To(() => _frame.anchorMin, x => _frame.anchorMin = x, _originAnchorMin, _animDuration).SetTarget(_frame);
            DOTween.To(() => _frame.anchorMax, x => _frame.anchorMax = x, _originAnchorMax, _animDuration).SetTarget(_frame);
            DOTween.To(() => _frame.offsetMin, x => _frame.offsetMin = x, _originOffsetMin, _animDuration).SetTarget(_frame);
            DOTween.To(() => _frame.offsetMax, x => _frame.offsetMax = x, _originOffsetMax, _animDuration).SetTarget(_frame);

            SetMenuVisible(true);
        }

        void SetMenuVisible(bool show)
        {
            _menu.DOKill();
            _menu.DOFade(show ? 1f : 0f, _animDuration);
            _menu.blocksRaycasts = show;
            _menu.interactable = show;

            _order.DOKill();
            _order.DOFade(show ? 1f : 0f, _animDuration);
            _order.blocksRaycasts = show;
            _order.interactable = show;
        }

        public void OpenUpgrade()
        {
            _upgrade.Open();
            SetFullFrame();
        }

        public void CloseUpgrade()
        {
            _upgrade.Close();
            SetOriginFrame();
        }
    }
}
