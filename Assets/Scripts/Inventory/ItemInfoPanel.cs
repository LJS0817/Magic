using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ItemInfoPanel : MonoBehaviour
{
    CanvasGroup _canvasGroup;
    [SerializeField] private Image _infoIconImage;
    [SerializeField] private TMP_Text _infoNameText;
    [SerializeField] private TMP_Text _infoDescriptionText;
    [SerializeField] private TMP_Text _infoStateText;
    [SerializeField] private TMP_Text _infoDurabilityText;
    [SerializeField] private TMP_Text _infoSpellext;
    Image _infoColorImage;
    Image _gradeColorImage;
    [SerializeField] private TMP_Text _clickInfoText;

    [Header("Scroll Info Display")]
    [SerializeField] private Image _spellIconImage;
    [SerializeField] private Image _drawingSampleImage;
    [SerializeField] private RectTransform _drawingContainer;
    [SerializeField] private GameObject _drawingLinePrefab;

    RectTransform _rectTransform;

    private ItemInstance _currentItem;
    DrawingLine drawingLine;
    private Coroutine _rebuildCoroutine;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _infoColorImage = _infoSpellext.transform.parent.GetComponent<Image>();
        _gradeColorImage = _infoStateText.transform.parent.GetComponent<Image>();
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        GameObject lineObj = Instantiate(_drawingLinePrefab, _drawingContainer);
        drawingLine = lineObj.GetComponent<DrawingLine>();
        drawingLine.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
        drawingLine.lineRenderer.widthCurve = AnimationCurve.Constant(0f, 1f, 0.05f);
        drawingLine.lineRenderer.sortingOrder = 11;
        Close();
    }

    public void Open()
    {
        _canvasGroup.alpha = 1f;
    }

    public void Close()
    {
        _canvasGroup.alpha = 0f;
        drawingLine.SetActive(false);
    }

    public void Setup(ItemInstance item, bool isEquipped = false, bool showEquipTextForWand = false)
    {
        _currentItem = item;
        drawingLine.SetActive(false);

        if (_spellIconImage != null) _spellIconImage.gameObject.SetActive(false);
        if (_drawingSampleImage != null) _drawingSampleImage.gameObject.SetActive(false);
        if (_drawingContainer != null)
        {
            _drawingContainer.gameObject.SetActive(false);
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

        string categoryText = "";
        Color categoryColor = Color.white;

        switch (item.Type)
        {
            case ItemType.Scroll:
                categoryText = "스크롤";
                ColorUtility.TryParseHtmlString("#0D47A1", out categoryColor); // 짙은 파랑
                break;
            case ItemType.Pen:
                categoryText = "필기구";
                ColorUtility.TryParseHtmlString("#827717", out categoryColor); // 올리브 (어두운 황록색)
                break;
            case ItemType.Wand:
                categoryText = "주무기";
                ColorUtility.TryParseHtmlString("#B71C1C", out categoryColor); // 짙은 빨강
                break;
            case ItemType.Robe:
                categoryText = "방어구";
                ColorUtility.TryParseHtmlString("#4A148C", out categoryColor); // 짙은 보라
                break;
            case ItemType.Cloak:
                categoryText = "보조 방어구";
                ColorUtility.TryParseHtmlString("#006064", out categoryColor); // 짙은 청록 (청록계열)
                break;
            case ItemType.Pouch:
                categoryText = "보조 장비";
                ColorUtility.TryParseHtmlString("#3E2723", out categoryColor); // 짙은 갈색
                break;
            case ItemType.Potion:
                categoryText = "소모품";
                ColorUtility.TryParseHtmlString("#880E4F", out categoryColor); // 짙은 자홍 (와인색)
                break;
            case ItemType.Material:
                categoryText = "재료";
                ColorUtility.TryParseHtmlString("#E65100", out categoryColor); // 짙은 주황
                break;
            case ItemType.RecipeBook:
                categoryText = "마법진";
                ColorUtility.TryParseHtmlString("#1B5E20", out categoryColor); // 짙은 초록
                break;
            case ItemType.Quest:
                categoryText = "퀘스트";
                ColorUtility.TryParseHtmlString("#212121", out categoryColor); // 짙은 회색
                break;
        }

        string keyAttribute = "";
        if (item is Item_Pen penAttr && penAttr.PenData != null)
        {
            keyAttribute = penAttr.PenData.consumesMana ? "마나 소모" : $"잉크 소모율: {penAttr.PenData.inkConsumptionRate}배";
        }
        else if (item is Item_Wand wandAttr && wandAttr.WandData != null)
        {
            keyAttribute = $"마나 소모 배율: {wandAttr.WandData.defaultManaCostMultiplier}배";
        }
        else if (item is Item_Robe robeAttr && robeAttr.RobeData != null)
        {
            keyAttribute = $"방어력: +{robeAttr.RobeData.bonusDefense} / 최대 마나: +{robeAttr.RobeData.bonusMaxMana}";
        }
        else if (item is Item_Cloak cloakAttr && cloakAttr.CloakData != null)
        {
            keyAttribute = $"방어력: +{cloakAttr.CloakData.bonusDefense} / 최대 마나: +{cloakAttr.CloakData.bonusMaxMana}";
        }
        else if (item is Item_Pouch pouchAttr && pouchAttr.PouchData != null)
        {
            keyAttribute = $"적재 용량: +{pouchAttr.PouchData.loadoutCapacityBonus}";
        }
        else if (item is Item_Potion potionAttr && potionAttr.PotionData != null)
        {
            if (potionAttr.PotionData.potionType == PotionType.ElementalResistance)
                keyAttribute = $"내성: +{potionAttr.PotionData.resistancePercentage * 100}% ({potionAttr.PotionData.resistanceDuration}초)";
            else
                keyAttribute = $"회복량: {potionAttr.PotionData.recoveryAmount}";
        }

        if (_infoDescriptionText != null)
        {
            _infoDescriptionText.gameObject.SetActive(true);
            _infoDescriptionText.text = item.ItemDescription;
            if (!string.IsNullOrEmpty(keyAttribute))
            {
                _infoDescriptionText.text += $"\n\n<color=#FFD700>{keyAttribute}</color>";
            }
        }

        if (_infoStateText != null)
        {
            _infoStateText.gameObject.SetActive(true);
            _infoStateText.text = item.Rarity.ToString();

            if (_gradeColorImage != null)
            {
                Color rarityColor = Color.white;
                switch (item.Rarity)
                {
                    case ItemRarity.Common:
                        ColorUtility.TryParseHtmlString("#757575", out rarityColor); // 회색
                        break;
                    case ItemRarity.Uncommon:
                        ColorUtility.TryParseHtmlString("#388E3C", out rarityColor); // 초록
                        break;
                    case ItemRarity.Rare:
                        ColorUtility.TryParseHtmlString("#1976D2", out rarityColor); // 파랑
                        break;
                    case ItemRarity.Epic:
                        ColorUtility.TryParseHtmlString("#7B1FA2", out rarityColor); // 보라
                        break;
                    case ItemRarity.Legendary:
                        ColorUtility.TryParseHtmlString("#F57F17", out rarityColor); // 황금
                        break;
                }
                _gradeColorImage.color = rarityColor;
            }
        }

        if (_infoColorImage != null && item.Type != ItemType.Ink)
        {
            _infoColorImage.color = categoryColor;
        }

        if (item is Item_Scroll scroll)
        {
            if (_infoSpellext != null) _infoSpellext.color = Color.white;
            _infoDurabilityText.text = $"내구도 : {scroll.currentDurability}/{(scroll.ScrollData != null ? scroll.ScrollData.maxDurability : 5)}";
            if (scroll.isEmpty)
            {
                _infoSpellext.text = $"빈 스크롤";
            }
            else
            {
                string spell = !string.IsNullOrEmpty(scroll.spellName) ? scroll.spellName : "알 수 없는 마법";
                _infoSpellext.text = $"{spell}";

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
                        
                        drawingLine.SetColor(scroll.drawnInkColor);

                        drawingLine.SetActive(true);
                        drawingLine.Clear();
                        foreach (var stroke in scroll.userDrawingData.strokes)
                        {
                            // 미니맵 용도에 맞게 선 두께/색상을 조절할 수 있다면 추가 작업
                            foreach (var p in stroke.points)
                            {
                                drawingLine.AddPoint(p);
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
            float maxAmt = ink.InkData != null ? ink.InkData.maxAmount : 100f;
            _infoSpellext.text = "";
            _infoColorImage.color = ink.InkData.inkColor;
            _infoDurabilityText.text = $"남은 양: {((int)ink.currentAmount).ToShortFormat()}/{((int)maxAmt).ToShortFormat()}";
        }
        else if (item is Item_Pen pen)
        {
            if (_infoSpellext != null)
            {
                _infoSpellext.gameObject.SetActive(true);
                _infoSpellext.text = categoryText;
                _infoSpellext.color = Color.white;
            }
            float maxCap = pen.PenData != null ? pen.PenData.maxInkCapacity : 0f;
            _infoDurabilityText.text = $"잉크 용량: {((int)pen.currentInkCapacity).ToShortFormat()}/{((int)maxCap).ToShortFormat()}";
        }
        else
        {
            if (_infoSpellext != null)
            {
                _infoSpellext.gameObject.SetActive(true);
                _infoSpellext.text = categoryText;
                _infoSpellext.color = Color.white;
            }
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

        RebuildLayout();
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
            
            RebuildLayout();
        }
    }


    public void SetupRecipeInfo(string recipeName)
    {
        if (_infoIconImage != null) _infoIconImage.gameObject.SetActive(false);
        
        if (!_infoNameText.gameObject.activeInHierarchy) _infoNameText.gameObject.SetActive(true);
        _infoNameText.text = recipeName;
        
        if (_infoDescriptionText != null) _infoDescriptionText.gameObject.SetActive(false);
        if (_infoStateText != null) _infoStateText.gameObject.SetActive(false);
        
        if (_clickInfoText != null)
        {
            _clickInfoText.gameObject.SetActive(true);
            _clickInfoText.text = "클릭 : 자세히 보기";
        }

        RebuildLayout();
    }

    private void RebuildLayout()
    {
        if (_rebuildCoroutine != null)
        {
            StopCoroutine(_rebuildCoroutine);
        }
        _rebuildCoroutine = StartCoroutine(RebuildLayoutRoutine());
    }

    private System.Collections.IEnumerator RebuildLayoutRoutine()
    {
        yield return new WaitForEndOfFrame();

        if (_infoNameText != null && _infoNameText.gameObject.activeSelf) _infoNameText.ForceMeshUpdate();
        if (_infoDescriptionText != null && _infoDescriptionText.gameObject.activeSelf) _infoDescriptionText.ForceMeshUpdate();
        if (_infoStateText != null && _infoStateText.gameObject.activeSelf) _infoStateText.ForceMeshUpdate();
        if (_infoSpellext != null && _infoSpellext.gameObject.activeSelf) _infoSpellext.ForceMeshUpdate();
        if (_infoDurabilityText != null && _infoDurabilityText.gameObject.activeSelf) _infoDurabilityText.ForceMeshUpdate();
        if (_clickInfoText != null && _clickInfoText.gameObject.activeSelf) _clickInfoText.ForceMeshUpdate();

        Canvas.ForceUpdateCanvases();

        if (_infoNameText != null && _infoNameText.gameObject.activeSelf)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_infoNameText.GetComponent<RectTransform>());
            
        if (_infoDescriptionText != null && _infoDescriptionText.gameObject.activeSelf)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_infoDescriptionText.GetComponent<RectTransform>());

        if (_infoStateText != null && _infoStateText.gameObject.activeSelf)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_infoStateText.GetComponent<RectTransform>());

        if (_infoSpellext != null && _infoSpellext.gameObject.activeSelf)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_infoSpellext.GetComponent<RectTransform>());
            if (_infoSpellext.transform.parent != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_infoSpellext.transform.parent.GetComponent<RectTransform>());
        }

        if (_infoDurabilityText != null && _infoDurabilityText.gameObject.activeSelf)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_infoDurabilityText.GetComponent<RectTransform>());

        if (_clickInfoText != null && _clickInfoText.gameObject.activeSelf)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_clickInfoText.GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
    }
}
