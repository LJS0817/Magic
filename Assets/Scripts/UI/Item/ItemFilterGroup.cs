using UnityEngine;

public class ItemFilterGroup : MonoBehaviour
{
    ItemFilter[] _filters;
    [SerializeField] ItemFilter _itemFilterPrefab;
    [SerializeField] bool _isContainAllFilter = true;
    [SerializeField] bool _filterByItemType = true;
    [SerializeField] string[] _filterNames;
    [SerializeField] Sprite _filterActiveSprite;


    void Start()
    {
        Init();
    }

    void Init()
    {
        _filters = new ItemFilter[_filterNames.Length + 1];
        for (int i = 0; i < _filterNames.Length + 1; i++)
        {
            ItemFilter filter = Instantiate(_itemFilterPrefab, transform);
            filter.Setup((i > 0 || !_isContainAllFilter) ? _filterNames[i - 1] : "All", _filterActiveSprite);
            _filters[i] = filter;
        }
    }
}
