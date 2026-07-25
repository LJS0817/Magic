using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public enum MarketType
{
    WandShop,
    InkAndPenShop,
    ParchmentShop,
    AdventurerGuild
}

public class MarketUI : PagedUIController<MarketItemSlotUI>
{
    [SerializeField] CanvasGroup _canvasGroup;
    RectTransform _rectTransform;
    public MarketType marketType;
    private List<ItemDataSO> _marketItems = new List<ItemDataSO>();

    public void Setup(List<ItemDataSO> items)
    {
        _marketItems = items;
        RefreshList();
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }

    protected override int GetTotalCapacity()
    {
        return _marketItems != null ? _marketItems.Count : 0;
    }

    protected override void UpdateSlot(MarketItemSlotUI slot, int dataIndex)
    {
        if (_marketItems != null && dataIndex >= 0 && dataIndex < _marketItems.Count)
        {
            slot.Setup(_marketItems[dataIndex]);
        }
        else
        {
            slot.gameObject.SetActive(false);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    void ControlCanvasGroup(bool show)
    {
        _canvasGroup.DOKill();
        _canvasGroup.DOFade(show ? 1f : 0f, 0.2f);
        _canvasGroup.blocksRaycasts = show;
        _canvasGroup.interactable = show;
    }

    void ControlRectTransform(bool show, bool goLeft)
    {
        _rectTransform.DOKill();

        float beginX = show ? 300f : 0f;
        float endX = show ? 0f : -300f;

        Vector2 anchoredPos = _rectTransform.anchoredPosition;
        anchoredPos.x = beginX * (goLeft ? 1f : -1f);
        _rectTransform.anchoredPosition = anchoredPos;

        _rectTransform.DOAnchorPosX(endX * (goLeft ? 1f : -1f), 0.2f).SetEase(show ? Ease.OutQuad : Ease.InQuad);
    }

    public void Show(bool goLeft)
    {
        ControlCanvasGroup(true);
        ControlRectTransform(true, goLeft);
    }

    public void Hide(bool goLeft)
    {
        ControlCanvasGroup(false);
        ControlRectTransform(false, goLeft);
    }
}