using UnityEngine;
using TMPro;

public class CurrencyUIController : MonoBehaviour
{
    [Header("Currency Texts")]
    [SerializeField] private TMP_Text _copperText;
    [SerializeField] private TMP_Text _silverText;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _platinumText;
    [SerializeField] private TMP_Text _gemText;

    private void Start()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged += UpdateCurrencyUI;

            // Initialize UI with current values
            UpdateCurrencyUI(CurrencyType.Copper, CurrencyManager.Instance.Copper);
            UpdateCurrencyUI(CurrencyType.Silver, CurrencyManager.Instance.Silver);
            UpdateCurrencyUI(CurrencyType.Gold, CurrencyManager.Instance.Gold);
            UpdateCurrencyUI(CurrencyType.Platinum, CurrencyManager.Instance.Platinum);
            UpdateCurrencyUI(CurrencyType.Gem, CurrencyManager.Instance.Gem);
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= UpdateCurrencyUI;
        }
    }

    private void UpdateCurrencyUI(CurrencyType type, int amount)
    {
        // Utility 네임스페이스의 ToShortFormat 확장 메서드 사용 (C# 확장 메서드 특성상 namespace 추가 필요)
        string amountStr = type == CurrencyType.Gem ? amount.ToShortFormat() : amount.ToString("N0");

        switch (type)
        {
            case CurrencyType.Copper:
                if (_copperText != null) _copperText.text = amountStr;
                break;
            case CurrencyType.Silver:
                if (_silverText != null) _silverText.text = amountStr;
                break;
            case CurrencyType.Gold:
                if (_goldText != null) _goldText.text = amountStr;
                break;
            case CurrencyType.Platinum:
                if (_platinumText != null) _platinumText.text = amountStr;
                break;
            case CurrencyType.Gem:
                if (_gemText != null) _gemText.text = amountStr;
                break;
        }
    }

    public void TestAdd(int type)
    {
        CurrencyManager.Instance.AddCurrency((CurrencyType)type, 10);
    }
}

