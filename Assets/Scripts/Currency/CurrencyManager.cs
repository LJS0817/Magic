using UnityEngine;
using System;
using System.Collections.Generic;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    // 화폐 가치 비율 상수 정의
    public const int COPPER_PER_SILVER = 100;
    public const int SILVER_PER_GOLD = 100;
    public const int GOLD_PER_PLATINUM = 100;

    // Copper 환산 배수
    public const long VALUE_COPPER = 1;
    public const long VALUE_SILVER = COPPER_PER_SILVER;
    public const long VALUE_GOLD = VALUE_SILVER * SILVER_PER_GOLD;
    public const long VALUE_PLATINUM = VALUE_GOLD * GOLD_PER_PLATINUM;

    // [Header("Currency Storage")]
    // [SerializeField] private int _copper;
    // [SerializeField] private int _silver;
    // [SerializeField] private int _gold;
    // [SerializeField] private int _platinum;
    // [SerializeField] private int _gem;
    private long[] _money;

    // 외부 읽기용 프로퍼티 및 변경 시 이벤트 호출
    public long Copper
    {
        get => _money[0];
        private set
        {
            if (_money[0] != value)
            {
                _money[0] = value;
                OnCurrencyChanged?.Invoke(CurrencyType.Copper, _money[0]);
            }
        }
    }

    public long Silver
    {
        get => _money[1];
        private set
        {
            if (_money[1] != value)
            {
                _money[1] = value;
                OnCurrencyChanged?.Invoke(CurrencyType.Silver, _money[1]);
            }
        }
    }

    public long Gold
    {
        get => _money[2];
        private set
        {
            if (_money[2] != value)
            {
                _money[2] = value;
                OnCurrencyChanged?.Invoke(CurrencyType.Gold, _money[2]);
            }
        }
    }

    public long Platinum
    {
        get => _money[3];
        private set
        {
            if (_money[3] != value)
            {
                _money[3] = value;
                OnCurrencyChanged?.Invoke(CurrencyType.Platinum, _money[3]);
            }
        }
    }

    public long Gem
    {
        get => _money[4];
        private set
        {
            if (_money[4] != value)
            {
                _money[4] = value;
                OnCurrencyChanged?.Invoke(CurrencyType.Gem, _money[4]);
            }
        }
    }

    // 재화 변경 이벤트 (재화 타입, 변경된 보유량)
    public event Action<CurrencyType, long> OnCurrencyChanged;

    // 파산 이벤트 및 한계치
    public event Action OnBankruptcy;
    public long bankruptcyThresholdCopper = 0; // 마이너스가 되는 즉시 파산

    private void Awake()
    {
        _money = new long[5] { 0, 0, 0, 0, 0 };
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 특정 재화의 보유량을 반환합니다.
    /// </summary>
    public long GetCurrencyAmount(CurrencyType type)
    {
        switch (type)
        {
            case CurrencyType.Copper: return Copper;
            case CurrencyType.Silver: return Silver;
            case CurrencyType.Gold: return Gold;
            case CurrencyType.Platinum: return Platinum;
            case CurrencyType.Gem: return Gem;
            default: return 0;
        }
    }

    /// <summary>
    /// 전체 일반 화폐(동/은/금/백금)를 동화(Copper) 기준으로 합산하여 가치를 반환합니다.
    /// </summary>
    public long GetTotalNormalCurrencyAsCopper()
    {
        return (Copper * VALUE_COPPER) +
               (Silver * VALUE_SILVER) +
               (Gold * VALUE_GOLD) +
               (Platinum * VALUE_PLATINUM);
    }

    /// <summary>
    /// 지정한 수량만큼의 재화를 보유하고 있는지 확인합니다.
    /// </summary>
    /// <param name="type">체크할 재화 종류</param>
    /// <param name="amount">요청 수량</param>
    /// <param name="autoConvert">자동 상하위 화폐 환산 적용 여부 (Gem은 제외)</param>
    public bool HasEnoughCurrency(CurrencyType type, long amount, bool autoConvert = true)
    {
        if (amount < 0) return false;

        if (type == CurrencyType.Gem)
        {
            return Gem >= amount;
        }

        if (autoConvert)
        {
            long requiredCopper = GetValueInCopper(type, amount);
            return GetTotalNormalCurrencyAsCopper() >= requiredCopper;
        }
        else
        {
            return GetCurrencyAmount(type) >= amount;
        }
    }

    /// <summary>
    /// 재화를 추가합니다. (autoConvert가 true일 경우 상위 화폐로 자동 환전합니다.)
    /// </summary>
    public void AddCurrency(CurrencyType type, long amount, bool autoConvert = true)
    {
        if (amount <= 0) return;

        switch (type)
        {
            case CurrencyType.Copper: Copper += amount; break;
            case CurrencyType.Silver: Silver += amount; break;
            case CurrencyType.Gold: Gold += amount; break;
            case CurrencyType.Platinum: Platinum += amount; break;
            case CurrencyType.Gem: Gem += amount; break;
        }
        
        if (autoConvert && type != CurrencyType.Gem)
        {
            CompressCurrency();
        }

        Debug.Log($"[CurrencyManager] Added {amount} {type}. Current: {GetCurrencyAmount(type)}");
    }

    /// <summary>
    /// 재화를 소비합니다.
    /// </summary>
    /// <param name="type">소비할 재화 종류</param>
    /// <param name="amount">소비할 양</param>
    /// <param name="autoConvert">자동 화폐 변환 적용 여부 (예: 은화 결제 시 동화가 부족하면 금화를 깨거나 동화를 합쳐 지불)</param>
    /// <param name="allowNegative">마이너스 자산을 허용할지 여부 (예: 위약금 차감 등)</param>
    /// <returns>소비 성공 여부</returns>
    public bool SpendCurrency(CurrencyType type, long amount, bool autoConvert = true, bool allowNegative = false)
    {
        if (amount < 0) return false;
        if (amount == 0) return true;

        if (!allowNegative && !HasEnoughCurrency(type, amount, autoConvert))
        {
            Debug.LogWarning($"[CurrencyManager] 부족한 재화: {type} {amount} 필요 (보유: {GetCurrencyAmount(type)})");
            return false;
        }

        if (type == CurrencyType.Gem)
        {
            Gem -= amount;
            Debug.Log($"[CurrencyManager] Spent {amount} Gem. Current: {Gem}");
            return true;
        }

        if (autoConvert)
        {
            // 전체를 Copper 단위로 환산하여 계산 후 다시 화폐를 재배치(상위 화폐 깨기 & 자동 정산)
            long totalCopper = GetTotalNormalCurrencyAsCopper();
            long costInCopper = GetValueInCopper(type, amount);
            long remainingCopper = totalCopper - costInCopper;

            DistributeCopper(remainingCopper);
            Debug.Log($"[CurrencyManager] Spent {amount} {type} (AutoConverted). Current - Pt:{Platinum}, Au:{Gold}, Ag:{Silver}, Cu:{Copper}");
        }
        else
        {
            // 단순 지정 재화 차감
            switch (type)
            {
                case CurrencyType.Copper: Copper -= amount; break;
                case CurrencyType.Silver: Silver -= amount; break;
                case CurrencyType.Gold: Gold -= amount; break;
                case CurrencyType.Platinum: Platinum -= amount; break;
            }
            Debug.Log($"[CurrencyManager] Spent {amount} {type} (No AutoConvert). Current: {GetCurrencyAmount(type)}");
        }

        return true;
    }

    /// <summary>
    /// 지갑에 쌓인 하위 화폐들을 자동으로 규칙에 맞춰 상위 화폐로 깔끔하게 정리합니다.
    /// (예: 250 동화 -> 2 은화 50 동화로 정리)
    /// </summary>
    public void CompressCurrency()
    {
        long totalCopper = GetTotalNormalCurrencyAsCopper();
        DistributeCopper(totalCopper);
        Debug.Log($"[CurrencyManager] Currency Compressed. Pt:{Platinum}, Au:{Gold}, Ag:{Silver}, Cu:{Copper}");
        
        if (totalCopper < -bankruptcyThresholdCopper)
        {
            Debug.LogError($"[CurrencyManager] 파산! 빚({-totalCopper} 동화)을 갚지 못해 파산했습니다.");
            OnBankruptcy?.Invoke();
        }
    }

    /// <summary>
    /// 특정 화폐 수량을 동화 가치로 환산하여 반환합니다.
    /// </summary>
    private long GetValueInCopper(CurrencyType type, long amount)
    {
        switch (type)
        {
            case CurrencyType.Copper: return amount * VALUE_COPPER;
            case CurrencyType.Silver: return amount * VALUE_SILVER;
            case CurrencyType.Gold: return amount * VALUE_GOLD;
            case CurrencyType.Platinum: return amount * VALUE_PLATINUM;
            default: return 0;
        }
    }

    /// <summary>
    /// 동화 기준의 값을 지갑의 상하위 화폐로 적절히 나누어 담습니다.
    /// </summary>
    private void DistributeCopper(long totalCopper)
    {
        long remaining = totalCopper;

        long newPlatinum = remaining / VALUE_PLATINUM;
        remaining %= VALUE_PLATINUM;

        long newGold = remaining / VALUE_GOLD;
        remaining %= VALUE_GOLD;

        long newSilver = remaining / VALUE_SILVER;
        remaining %= VALUE_SILVER;

        long newCopper = remaining / VALUE_COPPER;

        Platinum = newPlatinum;
        Gold = newGold;
        Silver = newSilver;
        Copper = newCopper;
    }

    /// <summary>
    /// 동화 단위의 금액을 백금화, 금화, 은화, 동화 단위의 읽기 쉬운 문자열로 변환합니다.
    /// </summary>
    public static string FormatCurrency(long totalCopper)
    {
        if (totalCopper == 0) return "0 동화";

        bool isNegative = totalCopper < 0;
        long remaining = Math.Abs(totalCopper);

        long pt = remaining / VALUE_PLATINUM;
        remaining %= VALUE_PLATINUM;

        long g = remaining / VALUE_GOLD;
        remaining %= VALUE_GOLD;

        long s = remaining / VALUE_SILVER;
        remaining %= VALUE_SILVER;

        long c = remaining / VALUE_COPPER;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        if (isNegative) sb.Append("-");

        if (pt > 0) sb.Append($"{pt} 백금화 ");
        if (g > 0) sb.Append($"{g} 금화 ");
        if (s > 0) sb.Append($"{s} 은화 ");
        if (c > 0) sb.Append($"{c} 동화 ");

        return sb.ToString().TrimEnd();
    }
}

