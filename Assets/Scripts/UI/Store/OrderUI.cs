using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[RequireComponent(typeof(RectTransform))]
public class OrderUI : MonoBehaviour
{
    public CustomerOrder OrderData { get; private set; }
    private Action<OrderUI> _onClickCallback;
    private Action<OrderUI> _onReturnToPoolCallback;

    [Header("UI Elements")]
    [SerializeField] SlotCustomButton _button;
    [SerializeField] private TMP_Text spellNameText;
    [SerializeField] private TMP_Text elementText;
    [SerializeField] private CurrencyValueUI budgetUI;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_Text deadlineText; // 마감일자 표시용 텍스트

    public void Initialize(CustomerOrder order, Action<OrderUI> onClickCallback, Action<OrderUI> onReturnToPoolCallback = null)
    {
        if (OrderData != null)
        {
            OrderData.OnStateChanged -= OnOrderStateChanged;
        }

        OrderData = order;
        _onClickCallback = onClickCallback;
        _onReturnToPoolCallback = onReturnToPoolCallback;

        if (_button != null)
        {
            _button.Deselect(immediate: true);
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnSlotClicked);
        }

        if (OrderData != null)
        {
            OrderData.OnStateChanged += OnOrderStateChanged;
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (OrderData == null) return;

        if (spellNameText != null)
            spellNameText.text = OrderData.requestedSpellName;

        if (elementText != null)
        {
            string elementStr = "무속성";
            switch (OrderData.requestedElement)
            {
                case SpellElement.Fire: elementStr = "화염 속성"; break;
                case SpellElement.Ice: elementStr = "빙결 속성"; break;
                case SpellElement.Lightning: elementStr = "전기 속성"; break;
                case SpellElement.Earth: elementStr = "대지 속성"; break;
            }
            elementText.text = elementStr;
        }

        budgetUI.SetValue(OrderData.claimedBudget);

        UpdateStateText();
    }

    private void UpdateStateText()
    {
        if (OrderData == null || stateText == null) return;

        string stateStr = "";
        switch (OrderData.state)
        {
            case OrderState.Received: stateStr = "주문 접수"; break;
            case OrderState.Haggling: stateStr = "흥정 중"; break;
            case OrderState.Pending: stateStr = "배송 대기"; break;
            case OrderState.Completed: stateStr = "거래 완료"; break;
            case OrderState.Failed: stateStr = "거래 실패"; break;
        }
        stateText.text = stateStr;

        if (deadlineText != null)
        {
            if (OrderData.state == OrderState.Completed || OrderData.state == OrderState.Failed)
            {
                deadlineText.text = "";
            }
            else
            {
                deadlineText.text = $"{OrderData.deadlineDaysLeft}일 남음";
            }
        }
    }

    private void OnOrderStateChanged(OrderState newState)
    {
        if (newState == OrderState.Completed || newState == OrderState.Failed)
        {
            if (OrderData != null)
            {
                OrderData.OnStateChanged -= OnOrderStateChanged;
            }

            if (_onReturnToPoolCallback != null)
            {
                _onReturnToPoolCallback.Invoke(this);
            }
            else
            {
                Destroy(gameObject);
            }
            return;
        }

        UpdateStateText();
    }

    private void OnDestroy()
    {
        if (OrderData != null)
        {
            OrderData.OnStateChanged -= OnOrderStateChanged;
        }
    }

    private void OnSlotClicked()
    {
        _onClickCallback?.Invoke(this);
    }

    public void Select()
    {
        if (_button != null) _button.Select();
    }

    public void Deselect(bool immediate = false)
    {
        if (_button != null) _button.Deselect(immediate);
    }

    public void CompleteOrder()
    {
        if (OrderData != null) OrderData.state = OrderState.Completed;
    }

    public void FailOrder()
    {
        if (OrderData != null) OrderData.state = OrderState.Failed;
    }
}


