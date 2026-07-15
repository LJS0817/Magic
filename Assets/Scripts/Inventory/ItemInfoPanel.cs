using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Magic.Utility;

namespace Magic.Inventory
{
    public class ItemInfoPanel : MonoBehaviour
    {
        CanvasGroup _canvasGroup;
        [SerializeField] private Image _infoIconImage;
        [SerializeField] private TMP_Text _infoNameText;
        [SerializeField] private TMP_Text _infoDescriptionText;
        [SerializeField] private TMP_Text _infoStateText;
        [SerializeField] private TMP_Text _clickInfoText;
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

        public void Setup(ItemInstance item, bool isEquipped = false)
        {
            _currentItem = item;

            if (_infoIconImage != null)
                _infoIconImage.sprite = item.ItemIcon;

            if (_infoNameText != null)
                _infoNameText.text = item.ItemName;

            if (_infoDescriptionText != null)
                _infoDescriptionText.text = item.ItemDescription;

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
                    string spell = (scroll.ScrollData != null && !string.IsNullOrEmpty(scroll.ScrollData.spellName)) ? scroll.ScrollData.spellName : "알 수 없는 마법";
                    _infoStateText.text = $"등급: {grade} | 마법진: {spell}";
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
                string actionText = "선택";
                if (item is Item_Scroll || item is Item_Ink || item is Item_Pen)
                {
                    actionText = isEquipped ? "장착 해제" : "장착";
                }
                _clickInfoText.text = $"더블 클릭 : {actionText} | 우클릭 : 버리기";
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
    }
}