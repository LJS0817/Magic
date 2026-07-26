using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketItemSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image iconImage;
    public TMP_Text nameText;
    public SlotCustomButton getDetail;
    [SerializeField] CurrencyValueUI valueUI;
    [SerializeField] Image focusImg;

    private ItemDataSO _itemData;

    public void SetFocusImage(Sprite sprite)
    {
        if (focusImg != null && sprite != null)
        {
            focusImg.sprite = sprite;
        }
    }

    public void Setup(ItemDataSO itemData, Action<ItemDataSO, long> callback)
    {
        _itemData = itemData;

        if (_itemData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (iconImage != null && _itemData.itemIcon != null)
        {
            iconImage.sprite = _itemData.itemIcon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        if (nameText != null)
        {
            nameText.text = _itemData.itemName;
        }

        long price = 100;
        if (MarketManager.Instance != null)
        {
            price = MarketManager.Instance.GetItemPrice(_itemData);
        }
        else if (_itemData.basePriceInCopper > 0)
        {
            price = _itemData.basePriceInCopper;
        }

        valueUI.SetValue(price);

        if (getDetail != null)
        {
            getDetail.Deselect(immediate: true);
            getDetail.onClick.RemoveAllListeners();
            getDetail.onClick.AddListener(() => { callback(_itemData, price); });
        }
    }

    public void Select()
    {
        if (getDetail != null) getDetail.Select();
    }

    public void Deselect(bool immediate = false)
    {
        if (getDetail != null) getDetail.Deselect(immediate);
    }
}

