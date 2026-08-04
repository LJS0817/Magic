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
                    
                    var drawingDB = DrawingDatabase.Instance;
                    if (drawingDB != null && drawingDB.recipes != null && PlayerDataManager.Instance != null)
                    {
                        foreach (var recipe in drawingDB.recipes)
                        {
                            var state = PlayerDataManager.Instance.GetRecipeState(recipe.SpellName);
                            
                            // 힌트만 판매 (상태가 Locked일 때만)
                            if (state == RecipeUnlockState.Locked)
                            {
                                ItemRecipeBookSO hintBook = ScriptableObject.CreateInstance<ItemRecipeBookSO>();
                                hintBook.name = $"Hint_{recipe.SpellName}";
                                hintBook.itemName = $"[힌트] {recipe.SpellName}";
                                hintBook.itemDescription = $"이 낡은 양피지를 읽으면 {recipe.SpellName} 마법의 실마리를 얻을 수 있을 것 같습니다.\n\n<color=yellow>사용 시 마법의 힌트를 획득합니다.</color>";
                                hintBook.itemIcon = recipe.icon; // 레시피 아이콘이 있다면 사용 (없어도 무방)
                                hintBook.targetSpellName = recipe.SpellName;
                                hintBook.isHintOnly = true;
                                hintBook.type = ItemType.RecipeBook;
                                hintBook.basePriceInCopper = (long)Mathf.Round(recipe.manaCost * 20f); // 마나 소모량 기반 가격 책정
                                
                                items.Add(hintBook);
                            }
                        }
                    }
                    break;
                case MarketType.AdventurerGuild:
                    if (db.materials != null) items.AddRange(db.materials);
                    items.AddRange(MarketManager.Instance.activeQuests);

                    if (db.scrolls != null && db.scrolls.Count > 0 && PlayerDataManager.Instance != null && PlayerDataManager.Instance.unlockedRecipeObjects != null)
                    {
                        ItemScrollSO baseScroll = db.scrolls[0];
                        if (baseScroll != null)
                        {
                            foreach (var recipe in PlayerDataManager.Instance.unlockedRecipeObjects)
                            {
                                if (recipe == null) continue;
                                ItemScrollSO completedScroll = Instantiate(baseScroll);
                                completedScroll.name = $"CompletedScroll_{recipe.SpellName}";
                                completedScroll.itemName = $"[완성] {recipe.SpellName} 스크롤";
                                completedScroll.itemDescription = $"{recipe.Description}\n\n<color=orange>이미 마법진이 완성된 스크롤입니다.</color>";
                                completedScroll.itemIcon = recipe.icon != null ? recipe.icon : baseScroll.itemIcon;
                                completedScroll.spellName = recipe.SpellName;
                                completedScroll.scrollElement = recipe.Element;
                                completedScroll.isPreCompleted = true;
                                
                                long magicMarketPrice = (long)Mathf.Round(recipe.manaCost * 5f * (recipe.Element != SpellElement.None ? 1.5f : 1f));
                                long totalMarketPrice = baseScroll.basePriceInCopper + magicMarketPrice;
                                completedScroll.basePriceInCopper = (long)Mathf.Round(totalMarketPrice * 1.05f);
                                
                                items.Add(completedScroll);
                            }
                        }
                    }
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

