using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemFilter : MonoBehaviour
{
    [SerializeField] TMP_Text _filterNameText;
    [SerializeField] SlotCustomButton _button;
    [SerializeField] Image _filterActiveSprite;
    [SerializeField] ItemType _type;
    [SerializeField] SpellElement _ele;

    public string FilterName { get; private set; }
    public ItemType Type => _type;
    public SpellElement Element => _ele;
    private Action<ItemFilter> _onFilterClicked;

    public void Setup(string filterName, ItemType type, SpellElement ele, Sprite activeSprite, Action<ItemFilter> onFilterClicked = null)
    {
        FilterName = filterName;
        if (_filterNameText != null && !string.IsNullOrEmpty(filterName)) _filterNameText.SetText(filterName);
        if (_filterActiveSprite != null && activeSprite != null) _filterActiveSprite.sprite = activeSprite;

        _type = type;
        _ele = ele;

        _onFilterClicked = onFilterClicked;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(onClick);
    }

    public void SetActive(bool isActive)
    {
        if (isActive) _button.Select();
        else _button.Deselect();
    }

    public void onClick()
    {
        _onFilterClicked?.Invoke(this);
    }
}
