using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemFilter : MonoBehaviour
{
    [SerializeField] TMP_Text _filterNameText;
    [SerializeField] SlotCustomButton _button;
    [SerializeField] Image _filterActiveSprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void Setup(string filterName, Sprite activeSprite)
    {
        _filterNameText.SetText(filterName);
        _filterActiveSprite.sprite = activeSprite;

        _button.onClick.AddListener(onClick);
    }

    public void onClick()
    {
        //Onclick event for filting items
    }
}
