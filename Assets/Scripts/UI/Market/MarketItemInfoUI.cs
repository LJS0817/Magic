using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketItemInfoUI : MonoBehaviour
{
    [SerializeField] TMP_Text _name;
    [SerializeField] TMP_Text _desc;
    [SerializeField] Image _icon;
    [SerializeField] CanvasGroup _infoGroup;
    [SerializeField] Button _descAmount;
    [SerializeField] Button _incAmount;
    [SerializeField] TMP_InputField _inputField;
    [SerializeField] CurrencyValueUI _currency;
    [SerializeField] CustomButton _buyButton;

    private ItemDataSO _itemData;
    private long _unitPrice; // 1개당 단가 (할인 적용 완료된 가격)

    private void Start()
    {
        if (_buyButton != null)
        {
            _buyButton.onClick.RemoveAllListeners();
            _buyButton.onClick.AddListener(OnBuyClicked);
        }

        if (_incAmount != null)
        {
            _incAmount.onClick.RemoveAllListeners();
            _incAmount.onClick.AddListener(OnIncreaseClicked);
        }

        if (_descAmount != null)
        {
            _descAmount.onClick.RemoveAllListeners();
            _descAmount.onClick.AddListener(OnDecreaseClicked);
        }

        if (_inputField != null)
        {
            _inputField.onValueChanged.RemoveAllListeners();
            _inputField.onValueChanged.AddListener(OnInputChanged);
            _inputField.onEndEdit.RemoveAllListeners();
            _inputField.onEndEdit.AddListener(OnInputEndEdit);
        }
    }

    public void SetUp(ItemDataSO item, long price)
    {
        _itemData = item;
        _unitPrice = price; // 이미 MarketManager에서 할인율이 1번 계산되어 넘어온 단가

        _name.text = item == null ? "" : item.itemName;
        _desc.text = item == null ? "" : item.itemDescription;
        if (_icon != null)
        {
            _icon.sprite = item?.itemIcon;
            _icon.enabled = item != null && item.itemIcon != null;
        }

        if (_infoGroup != null)
        {
            _infoGroup.alpha = item == null ? 0f : 1f;
            _infoGroup.blocksRaycasts = item != null;
            _infoGroup.interactable = item != null;
        }

        if (_inputField != null)
        {
            _inputField.text = "1";
        }

        UpdateTotalPrice();
    }

    public void Clear()
    {
        SetUp(null, 0);
    }

    private void OnIncreaseClicked()
    {
        int current = GetCurrentAmount();
        current++;

        int maxCap = 99;
        if (InventoryManager.Instance != null)
        {
            int remainingSpace = InventoryManager.Instance.GetMaxCapacity() - InventoryManager.Instance.items.Count;
            maxCap = Mathf.Clamp(remainingSpace, 1, 99);
        }
        if (current > maxCap) current = maxCap;

        if (_inputField != null) _inputField.text = current.ToString();
        UpdateTotalPrice();
    }

    private void OnDecreaseClicked()
    {
        int current = GetCurrentAmount();
        current--;
        if (current < 1) current = 1;

        if (_inputField != null) _inputField.text = current.ToString();
        UpdateTotalPrice();
    }

    private void OnInputChanged(string text)
    {
        UpdateTotalPrice();
    }

    private void OnInputEndEdit(string text)
    {
        int current = GetCurrentAmount();
        if (_inputField != null) _inputField.text = current.ToString();
        UpdateTotalPrice();
    }

    private int GetCurrentAmount()
    {
        if (_inputField != null && int.TryParse(_inputField.text, out int amount))
        {
            return Mathf.Max(1, amount);
        }
        return 1;
    }

    private void UpdateTotalPrice()
    {
        int amount = GetCurrentAmount();
        long totalPrice = _unitPrice * amount;
        if (_currency != null)
        {
            _currency.SetValue(totalPrice);
        }

        if (transform is RectTransform rt)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
    }

    private void OnBuyClicked()
    {
        if (_itemData != null && MarketManager.Instance != null)
        {
            int amount = GetCurrentAmount();
            bool success = MarketManager.Instance.BuyItem(_itemData, amount);
            if (success)
            {
                // Optional: Play buy sound, show particle, etc.
            }
        }
    }
}
