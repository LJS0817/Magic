using UnityEngine;
using UnityEngine.UI;

namespace Magic.Inventory
{
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] private Image _bg;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _slotButton;

        private ItemInstance _item;
        private System.Action<ItemInstance> _onClickCallback;

        public void Initialize(System.Action<ItemInstance> onClickCallback)
        {
            _onClickCallback = onClickCallback;
            if (_slotButton != null)
            {
                _slotButton.onClick.RemoveAllListeners();
                _slotButton.onClick.AddListener(OnSlotClicked);
            }
        }

        public void SetInfo(ItemInstance item, Sprite slotBg, Sprite emptyIcon)
        {
            if (_bg != null) _bg.sprite = slotBg;
            _item = item;
            
            if (_iconImage != null)
            {
                if (_item != null && _item.ItemIcon != null)
                {
                    _iconImage.sprite = _item.ItemIcon;
                    _iconImage.gameObject.SetActive(true);
                }
                else
                {
                    if (emptyIcon != null)
                    {
                        _iconImage.sprite = emptyIcon;
                        _iconImage.gameObject.SetActive(true);
                    }
                    else
                    {
                        _iconImage.sprite = null;
                        _iconImage.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void OnSlotClicked()
        {
            if (_item != null)
            {
                _onClickCallback?.Invoke(_item);
            }
        }
    }
}
