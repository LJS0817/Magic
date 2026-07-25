using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class MarketUIController : MonoBehaviour
{
    [Header("UI References")]
    public List<MarketUI> _markets;
    public TMP_Text marketNameText;

    [Header("Market Navigation")]
    public Button nextMarketButton;
    public Button prevMarketButton;

    private int _marketIndex = -1;
    private bool _isInitialized = false;

    private void Awake()
    {
        if (nextMarketButton != null) nextMarketButton.onClick.AddListener(NextMarket);
        if (prevMarketButton != null) prevMarketButton.onClick.AddListener(PrevMarket);
    }

    private void Start()
    {
        InitializeMarkets();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            PrevMarket();
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            NextMarket();   
        }
    }

    private void InitializeMarkets()
    {
        if (_isInitialized) return;

        var db = InventoryManager.Instance != null ? InventoryManager.Instance.itemDatabase : null;
        if (db == null)
        {
            Debug.LogWarning("[MarketUIController] ItemDatabase를 찾을 수 없습니다.");
            return;
        }

        foreach (var market in _markets)
        {
            if (market == null) continue;

            List<ItemDataSO> items = new List<ItemDataSO>();
            switch (market.marketType)
            {
                case MarketType.WandShop:
                    if (db.wands != null) items.AddRange(db.wands);
                    break;
                case MarketType.InkAndPenShop:
                    if (db.inks != null) items.AddRange(db.inks);
                    if (db.pens != null) items.AddRange(db.pens);
                    break;
                case MarketType.ParchmentShop:
                    if (db.scrolls != null) items.AddRange(db.scrolls);
                    break;
                case MarketType.AdventurerGuild:
                    if (db.potions != null) items.AddRange(db.potions);
                    break;
            }
            market.Setup(items);
        }
        SelectMarket(0, true);
        _isInitialized = true;
    }

    public void SelectMarket(int index, bool goLeft)
    {
        if(_marketIndex == index) return;
        
        if(_marketIndex >= 0) _markets[_marketIndex].Hide(goLeft);
        _marketIndex = index;

        _markets[_marketIndex].Show(goLeft);
        _markets[_marketIndex].RefreshList();
        UpdateMarketUI(_marketIndex);
    }

    private void UpdateMarketUI(int index)
    {
        if (marketNameText != null && _markets != null && index >= 0 && index < _markets.Count)
        {
            var market = _markets[index];
            if (market != null)
            {
                marketNameText.text = GetMarketName(market.marketType);
            }
        }
    }

    private string GetMarketName(MarketType type)
    {
        switch (type)
        {
            case MarketType.WandShop: return "지팡이 전문 마켓";
            case MarketType.InkAndPenShop: return "잉크 펜 전문 마켓";
            case MarketType.ParchmentShop: return "양피지 전문 마켓";
            case MarketType.AdventurerGuild: return "모험자 길드";
            default: return "상점";
        }
    }

    public void NextMarket()
    {
        if (_markets == null || _markets.Count == 0) return;
        int nextIndex = (_marketIndex + 1) % _markets.Count;
        SelectMarket(nextIndex, true);
    }

    public void PrevMarket()
    {
        if (_markets == null || _markets.Count == 0) return;
        int prevIndex = (_marketIndex - 1 + _markets.Count) % _markets.Count;
        SelectMarket(prevIndex, false);
    }
}

