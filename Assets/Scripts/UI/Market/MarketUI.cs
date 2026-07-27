using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public enum MarketType
{
    EquipmentShop,
    DrawingShop,
    PotionShop,
    AdventurerGuild
}

public class MarketUI : PagedUIController<MarketItemSlotUI>
{
    [SerializeField] CanvasGroup _canvasGroup;
    RectTransform _rectTransform;
    public MarketType marketType;
    private List<ItemDataSO> _marketItems = new List<ItemDataSO>();
    [SerializeField] MarketItemInfoUI _infoUI;
    private MarketItemSlotUI _selectedSlot;
    [SerializeField] Sprite _slotImage;
    [SerializeField] ItemFilterGroup _filterGroup;
    private List<ItemDataSO> _filteredItems = new List<ItemDataSO>();

    public void Setup(List<ItemDataSO> items)
    {
        _marketItems = items;
        if (_filterGroup != null)
        {
            _filterGroup.Init();
            _filterGroup.ResetFilter();
        }
        else
        {
            FilterItems(null);
            RefreshList();
        }
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }

    protected override int GetTotalCapacity()
    {
        return _filteredItems != null ? _filteredItems.Count : 0;
    }

    protected override void OnSlotCreated(MarketItemSlotUI slot)
    {
        if (slot != null) slot.SetFocusImage(_slotImage);
    }

    protected override void UpdateSlot(MarketItemSlotUI slot, int dataIndex)
    {
        if (_filteredItems != null && dataIndex >= 0 && dataIndex < _filteredItems.Count)
        {
            slot.Setup(_filteredItems[dataIndex], (item, price) => OnSlotClicked(slot, item, price));
        }
        else
        {
            slot.gameObject.SetActive(false);
        }
    }

    private void OnSlotClicked(MarketItemSlotUI clickedSlot, ItemDataSO itemData, long price)
    {
        if (_selectedSlot != null && _selectedSlot != clickedSlot)
        {
            _selectedSlot.Deselect();
        }

        _selectedSlot = clickedSlot;
        if (_selectedSlot != null)
        {
            _selectedSlot.Select();
        }

        if (_infoUI != null)
        {
            _infoUI.SetUp(itemData, price);
        }
    }

    public override void RefreshList()
    {
        if (_selectedSlot != null)
        {
            _selectedSlot.Deselect(immediate: true);
            _selectedSlot = null;
        }
        if (_infoUI != null)
        {
            _infoUI.Clear();
        }
        base.RefreshList();
    }

    protected override void Awake()
    {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_filterGroup == null) _filterGroup = GetComponentInChildren<ItemFilterGroup>(true);
        if (_filterGroup != null)
        {
            _filterGroup.Init();
            _filterGroup.OnFilterChanged += OnFilterChanged;
        }
    }

    private void OnFilterChanged(ItemFilter filter)
    {
        FilterItems(filter);
        RefreshList();
    }

    private void FilterItems(ItemFilter filter)
    {
        _filteredItems.Clear();
        if (_marketItems == null) return;

        bool isAll = filter == null || 
                     string.IsNullOrEmpty(filter.FilterName) || 
                     filter.FilterName.Equals("All", System.StringComparison.OrdinalIgnoreCase) || 
                     filter.FilterName.Equals("전체", System.StringComparison.OrdinalIgnoreCase);

        foreach (var item in _marketItems)
        {
            if (item == null) continue;

            if (isAll)
            {
                _filteredItems.Add(item);
                continue;
            }

            if (MatchesFilter(item, filter))
            {
                _filteredItems.Add(item);
            }
        }
    }

    private bool MatchesFilter(ItemDataSO item, ItemFilter filter)
    {
        if (filter == null) return true;

        if (item is ItemPotionSO potion)
        {
            SpellElement elem = potion.potionType == PotionType.ElementalResistance 
                ? potion.resistanceElement 
                : SpellElement.None;

            return elem == filter.Element;
        }
        else
        {
            return item.type == filter.Type;
        }
    }

    void ControlCanvasGroup(bool show)
    {
        _canvasGroup.DOKill();
        _canvasGroup.DOFade(show ? 1f : 0f, 0.2f);
        _canvasGroup.blocksRaycasts = show;
        _canvasGroup.interactable = show;
    }

    void ControlRectTransform(bool show, bool goLeft)
    {
        _rectTransform.DOKill();

        float beginX = show ? 300f : 0f;
        float endX = show ? 0f : -300f;

        Vector2 anchoredPos = _rectTransform.anchoredPosition;
        anchoredPos.x = beginX * (goLeft ? 1f : -1f);
        _rectTransform.anchoredPosition = anchoredPos;

        _rectTransform.DOAnchorPosX(endX * (goLeft ? 1f : -1f), 0.2f).SetEase(show ? Ease.OutQuad : Ease.InQuad);
    }

    public void Show(bool goLeft)
    {
        ControlCanvasGroup(true);
        ControlRectTransform(true, goLeft);
    }

    public void Hide(bool goLeft)
    {
        if (_selectedSlot != null)
        {
            _selectedSlot.Deselect(immediate: true);
            _selectedSlot = null;
        }
        if (_infoUI != null)
        {
            _infoUI.Clear();
        }
        ControlCanvasGroup(false);
        ControlRectTransform(false, goLeft);
    }
}