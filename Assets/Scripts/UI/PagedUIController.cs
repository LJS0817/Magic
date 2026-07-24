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

    protected virtual void Awake()
    {
        if (_prevPageButton != null) _prevPageButton.onClick.AddListener(OnPrevPage);
        if (_nextPageButton != null) _nextPageButton.onClick.AddListener(OnNextPage);
    }

    protected virtual void OnEnable()
    {
        RefreshList();
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
        
        return slot;
    }

    // Subclasses must implement these
    protected abstract int GetTotalCapacity();
    protected abstract void UpdateSlot(TSlot slot, int dataIndex);
}

