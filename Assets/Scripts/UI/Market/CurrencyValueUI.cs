using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CurrencyValueUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] TMP_Text wText; // Platinum (백금화)
    [SerializeField] TMP_Text gText; // Gold (금화)
    [SerializeField] TMP_Text sText; // Silver (은화)
    [SerializeField] TMP_Text bText; // Copper/Bronze (동화)

    [Header("Options")]
    [Tooltip("0인 화폐 단위는 UI에서 자동으로 숨길지 여부")]
    [SerializeField] private bool hideZeroValues = true;

    private long _currentValue;

    public long CurrentValue => _currentValue;

    public void SetValue(long v)
    {
        _currentValue = v;
        RefreshUI();
    }

    public void AddValue(long baseValue)
    {
        _currentValue += baseValue;
        RefreshUI();
    }

    public void RefreshUI()
    {
        long absVal = Math.Abs(_currentValue);
        bool isNegative = _currentValue < 0;

        long w = absVal / CurrencyManager.VALUE_PLATINUM;
        absVal %= CurrencyManager.VALUE_PLATINUM;

        long g = absVal / CurrencyManager.VALUE_GOLD;
        absVal %= CurrencyManager.VALUE_GOLD;

        long s = absVal / CurrencyManager.VALUE_SILVER;
        absVal %= CurrencyManager.VALUE_SILVER;

        long b = absVal;

        // 모든 화폐가 0일 때는 최소단위인 동화(bText)를 반드시 0으로 표시하기 위함
        bool isAllZero = (_currentValue == 0);

        UpdateSlot(wText, w, hideZeroValues && !isAllZero, isNegative && w > 0);
        if (w > 0) isNegative = false; // 음수 부호는 가장 큰 화폐 단위에만 1번 붙임

        UpdateSlot(gText, g, hideZeroValues && !isAllZero, isNegative && g > 0);
        if (g > 0) isNegative = false;

        UpdateSlot(sText, s, hideZeroValues && !isAllZero, isNegative && s > 0);
        if (s > 0) isNegative = false;

        UpdateSlot(bText, b, hideZeroValues && !isAllZero && (w > 0 || g > 0 || s > 0), isNegative && (b > 0 || isAllZero));

        if (transform is RectTransform rt)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
    }

    private void UpdateSlot(TMP_Text textComponent, long amount, bool shouldHide, bool prependMinus)
    {
        if (textComponent == null) return;

        if (shouldHide && amount == 0)
        {
            if (textComponent.transform.parent.gameObject.activeSelf)
                textComponent.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            if (!textComponent.transform.parent.gameObject.activeSelf)
                textComponent.transform.parent.gameObject.SetActive(true);

            string strVal = amount.ToString("N0");
            if (prependMinus) strVal = "-" + strVal;

            textComponent.text = strVal;
        }
    }
}
