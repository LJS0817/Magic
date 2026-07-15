using UnityEngine;
using UnityEngine.UI;
using Magic.Inventory;

namespace Magic.UI
{
    public class EquippedItemUI : MonoBehaviour
    {
        [Header("Equipped Scroll")]
        [SerializeField] private Image _scrollImage;
        [SerializeField] private GameObject _scrollEmptyGo; // 스크롤이 없을 때 보여줄 기본 UI (옵션)

        [Header("Equipped Pen")]
        [SerializeField] private Image _penImage;
        [SerializeField] private GameObject _penEmptyGo; // 펜이 없을 때 보여줄 기본 UI (옵션)

        [Header("Equipped Ink (4 Parts Assembly)")]
        [SerializeField] private GameObject _inkContainer; // 잉크 전체 컨테이너
        [SerializeField] private Image _inkBaseLayer;
        [SerializeField] private Image _inkLiquidLayer;
        [SerializeField] private Image _inkLabelLayer;
        [SerializeField] private Image _inkCapLayer;
        [SerializeField] private GameObject _inkEmptyGo; // 잉크가 없을 때 보여줄 기본 UI (옵션)

        private void Start()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnScrollEquipped += UpdateScroll;
                InventoryManager.Instance.OnPenEquipped += UpdatePen;
                InventoryManager.Instance.OnInkEquipped += UpdateInk;
                
                // 초기 상태 동기화
                UpdateScroll(InventoryManager.Instance.EquippedScroll);
                UpdatePen(InventoryManager.Instance.EquippedPen);
                UpdateInk(InventoryManager.Instance.EquippedInk);
            }
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnScrollEquipped -= UpdateScroll;
                InventoryManager.Instance.OnPenEquipped -= UpdatePen;
                InventoryManager.Instance.OnInkEquipped -= UpdateInk;
            }
        }

        private void UpdateScroll(Item_Scroll scroll)
        {
            if (scroll != null && scroll.ItemIcon != null)
            {
                if (_scrollImage != null)
                {
                    _scrollImage.sprite = scroll.ItemIcon;
                    _scrollImage.gameObject.SetActive(true);
                }
                if (_scrollEmptyGo != null) _scrollEmptyGo.SetActive(false);
            }
            else
            {
                if (_scrollImage != null) _scrollImage.gameObject.SetActive(false);
                if (_scrollEmptyGo != null) _scrollEmptyGo.SetActive(true);
            }
        }

        private void UpdatePen(Item_Pen pen)
        {
            if (pen != null && pen.ItemIcon != null)
            {
                if (_penImage != null)
                {
                    _penImage.sprite = pen.ItemIcon;
                    _penImage.gameObject.SetActive(true);
                }
                if (_penEmptyGo != null) _penEmptyGo.SetActive(false);
            }
            else
            {
                if (_penImage != null) _penImage.gameObject.SetActive(false);
                if (_penEmptyGo != null) _penEmptyGo.SetActive(true);
            }
        }

        private void UpdateInk(Item_Ink ink)
        {
            if (ink != null && ink.InkData != null)
            {
                if (_inkContainer != null) _inkContainer.SetActive(true);
                if (_inkEmptyGo != null) _inkEmptyGo.SetActive(false);
                
                var data = ink.InkData;
                
                if (_inkBaseLayer != null)
                {
                    if (data.inkBaseSprite != null) _inkBaseLayer.sprite = data.inkBaseSprite;
                    _inkBaseLayer.gameObject.SetActive(_inkBaseLayer.sprite != null);
                }
                
                if (_inkLiquidLayer != null)
                {
                    if (data.inkLiquidSprite != null) _inkLiquidLayer.sprite = data.inkLiquidSprite;
                    _inkLiquidLayer.color = data.inkColor;
                    _inkLiquidLayer.gameObject.SetActive(_inkLiquidLayer.sprite != null);
                }
                
                if (_inkLabelLayer != null)
                {
                    if (data.inkLabelSprite != null) _inkLabelLayer.sprite = data.inkLabelSprite;
                    _inkLabelLayer.gameObject.SetActive(_inkLabelLayer.sprite != null);
                }
                
                if (_inkCapLayer != null)
                {
                    if (data.inkCapSprite != null) _inkCapLayer.sprite = data.inkCapSprite;
                    _inkCapLayer.gameObject.SetActive(_inkCapLayer.sprite != null);
                }
            }
            else
            {
                if (_inkContainer != null) _inkContainer.SetActive(false);
                if (_inkEmptyGo != null) _inkEmptyGo.SetActive(true);
            }
        }
    }
}
