using System.Collections.Generic;
using UnityEngine;

public class InventoryUIController : PagedUIController<InventorySlot>
{
    
    [SerializeField] private List<Sprite> _slotBackgrounds;
    [SerializeField] private Sprite _emptySlotIcon;

    [Header("Info Panel")]
    [SerializeField] private ItemInfoPanel _infoPanel;
    [SerializeField] private RectTransform _inventoryPanel;

    private Dictionary<ItemInstance, Sprite> _itemBackgroundMap = new Dictionary<ItemInstance, Sprite>();
    private ItemInstance _selectedItem;
    private InventorySlot _hoveredSlot;
    private int _currentSelectedAmount = 1;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if (_infoPanel != null)
            _infoPanel.Close();
        
        InventoryManager.Instance.OnInventoryChanged += RefreshList;
        InventoryManager.Instance.OnScrollEquipped += OnScrollEquipped;
        InventoryManager.Instance.OnInkEquipped += OnInkEquipped;
        InventoryManager.Instance.OnPenEquipped += OnPenEquipped;
        
        // 처음에 인벤토리에 있는 아이템들을 UI에 반영
        RefreshList();
    }

    private void OnScrollEquipped(Item_Scroll item) => RefreshList();
    private void OnInkEquipped(Item_Ink item) => RefreshList();
    private void OnPenEquipped(Item_Pen item) => RefreshList();

    private void Update()
    {
        if (_hoveredSlot != null && _hoveredSlot.Item != null && _hoveredSlot.Item.IsStackable)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                if (scroll > 0) _currentSelectedAmount++;
                else _currentSelectedAmount--;

                _currentSelectedAmount = Mathf.Clamp(_currentSelectedAmount, 1, _hoveredSlot.Item.count);
                
                if (_infoPanel != null)
                {
                    _infoPanel.UpdateStackAmount(_currentSelectedAmount);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshList;
            InventoryManager.Instance.OnScrollEquipped -= OnScrollEquipped;
            InventoryManager.Instance.OnInkEquipped -= OnInkEquipped;
            InventoryManager.Instance.OnPenEquipped -= OnPenEquipped;
        }
    }

    protected override void OnEnable()
    {
        // UI 창이 켜질 때마다 제일 첫 페이지로 가고 싶다면 _currentPage = 0; 추가 가능
        RefreshList();
    }

    public override void RefreshList()
    {
        if (InventoryManager.Instance == null) return;

        var items = InventoryManager.Instance.items;
        
        // 인벤토리에 더 이상 없는 아이템의 배경 정보는 메모리에서 삭제 (정리)
        List<ItemInstance> keysToRemove = new List<ItemInstance>();
        foreach (var key in _itemBackgroundMap.Keys)
        {
            if (!items.Contains(key)) keysToRemove.Add(key);
        }
        foreach (var key in keysToRemove) _itemBackgroundMap.Remove(key);

        base.RefreshList();
    }

    protected override int GetTotalCapacity()
    {
        if (InventoryManager.Instance == null) return 0;
        return InventoryManager.Instance.GetMaxCapacity();
    }

    protected override void UpdateSlot(InventorySlot slot, int dataIndex)
    {
        if (InventoryManager.Instance == null) return;
        var items = InventoryManager.Instance.items;

        slot.Initialize(OnSlotClicked, OnSlotPointerEnter, OnSlotPointerExit, OnSlotDoubleClicked, OnSlotRightClicked);

        if (dataIndex < items.Count)
        {
            ItemInstance currentItem = items[dataIndex];
            bool isEquipped = IsItemEquipped(currentItem);
            slot.SetInfo(currentItem, GetSlotBackground(currentItem), _emptySlotIcon, isEquipped);
        }
        else
        {
            slot.SetInfo(null, GetSlotBackground(null), _emptySlotIcon, false);
        }
    }
    private void OnSlotClicked(InventorySlot slot)
    {
        if (slot == null || slot.Item == null) return;
        
        _selectedItem = slot.Item;
    }

    private void OnSlotDoubleClicked(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || InventoryManager.Instance == null) return;
        
        // 1. 주문 흥정이 타결되어 스크롤 선택 대기 중인 상태인지 확인
        if (StoreManager.Instance != null && StoreManager.Instance.IsOrderContainerOpen)
        {
            if (slot.Item is Item_Scroll selectedScroll)
            {
                StoreManager.Instance.SelectItemForOrder(selectedScroll);
            }
            
            // 상점 창이 열려있는 동안에는 일반 장착/장착 해제 로직을 원천 차단합니다.
            return;
        }

        // 2. 일반 장착 및 사용 로직
        var inv = InventoryManager.Instance;
        if (slot.Item.IsStackable)
        {
            if (slot.Item is Item_Potion potion)
            {
                var pData = PlayerDataManager.Instance;
                if (pData != null && potion.PotionData != null)
                {
                    if (potion.PotionData.potionType == PotionType.Health)
                    {
                        pData.currentHealth += potion.PotionData.recoveryAmount;
                        if (pData.currentHealth > pData.GetMaxHealth()) pData.currentHealth = pData.GetMaxHealth();
                        Debug.Log($"<color=green>[아이템] {potion.ItemName}을(를) 사용해 체력을 회복했습니다!</color>");
                    }
                    else if (potion.PotionData.potionType == PotionType.Mana)
                    {
                        pData.currentMana += potion.PotionData.recoveryAmount;
                        if (pData.currentMana > pData.GetMaxMana()) pData.currentMana = pData.GetMaxMana();
                        
                        var drawingMgr = FindObjectOfType<CombatDrawingManager>();
                        if (drawingMgr != null && drawingMgr.manaSlider != null)
                        {
                            drawingMgr.manaSlider.SetValue(pData.currentMana, pData.GetMaxMana());
                        }
                        Debug.Log($"<color=cyan>[아이템] {potion.ItemName}을(를) 사용해 마력을 회복했습니다!</color>");
                    }
                    else if (potion.PotionData.potionType == PotionType.ElementalResistance)
                    {
                        pData.ApplyElementalResistancePotion(potion.PotionData.resistanceElement, potion.PotionData.resistancePercentage, potion.PotionData.resistanceDuration);
                        Debug.Log($"<color=yellow>[아이템] {potion.ItemName}을(를) 사용해 {potion.PotionData.resistanceElement} 속성 내성이 {potion.PotionData.resistancePercentage * 100}% 증가했습니다 ({potion.PotionData.resistanceDuration}초)!</color>");
                    }
                    
                    inv.RemoveItem(potion, 1);
                    RefreshList();
                }
            }
            else
            {
                Debug.Log($"[Inventory] {slot.Item.ItemName} {_currentSelectedAmount}개 더블클릭 됨.");
            }
        }
        else if (slot.Item is Item_Scroll scroll) 
        {
            if (inv.EquippedScroll == scroll) inv.EquippedScroll = null;
            else inv.EquippedScroll = scroll;
        }
        else if (slot.Item is Item_Ink ink)
        {
            if (inv.EquippedInk == ink) inv.EquippedInk = null;
            else inv.EquippedInk = ink;
        }
        else if (slot.Item is Item_Pen pen)
        {
            if (inv.EquippedPen == pen) inv.EquippedPen = null;
            else inv.EquippedPen = pen;
        }
        else if (slot.Item is Item_Wand)
        {
            Debug.LogWarning("[인벤토리] 지팡이는 전투 로드아웃 화면(Combat Preparation)에서만 장착할 수 있습니다!");
            return;
        }
        
        // 장착 상태가 바뀌었으므로 툴팁 정보 갱신
        if (_infoPanel != null && _hoveredSlot == slot)
        {
            if (slot.Item != null)
            {
                _infoPanel.Setup(slot.Item, IsItemEquipped(slot.Item));
            }
            else
            {
                _infoPanel.Close();
            }
        }
    }

    private void OnSlotRightClicked(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || InventoryManager.Instance == null) return;

        int amountToRemove = slot.Item.IsStackable ? _currentSelectedAmount : -1;
        InventoryManager.Instance.RemoveItem(slot.Item, amountToRemove);
        
        if (_hoveredSlot == slot)
        {
            if (slot.Item == null || !InventoryManager.Instance.items.Contains(slot.Item))
            {
                _hoveredSlot = null;
                if (_infoPanel != null) _infoPanel.Close();
            }
            else
            {
                // Item still exists (only part of stack removed)
                _currentSelectedAmount = Mathf.Clamp(_currentSelectedAmount, 1, slot.Item.count);
                if (_infoPanel != null)
                {
                    _infoPanel.Setup(slot.Item, IsItemEquipped(slot.Item));
                    _infoPanel.UpdateStackAmount(_currentSelectedAmount);
                }
            }
        }
    }

    private void OnSlotPointerEnter(InventorySlot slot)
    {
        if (slot == null || slot.Item == null) return;
        
        _hoveredSlot = slot;
        _currentSelectedAmount = 1;

        // 슬롯 호버 시 정보 패널을 띄우고 위치를 슬롯 아래로 이동
        if (_infoPanel != null)
        {
            _infoPanel.Open();
            _infoPanel.Setup(slot.Item, IsItemEquipped(slot.Item));
            
            if (slot.Item.IsStackable)
            {
                _infoPanel.UpdateStackAmount(_currentSelectedAmount);
            }
            
            _infoPanel.ClippingPosition(slot.GetComponent<RectTransform>(), _inventoryPanel);
        }
    }

    private void OnSlotPointerExit(InventorySlot slot)
    {
        if (_hoveredSlot == slot)
        {
            _hoveredSlot = null;
        }
        StartCoroutine(CheckAndClosePanel());
    }

    private System.Collections.IEnumerator CheckAndClosePanel()
    {
        yield return null; // 1프레임 대기
        
        if (_infoPanel != null && _hoveredSlot == null)
        {
            _infoPanel.Close();
        }
    }


    private bool IsItemEquipped(ItemInstance item)
    {
        if (item == null) return false;

        if (StoreManager.Instance != null && StoreManager.Instance.IsOrderContainerOpen)
        {
            return StoreManager.Instance.SelectedOrderItem == item;
        }

        if (item is Item_Scroll scroll) return InventoryManager.Instance.EquippedScroll == scroll;
        if (item is Item_Ink ink) return InventoryManager.Instance.EquippedInk == ink;
        if (item is Item_Pen pen) return InventoryManager.Instance.EquippedPen == pen;
        
        return false;
    }


    Sprite GetSlotBackground(ItemInstance item)
    {
        if (_slotBackgrounds == null || _slotBackgrounds.Count == 0) return null;

        // 빈 슬롯일 경우 기본 배경(예: 첫 번째 배경) 반환
        if (item == null)
        {
            return _slotBackgrounds[0];
        }

        // 이미 배경이 지정된 아이템이면 기존 배경 반환
        if (_itemBackgroundMap.TryGetValue(item, out Sprite bg))
        {
            return bg;
        }

        // 새로운 아이템이면 랜덤 배경을 지정하고 사전에 저장
        Sprite newBg = _slotBackgrounds[Random.Range(0, _slotBackgrounds.Count)];
        _itemBackgroundMap[item] = newBg;
        return newBg;
    }
}

