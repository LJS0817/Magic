using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public abstract class PagedUIController<TSlot> : MonoBehaviour where TSlot : MonoBehaviour
{
    [Header("Container Settings")]
    [SerializeField] protected Transform _slotContainer;
    [SerializeField] protected GameObject _slotPrefab;

    [Header("Pagination")]
    [SerializeField] protected int _slotsPerPage = 20;
    [SerializeField] protected Button _prevPageButton;
    [SerializeField] protected Button _nextPageButton;
    [SerializeField] protected TMP_Text _pageText;

    protected List<TSlot> _slotPool = new List<TSlot>();
    protected int _currentPage = 0;

    [Header("Slot Background & Info (Common)")]
    [SerializeField] protected List<Sprite> _slotBackgrounds;
    [SerializeField] protected Sprite _emptySlotIcon;
    [SerializeField] protected ItemInfoPanel _infoPanel;
    [SerializeField] protected RectTransform _inventoryPanel;
    
    protected Dictionary<ItemInstance, Sprite> _itemBackgroundMap = new Dictionary<ItemInstance, Sprite>();
    protected int _currentSelectedAmount = 1;

    protected Sprite GetSlotBackground(ItemInstance item)
    {
        if (_slotBackgrounds == null || _slotBackgrounds.Count == 0) return null;
        if (item == null) return _slotBackgrounds[0];
        if (_itemBackgroundMap.TryGetValue(item, out Sprite bg)) return bg;
        
        Sprite newBg = _slotBackgrounds[Random.Range(0, _slotBackgrounds.Count)];
        _itemBackgroundMap[item] = newBg;
        return newBg;
    }

    protected virtual int GetHoveredItemMaxCount() { return 1; }
    protected virtual void OnQuantityChanged(int amount) 
    { 
        if (_infoPanel != null)
        {
            _infoPanel.UpdateStackAmount(amount);
        }
    }

    protected virtual void Update()
    {
        int maxCount = GetHoveredItemMaxCount();
        if (maxCount > 1)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                if (scroll > 0) _currentSelectedAmount++;
                else _currentSelectedAmount--;

                _currentSelectedAmount = Mathf.Clamp(_currentSelectedAmount, 1, maxCount);
                OnQuantityChanged(_currentSelectedAmount);
            }
        }
    }

    protected virtual void Awake()
    {
        if (_prevPageButton != null) _prevPageButton.onClick.AddListener(OnPrevPage);
        if (_nextPageButton != null) _nextPageButton.onClick.AddListener(OnNextPage);
    }

    protected virtual void OnEnable()
    {
        // UI 창이 켜질 때마다 강제로 전체 목록을 갱신하지 않고, 
        // 데이터가 변경되었을 때 이벤트(event)를 통해서만 갱신하도록 구조 변경
    }

    public virtual void RefreshList()
    {
        int maxCapacity = GetTotalCapacity();
        int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)maxCapacity / _slotsPerPage));
        
        if (_currentPage >= totalPages) _currentPage = totalPages - 1;
        if (_currentPage < 0) _currentPage = 0;

        int startIndex = _currentPage * _slotsPerPage;
        int endIndex = Mathf.Min(startIndex + _slotsPerPage, maxCapacity);
        int displayCount = endIndex - startIndex;

        for (int i = 0; i < displayCount; i++)
        {
            TSlot slot = GetOrCreateSlot(i);
            slot.gameObject.SetActive(true);
            
            int dataIndex = startIndex + i;
            UpdateSlot(slot, dataIndex);
        }

        for (int i = displayCount; i < _slotPool.Count; i++)
        {
            _slotPool[i].gameObject.SetActive(false);
        }

        if (_slotContainer is RectTransform rectTransform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        if (_pageText != null) _pageText.text = $"{_currentPage + 1} / {totalPages}";
        if (_prevPageButton != null) _prevPageButton.interactable = _currentPage > 0;
        if (_nextPageButton != null) _nextPageButton.interactable = _currentPage < totalPages - 1;
    }

    protected virtual void OnPrevPage()
    {
        if (_currentPage > 0)
        {
            _currentPage--;
            RefreshList();
        }
    }

    protected virtual void OnNextPage()
    {
        int maxCapacity = GetTotalCapacity();
        int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)maxCapacity / _slotsPerPage));
        
        if (_currentPage < totalPages - 1)
        {
            _currentPage++;
            RefreshList();
        }
    }

    protected TSlot GetOrCreateSlot(int index)
    {
        if (index < _slotPool.Count)
        {
            return _slotPool[index];
        }

        GameObject slotObj = Instantiate(_slotPrefab, _slotContainer);
        TSlot slot = slotObj.GetComponent<TSlot>();
        _slotPool.Add(slot);
        OnSlotCreated(slot);
        
        return slot;
    }

    protected virtual void OnSlotCreated(TSlot slot) { }

    // Subclasses must implement these
    protected abstract int GetTotalCapacity();
    protected abstract void UpdateSlot(TSlot slot, int dataIndex);
}

