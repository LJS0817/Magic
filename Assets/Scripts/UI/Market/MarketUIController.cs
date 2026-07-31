using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class MarketUIController : MonoBehaviour
{
    [Header("UI References")]
    public List<MarketUI> _markets;

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
                case MarketType.PotionShop:
                    if (db.potions != null) items.AddRange(db.potions);
                    break;
                case MarketType.EquipmentShop:
                    if (db.wands != null) items.AddRange(db.wands);
                    if (db.pouches != null) items.AddRange(db.pouches);
                    if (db.robes != null) items.AddRange(db.robes);
                    if (db.cloaks != null) items.AddRange(db.cloaks);
                    break;
                case MarketType.DrawingShop:
                    if (db.inks != null) items.AddRange(db.inks);
                    if (db.pens != null) items.AddRange(db.pens);
                    if (db.scrolls != null) items.AddRange(db.scrolls);
                    if (db.drawingTools != null) items.AddRange(db.drawingTools);
                    break;
                case MarketType.AdventurerGuild:
                    if (db.materials != null) items.AddRange(db.materials);
                    items.AddRange(MarketManager.Instance.activeQuests);
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

