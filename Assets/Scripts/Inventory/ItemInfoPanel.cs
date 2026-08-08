using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemInfoPanel : MonoBehaviour
{
    CanvasGroup _canvasGroup;
    [SerializeField] private Image _infoIconImage;
    [SerializeField] private TMP_Text _infoNameText;
    [SerializeField] private TMP_Text _infoNameTextOnly;
    [SerializeField] private TMP_Text _infoDescriptionText;
    [SerializeField] private TMP_Text _infoStateText;
    [SerializeField] private TMP_Text _clickInfoText;

    [Header("Scroll Info Display")]
    [SerializeField] private Image _spellIconImage;
    [SerializeField] private Image _drawingSampleImage;
    [SerializeField] private RectTransform _drawingContainer;
    [SerializeField] private GameObject _drawingLinePrefab;

    RectTransform _rectTransform;

    private ItemInstance _currentItem;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        Close();
    }

    public void Open()
    {
        _canvasGroup.alpha = 1f;
    }

    public void Close()
    {
        _canvasGroup.alpha = 0f;
    }

    public void Setup(ItemInstance item, bool isEquipped = false, bool showEquipTextForWand = false)
    {
        _currentItem = item;

        if (_infoNameTextOnly.gameObject.activeInHierarchy) _infoNameTextOnly.gameObject.SetActive(false);
        if (_spellIconImage != null) _spellIconImage.gameObject.SetActive(false);
        if (_drawingSampleImage != null) _drawingSampleImage.gameObject.SetActive(false);
        if (_drawingContainer != null)
        {
            _drawingContainer.gameObject.SetActive(false);
            foreach (Transform child in _drawingContainer) Destroy(child.gameObject);
        }

        if (_infoIconImage != null)
        {
            _infoIconImage.gameObject.SetActive(true);
            _infoIconImage.sprite = item.ItemIcon;
        }

        if (_infoNameText != null)
        {
            _infoNameText.gameObject.SetActive(true);
            _infoNameText.text = item.ItemName;
        }

        if (_infoDescriptionText != null)
        {
            _infoDescriptionText.gameObject.SetActive(true);
            _infoDescriptionText.text = item.ItemDescription;
        }

        if (item is Item_Scroll scroll)
        {
            _infoStateText.gameObject.SetActive(true);
            string grade = item.Rarity.ToString();
            _infoNameText.text = _infoNameText.text + $" (내구도: {scroll.currentDurability}/{(scroll.ScrollData != null ? scroll.ScrollData.maxDurability : 5)})";
            if (scroll.isEmpty)
            {
                _infoStateText.text = $"등급: {grade} | 상태: 빈 스크롤";
            }
            else
            {
                string spell = !string.IsNullOrEmpty(scroll.spellName) ? scroll.spellName : "알 수 없는 마법";
                _infoStateText.text = $"등급: {grade} | 마법진: {spell}";

                if (_spellIconImage != null && scroll.spellIcon != null)
                {
                    _spellIconImage.gameObject.SetActive(true);
                    _spellIconImage.sprite = scroll.spellIcon;
                }

                if (scroll.userDrawingData != null && scroll.userDrawingData.strokes.Count > 0)
                {
                    if (_drawingContainer != null && _drawingLinePrefab != null)
                    {
                        _drawingContainer.gameObject.SetActive(true);
                        foreach (var stroke in scroll.userDrawingData.strokes)
                        {
                            GameObject lineObj = Instantiate(_drawingLinePrefab, _drawingContainer);
                            DrawingLine drawingLine = lineObj.GetComponent<DrawingLine>();
                            if (drawingLine != null)
                            {
                                // 미니맵 용도에 맞게 선 두께/색상을 조절할 수 있다면 추가 작업
                                foreach (var p in stroke.points)
                                {
                                    drawingLine.AddPoint(p);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (_drawingSampleImage != null && scroll.ScrollData != null && scroll.ScrollData.sampleImage != null)
                    {
                        _drawingSampleImage.gameObject.SetActive(true);
                        _drawingSampleImage.sprite = scroll.ScrollData.sampleImage;
                    }
                }
            }
        }
        else if (item is Item_Ink ink)
        {
            _infoStateText.gameObject.SetActive(true);
            string grade = item.Rarity.ToString();
            string colorHex = ink.InkData != null ? ColorUtility.ToHtmlStringRGB(ink.InkData.inkColor) : "FFFFFF";
            float maxAmt = ink.InkData != null ? ink.InkData.maxAmount : 100f;
            _infoStateText.text = $"등급: {grade} | 색상: <color=#{colorHex}>■</color> | 남은 양: {((int)ink.currentAmount).ToShortFormat()}/{((int)maxAmt).ToShortFormat()}";
        }
        else if (item is Item_Pen pen)
        {
            _infoStateText.gameObject.SetActive(true);
            string grade = item.Rarity.ToString();
            float maxCap = pen.PenData != null ? pen.PenData.maxInkCapacity : 0f;
            _infoStateText.text = $"등급: {grade} | 잉크 용량: {((int)pen.currentInkCapacity).ToShortFormat()}/{((int)maxCap).ToShortFormat()}";
        }
        else
        {
            _infoStateText.gameObject.SetActive(false);
        }

        if (_clickInfoText != null)
        {
            _clickInfoText.gameObject.SetActive(true);
            
            if (item is Item_Wand && !showEquipTextForWand)
            {
                _clickInfoText.text = "우클릭 : 버리기";
            }
            else
            {
                string actionText = "선택";
                if (StoreManager.Instance != null && StoreManager.Instance.IsOpened)
                {
                    if (item is Item_Scroll) actionText = "<color=orange>선택</color>";
                    else actionText = "<color=gray>선택 불가</color>";
                }
                else
                {
                    if (item is Item_Scroll || item is Item_Ink || item is Item_Pen || item is Item_Wand)
                    {
                        actionText = isEquipped ? "장착 해제" : "장착";
                        _clickInfoText.text = $"더블 클릭 : {actionText} | 우클릭 : 버리기";
                    }
                    else if (item.IsStackable)
                    {
                        actionText = "사용/선택";
                        _clickInfoText.text = $"더블 클릭 : {actionText} | 우클릭 : 1개 버리기\n(마우스 휠로 수량 조절)";
                    }
                    else
                    {
                        _clickInfoText.text = $"더블 클릭 : {actionText} | 우클릭 : 버리기";
                    }
                }
            }
        }

        // 텍스트 내용 변경 후 즉시 크기를 반영하기 위한 강제 갱신
        Canvas.ForceUpdateCanvases();

        if (_infoNameText != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_infoNameText.GetComponent<RectTransform>());
            
        if (_infoDescriptionText != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_infoDescriptionText.GetComponent<RectTransform>());

        if (_infoStateText != null && _infoStateText.gameObject.activeSelf)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_infoStateText.GetComponent<RectTransform>());

        if (_clickInfoText != null && _clickInfoText.gameObject.activeSelf)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_clickInfoText.GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
    }

    public void UpdateStackAmount(int amount)
    {
        if (_currentItem != null && _currentItem.IsStackable && _clickInfoText != null)
        {
            string actionText = "사용/선택";
            if (StoreManager.Instance != null && StoreManager.Instance.IsOpened)
            {
                actionText = "<color=gray>선택 불가</color>";
            }
            _clickInfoText.text = $"더블 클릭 : {actionText} | 우클릭 : {amount}개 버리기\n(마우스 휠로 수량 조절)";
            
            Canvas.ForceUpdateCanvases();
            if (_clickInfoText.gameObject.activeSelf)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_clickInfoText.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }
    }

    public void ClippingPosition(RectTransform slotRect, RectTransform boundaryRect, bool forceAbove = false)
    {
        if (slotRect == null || boundaryRect == null) return;
        
        RectTransform infoRect = _rectTransform;

        LayoutRebuilder.ForceRebuildLayoutImmediate(infoRect);

        Vector3[] slotCorners = new Vector3[4];
        slotRect.GetWorldCorners(slotCorners);
        Vector3 bottomCenter = (slotCorners[0] + slotCorners[3]) / 2f;
        Vector3 topCenter = (slotCorners[1] + slotCorners[2]) / 2f;
        
        Vector3[] invCorners = new Vector3[4];
        boundaryRect.GetWorldCorners(invCorners);
        
        // 상단에서 75% 지점 (하위 25%)을 기준으로 삼음
        float thresholdY = Mathf.Lerp(invCorners[2].y, invCorners[0].y, 0.75f);

        // 슬롯이 해당 기준치보다 아래쪽에 있다면 패널을 슬롯 위(Top)로 표시
        bool showAbove = forceAbove || slotRect.position.y < thresholdY;
        
        Vector3[] initialPanelCorners = new Vector3[4];
        
        if (showAbove)
        {
            infoRect.position = topCenter;
            infoRect.GetWorldCorners(initialPanelCorners);
            Vector3 panelBottomCenter = (initialPanelCorners[0] + initialPanelCorners[3]) / 2f;
            Vector3 pivotToBottomShift = infoRect.position - panelBottomCenter;
            
            infoRect.position = topCenter + pivotToBottomShift;
            infoRect.anchoredPosition += new Vector2(0, 5f);
        }
        else
        {
            infoRect.position = bottomCenter;
            infoRect.GetWorldCorners(initialPanelCorners);
            Vector3 panelTopCenter = (initialPanelCorners[1] + initialPanelCorners[2]) / 2f;
            Vector3 pivotToTopShift = infoRect.position - panelTopCenter;
            
            infoRect.position = bottomCenter + pivotToTopShift;
            infoRect.anchoredPosition += new Vector2(0, -5f);
        }
        
        Vector3[] panelCorners = new Vector3[4];
        infoRect.GetWorldCorners(panelCorners);
        
        Vector3 offset = Vector3.zero;

        // 좌/우 (Width) 보정
        if (panelCorners[0].x < invCorners[0].x)
            offset.x = invCorners[0].x - panelCorners[0].x;
        else if (panelCorners[2].x > invCorners[2].x)
            offset.x = invCorners[2].x - panelCorners[2].x;
            
        infoRect.position += offset;
    }

    public void SetupRecipeInfo(string recipeName)
    {
        if (_infoIconImage != null) _infoIconImage.gameObject.SetActive(false);
        
        if (!_infoNameTextOnly.gameObject.activeInHierarchy) _infoNameTextOnly.gameObject.SetActive(true);
        _infoNameTextOnly.text = recipeName;
        
        if (_infoDescriptionText != null) _infoDescriptionText.gameObject.SetActive(false);
        if (_infoStateText != null) _infoStateText.gameObject.SetActive(false);
        
        if (_clickInfoText != null)
        {
            _clickInfoText.gameObject.SetActive(true);
            _clickInfoText.text = "클릭 : 자세히 보기";
        }

        Canvas.ForceUpdateCanvases();

        if (_infoNameTextOnly != null && _infoNameTextOnly.gameObject.activeSelf)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_infoNameTextOnly.GetComponent<RectTransform>());

        if (_clickInfoText != null && _clickInfoText.gameObject.activeSelf)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_clickInfoText.GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
    }
}
