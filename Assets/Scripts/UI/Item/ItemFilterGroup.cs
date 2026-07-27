using System;
using UnityEngine;

public class ItemFilterGroup : MonoBehaviour
{
    ItemFilter[] _filters;
    [SerializeField] ItemFilter _itemFilterPrefab;
    [SerializeField] bool _filterByItemType = true;
    [SerializeField] string[] _filterNames;
    [SerializeField] ItemType[] _filterTypes;
    [SerializeField] SpellElement[] _filterElements;
    [SerializeField] Sprite _filterActiveSprite;

    public event Action<ItemFilter> OnFilterChanged;
    public ItemFilter CurrentFilter { get; private set; }
    private bool _isInitialized = false;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        int count = _filterNames.Length;
        _filters = new ItemFilter[count];

        for (int i = 0; i < count; i++)
        {
            ItemFilter filter = Instantiate(_itemFilterPrefab, transform);

            ItemType type = _filterTypes[i];
            SpellElement ele = _filterElements[i];

            filter.Setup(_filterNames[i], type, ele, _filterActiveSprite, OnFilterClicked);
            _filters[i] = filter;
        }

        ResetFilter();
    }

    public void ResetFilter()
    {
        SelectFilter(_filters[0], true);
    }

    public void SelectFilter(ItemFilter filter, bool forceNotify = false)
    {
        if (!forceNotify && CurrentFilter == filter) return;

        CurrentFilter = filter;
        if (_filters != null)
        {
            foreach (var f in _filters)
            {
                if (f != null) f.SetActive(f == filter);
            }
        }

        OnFilterChanged?.Invoke(CurrentFilter);
    }

    private void OnFilterClicked(ItemFilter clickedFilter)
    {
        SelectFilter(clickedFilter);
    }
}
