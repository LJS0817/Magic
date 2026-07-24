using UnityEngine;
using UnityEngine.UI;

public class EquippedItemUI : MonoBehaviour
{
    [Header("Equipped Scroll")]
    [SerializeField] private Image _scrollImage;
    [SerializeField] private GameObject _scrollEmptyGo; // 스크롤이 없을 때 보여줄 기본 UI (옵션)
    private FloatingEffect _scrollFloatingEffect;

    [Header("Equipped Pen")]
    [SerializeField] private Image _penImage;
    [SerializeField] private GameObject _penEmptyGo; // 펜이 없을 때 보여줄 기본 UI (옵션)

    [Header("Equipped Ink (4 Parts Assembly)")]
    [SerializeField] private CanvasGroup _inkContainer; // 잉크 전체 컨테이너
    [SerializeField] private Image _inkBaseLayer;
    [SerializeField] private Image _inkLiquidLayer;
    [SerializeField] private Image _inkTopBackLayer;
    [SerializeField] private Image _inkTopLayer;
    [SerializeField] private GameObject _inkEmptyGo; // 잉크가 없을 때 보여줄 기본 UI (옵션)
    private FloatingEffect _inkFloatingEffect;

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnScrollEquipped += UpdateScroll;
            InventoryManager.Instance.OnPenEquipped += UpdatePen;
            InventoryManager.Instance.OnInkEquipped += UpdateInk;

            _scrollFloatingEffect = _scrollImage.GetComponent<FloatingEffect>();
            _inkFloatingEffect = _inkContainer.GetComponent<FloatingEffect>();

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
        if (scroll != null)
        {
            if (_scrollImage != null)
            {
                _scrollImage.sprite = scroll.ItemIcon;
                _scrollImage.gameObject.SetActive(true);
                _scrollImage.enabled = true;

                _scrollFloatingEffect.StopFloating();
                _scrollFloatingEffect.RestartFloatingEffect();
            }
            if (_scrollEmptyGo != null) _scrollEmptyGo.SetActive(false);
        }
        else
        {
            if (_scrollImage != null) _scrollImage.enabled = false;
            if (_scrollEmptyGo != null) _scrollEmptyGo.SetActive(true);
        }
    }

    private void UpdatePen(Item_Pen pen)
    {
        if (pen != null)
        {
            if (_penImage != null)
            {
                _penImage.sprite = pen.ItemIcon;
                _penImage.enabled = true;
            }
            if (_penEmptyGo != null) _penEmptyGo.SetActive(false);
        }
        else
        {
            if (_penImage != null) _penImage.enabled = false;
            if (_penEmptyGo != null) _penEmptyGo.SetActive(true);
        }

        if (pen != null && pen.PenData != null && pen.PenData.consumesMana)
        {
            if (_inkContainer != null) _inkContainer.alpha = 0f;
        }
        else
        {
            if (_inkContainer != null && InventoryManager.Instance.EquippedInk != null) _inkContainer.alpha = 1f;
        }

        UpdateInkFloatingState();
    }

    private void UpdateInk(Item_Ink ink)
    {
        if (ink != null)
        {
            var currentPen = InventoryManager.Instance.EquippedPen;
            if (currentPen != null && currentPen.PenData != null && currentPen.PenData.consumesMana)
            {
                if (_inkContainer != null) _inkContainer.alpha = 0f;
            }
            else
            {
                if (_inkContainer != null) _inkContainer.alpha = 1f;
            }
            if (_inkEmptyGo != null) _inkEmptyGo.SetActive(false);
            
            var data = ink.InkData;
            
            if (_inkBaseLayer != null) _inkBaseLayer.sprite = data.inkBaseSprite;
            
            if (_inkLiquidLayer != null)
            {
                if (data.inkLiquidSprite != null) _inkLiquidLayer.sprite = data.inkLiquidSprite;
                _inkLiquidLayer.color = data.inkLiquidColorAlpha;
            }
            
            if (_inkTopBackLayer != null) if (data.inkTopBackSprite != null) _inkTopBackLayer.sprite = data.inkTopBackSprite;
            
            if (_inkTopLayer != null) if (data.inkTopSprite != null) _inkTopLayer.sprite = data.inkTopSprite;
        }
        else
        {
            if (_inkContainer != null) _inkContainer.alpha = 0;
            if (_inkEmptyGo != null) _inkEmptyGo.SetActive(true);
        }

        UpdateInkFloatingState();
    }

    private void UpdateInkFloatingState()
    {
        if (InventoryManager.Instance.EquippedPen != null || InventoryManager.Instance.EquippedInk != null) _inkFloatingEffect.RestartFloatingEffect();
        else _inkFloatingEffect.PauseFloating();
    }
}

