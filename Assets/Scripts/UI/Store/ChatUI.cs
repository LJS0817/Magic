using UnityEngine;
using UnityEngine.UI;
using TMPro; // Assuming TextMeshPro is used for UI text
using System.Collections.Generic;

namespace Magic.Store
{
    [RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
    public class ChatUI : MonoBehaviour
    {
        [Header("Fixed Chat UI")]
        [SerializeField] private TMP_Text initialMessageText;
        [SerializeField] private HaggleChatUI[] haggleTexts; // Size 3
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text itemText;

        [Header("Layout Rebuild")]
        [SerializeField] private RectTransform contentLayoutRect;

        [Header("Haggle Input")]
        [SerializeField] private TMP_InputField platinumInput;
        [SerializeField] private TMP_InputField goldInput;
        [SerializeField] private TMP_InputField silverInput;
        [SerializeField] private TMP_InputField copperInput;

        private CanvasGroup _canvasGroup;
        private OrderUI _currentOrderUI;
        private Magic.Inventory.ItemInstance _selectedItem;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            
            // Start hidden
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void OpenChat(OrderUI orderUI)
        {
            _currentOrderUI = orderUI;
            CustomerOrder data = _currentOrderUI.OrderData;

            PopulateChatHistory(data);

            // Enable interactions immediately
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        private void PopulateChatHistory(CustomerOrder data)
        {
            if (data.chatHistory == null || data.chatHistory.Count == 0) return;

            // 1. Initial Message
            if (initialMessageText != null)
            {
                initialMessageText.text = data.chatHistory[0];
                initialMessageText.gameObject.SetActive(true);
            }

            // 2. Hide all Haggle UI initially
            for (int i = 0; i < haggleTexts.Length; i++)
            {
                if (haggleTexts[i] != null) haggleTexts[i].Close();
            }
            if (resultText != null) resultText.gameObject.SetActive(false);

            // 3. Assign Haggle Attempts
            int historyIndex = 1;
            int haggleIndex = 0;

            while (historyIndex < data.chatHistory.Count)
            {
                if (data.chatHistory[historyIndex].StartsWith("플레이어:"))
                {
                    string deal = data.chatHistory[historyIndex];
                    historyIndex++;
                    
                    if (historyIndex < data.chatHistory.Count)
                    {
                        // 손님의 응답 텍스트는 UI에 표시하지 않지만, chatHistory 인덱스는 건너뜁니다.
                        historyIndex++;
                    }
                    
                    bool isRejected = true;
                    // 현재 파싱 중인 흥정이 마지막 흥정이고, 상태가 Pending이거나 Completed라면 성공한 흥정
                    if (historyIndex >= data.chatHistory.Count && (data.state == OrderState.Pending || data.state == OrderState.Completed))
                    {
                        isRejected = false;
                    }

                    haggleTexts[haggleIndex].Show(deal, isRejected);
                    haggleIndex++;
                }
                else
                {
                    resultText.text = data.chatHistory[historyIndex];
                    historyIndex++;
                }
            }

            itemText.gameObject.SetActive(false);

            // Force rebuild layout for ContentSizeFitter and VerticalLayoutGroup
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentLayoutRect);
        }

        public void CloseChat()
        {
            if(_canvasGroup.alpha == 0f) return;
            // Disable interactions immediately
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0f;

            _currentOrderUI = null;
            _selectedItem = null;
            if (itemText != null) itemText.gameObject.SetActive(false);
        }

        public void SelectItem(Magic.Inventory.ItemInstance item)
        {
            if (_currentOrderUI.OrderData == null || item == null) return;
            if(_currentOrderUI.OrderData.state != OrderState.Pending && _currentOrderUI.OrderData.state != OrderState.Haggling)
            {
                itemText.gameObject.SetActive(false);
                return;
            }

            if (item is Magic.Inventory.Item_Scroll scroll)
            {
                _selectedItem = scroll;
                string grade = scroll.Rarity.ToString();
                string spell = (scroll.ScrollData != null && scroll.ScrollData.spellName != "") ? scroll.ScrollData.spellName : "알 수 없는 마법";
                if (itemText != null)
                {
                    itemText.text = $"[선택됨] {item.ItemName}\n마법: {spell}\n등급: {grade}";
                    itemText.gameObject.SetActive(true);
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentLayoutRect);
        }

        public void OnDeliverButtonClicked()
        {
            if (_currentOrderUI == null || _selectedItem == null) 
            {
                Debug.LogWarning("주문이 없거나 선택된 아이템이 없습니다.");
                return;
            }

            // StoreManager에 주문 배송 처리 요청
            StoreManager.Instance.DeliverOrder(_currentOrderUI.OrderData.orderID, _selectedItem);
            
            // 배송 후 창 닫기
            CloseChat();
        }

        public void OnCompleteOrderClicked()
        {
            if (_currentOrderUI != null)
            {
                OrderUI temp = _currentOrderUI;
                CloseChat();
                temp.CompleteOrder();
            }
        }

        public void OnFailOrderClicked()
        {
            if (_currentOrderUI != null)
            {
                OrderUI temp = _currentOrderUI;
                CloseChat();
                temp.FailOrder();
            }
        }

        public void OnSubmitHaggleClicked()
        {
            if (_currentOrderUI == null) return;

            CustomerOrder data = _currentOrderUI.OrderData;
            
            // 인내심(patience)을 기준으로 현재 몇 번째 흥정 시도인지 계산 (100 -> 0, 66 -> 1, 32 -> 2)

            // 흥정 가능한 상태인지 체크
            if (data.state != OrderState.Received && data.state != OrderState.Haggling)
            {
                // if (responseTexts != null && haggleIndex < responseTexts.Length && responseTexts[haggleIndex] != null)
                // {
                //     responseTexts[haggleIndex].text = "<color=red>현재 흥정할 수 있는 상태가 아닙니다.</color>";
                //     responseTexts[haggleIndex].gameObject.SetActive(true);
                //     if (contentLayoutRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(contentLayoutRect);
                // }
                return;
            }

            int haggleIndex = Mathf.RoundToInt((100f - data.patience) / 34f);
            int pt = 0, g = 0, s = 0, c = 0;
            if (platinumInput != null && !string.IsNullOrEmpty(platinumInput.text)) int.TryParse(platinumInput.text, out pt);
            if (goldInput != null && !string.IsNullOrEmpty(goldInput.text)) int.TryParse(goldInput.text, out g);
            if (silverInput != null && !string.IsNullOrEmpty(silverInput.text)) int.TryParse(silverInput.text, out s);
            if (copperInput != null && !string.IsNullOrEmpty(copperInput.text)) int.TryParse(copperInput.text, out c);

            long totalCopper = pt * Magic.Inventory.CurrencyManager.VALUE_PLATINUM +
                               g * Magic.Inventory.CurrencyManager.VALUE_GOLD +
                               s * Magic.Inventory.CurrencyManager.VALUE_SILVER +
                               c * Magic.Inventory.CurrencyManager.VALUE_COPPER;

            if (totalCopper > 0 && totalCopper <= int.MaxValue)
            {
                int askingPrice = (int)totalCopper;
                int beforeCount = data.chatHistory.Count;

                // StoreManager의 HaggleOrder 호출 (호출 후 chatHistory에 2~3줄 추가됨)
                StoreManager.Instance.HaggleOrder(data.orderID, askingPrice);

                // 입력창 초기화
                if (platinumInput != null) platinumInput.text = "";
                if (goldInput != null) goldInput.text = "";
                if (silverInput != null) silverInput.text = "";
                if (copperInput != null) copperInput.text = "";

                // 새로 추가된 데이터만 명시적으로 UI에 반영 (비효율적인 전체 새로고침 제거)
                if (data.chatHistory.Count >= beforeCount + 2)
                {
                    bool isRejected = (data.state != OrderState.Pending && data.state != OrderState.Completed);
                    haggleTexts[haggleIndex].Show(data.chatHistory[beforeCount], isRejected);
                }
                // 흥정이 3번 연속 실패해 3번째 문장(인내심 한계)이 추가된 경우 처리
                if (data.chatHistory.Count >= beforeCount + 3)
                {
                    resultText.text = data.chatHistory[beforeCount + 2];
                    resultText.gameObject.SetActive(true);
                }
            }
            else
            {
                haggleTexts[haggleIndex].Show("<color=red>잘못된 금액입니다.</color>", true);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentLayoutRect);
        }
    }
}
