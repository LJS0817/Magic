using UnityEngine;
using UnityEngine.UI;
using TMPro; // Assuming TextMeshPro is used for UI text
using Magic.Store;

namespace Magic.UI.CustomerService
{
    [RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
    public class ChatUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI customerNameText;
        [SerializeField] private TextMeshProUGUI orderDescriptionText;
        [Tooltip("The external container that handles the OrderUI movement")]
        [SerializeField] private OrderContainerUI orderContainerUI;

        private CanvasGroup _canvasGroup;
        private OrderUI _currentOrderUI;

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

            // Update UI elements based on order data
            if (customerNameText != null) customerNameText.text = data.customerName;
            if (orderDescriptionText != null) orderDescriptionText.text = data.orderDescription;
            
            // TODO: Populate chat history visually

            // Enable interactions immediately
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            // if (orderContainerUI != null)
            // {
            //     orderContainerUI.Open(_currentOrderUI);
            // }
        }

        public void CloseChat()
        {
            // Disable interactions immediately
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0f;

            // if (orderContainerUI != null)
            // {
            //     orderContainerUI.Close(_currentOrderUI);
            // }

            _currentOrderUI = null;
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
    }
}
