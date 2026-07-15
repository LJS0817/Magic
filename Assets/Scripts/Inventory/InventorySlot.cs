using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Magic.Inventory
{
    public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image _bg;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _slotButton;
        [SerializeField] private GameObject _equippedMark;

        private ItemInstance _item;
        public ItemInstance Item => _item;
        private System.Action<InventorySlot> _onClickCallback;
        private System.Action<InventorySlot> _onEnterCallback;
        private System.Action<InventorySlot> _onExitCallback;
        private System.Action<InventorySlot> _onDoubleClickCallback;
        private System.Action<InventorySlot> _onRightClickCallback;
        private float _lastClickTime = 0f;

        public void Initialize(System.Action<InventorySlot> onClickCallback, 
                               System.Action<InventorySlot> onEnterCallback = null, 
                               System.Action<InventorySlot> onExitCallback = null,
                               System.Action<InventorySlot> onDoubleClickCallback = null,
                               System.Action<InventorySlot> onRightClickCallback = null)
        {
            _onClickCallback = onClickCallback;
            _onEnterCallback = onEnterCallback;
            _onExitCallback = onExitCallback;
            _onDoubleClickCallback = onDoubleClickCallback;
            _onRightClickCallback = onRightClickCallback;
            if (_slotButton != null)
            {
                _slotButton.onClick.RemoveAllListeners();
                _slotButton.onClick.AddListener(OnSlotClicked);
            }
        }

        public void SetInfo(ItemInstance item, Sprite slotBg, Sprite emptyIcon, bool isEquipped = false)
        {
            if (_bg != null) _bg.sprite = slotBg;
            _item = item;
            
            if (_item != null)
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

            if (_equippedMark != null)
            {
                _equippedMark.SetActive(isEquipped);
            }
        }

        private void OnSlotClicked()
        {
            if (_item != null)
            {
                if (Time.unscaledTime - _lastClickTime < 0.3f)
                {
                    _onDoubleClickCallback?.Invoke(this);
                    _lastClickTime = 0f;
                }
                else
                {
                    _onClickCallback?.Invoke(this);
                    _lastClickTime = Time.unscaledTime;
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_item != null)
            {
                _onEnterCallback?.Invoke(this);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_item != null)
            {
                _onExitCallback?.Invoke(this);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (_item != null)
                {
                    _onRightClickCallback?.Invoke(this);
                }
            }
        }
    }
}
