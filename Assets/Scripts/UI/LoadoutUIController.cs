using System.Collections.Generic;
using UnityEngine;

public class LoadoutUIController : PagedUIController<InventorySlot>
{
    [SerializeField] private List<Sprite> _slotBackgrounds;
    [SerializeField] private Sprite _emptySlotIcon;

    [Header("Info Panel")]
    [SerializeField] private ItemInfoPanel _infoPanel;
    [SerializeField] private RectTransform _inventoryPanel;

    private Dictionary<ItemInstance, Sprite> _itemBackgroundMap = new Dictionary<ItemInstance, Sprite>();
    private InventorySlot _hoveredSlot;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if (_infoPanel != null)
            _infoPanel.Close();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnLoadoutChanged += RefreshList;
            InventoryManager.Instance.OnScrollEquipped += OnScrollEquipped;
            InventoryManager.Instance.OnInkEquipped += OnInkEquipped;
            InventoryManager.Instance.OnPenEquipped += OnPenEquipped;
        }

        RefreshList();
    }

    private void OnScrollEquipped(Item_Scroll item) => RefreshList();
    private void OnInkEquipped(Item_Ink item) => RefreshList();
    private void OnPenEquipped(Item_Pen item) => RefreshList();

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnLoadoutChanged -= RefreshList;
            InventoryManager.Instance.OnScrollEquipped -= OnScrollEquipped;
            InventoryManager.Instance.OnInkEquipped -= OnInkEquipped;
            InventoryManager.Instance.OnPenEquipped -= OnPenEquipped;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        RefreshList();
    }

    public override void RefreshList()
    {
        if (InventoryManager.Instance == null) return;
        var loadout = InventoryManager.Instance.combatLoadout;

        // 안 쓰이는 배경 이미지 매핑 정리
        List<ItemInstance> keysToRemove = new List<ItemInstance>();
        foreach (var key in _itemBackgroundMap.Keys)
        {
            if (!loadout.Contains(key)) keysToRemove.Add(key);
        }
        foreach (var key in keysToRemove) _itemBackgroundMap.Remove(key);

        base.RefreshList();
    }

    protected override int GetTotalCapacity()
    {
        // 로드아웃 사이즈는 20칸 정도로 고정하거나, 현재 아이템 개수에 맞춤
        // 여기서는 넉넉하게 20개 기준으로 잡습니다. (필요 시 수정 가능)
        if (InventoryManager.Instance == null) return 20;
        return Mathf.Max(20, InventoryManager.Instance.combatLoadout.Count);
    }

    protected override void UpdateSlot(InventorySlot slot, int dataIndex)
    {
        if (InventoryManager.Instance == null) return;
        var loadout = InventoryManager.Instance.combatLoadout;

        slot.Initialize(OnSlotClicked, OnSlotPointerEnter, OnSlotPointerExit, OnSlotDoubleClicked, OnSlotRightClicked);

        if (dataIndex < loadout.Count)
        {
            ItemInstance currentItem = loadout[dataIndex];
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
    }

    private void OnSlotDoubleClicked(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || InventoryManager.Instance == null) return;

        var inv = InventoryManager.Instance;
        if (slot.Item is Item_Scroll scroll) 
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
        else if (slot.Item is Item_Potion potion)
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

        // 스크롤 아이템인 경우 '사용' (전투 중 완성된 마법 발사)
        if (slot.Item is Item_Scroll scroll && !scroll.isEmpty && scroll.ScrollData != null)
        {
            var combatManager = FindAnyObjectByType<CombatManager>();
            if (combatManager != null && combatManager.CurrentState != CombatState.BattleEnd)
            {
                var drawingManager = FindAnyObjectByType<CombatDrawingManager>();
                if (drawingManager != null)
                {
                    drawingManager.CastSpellFromScroll(scroll);
                }
            }
        }
        
        // 잉크나 펜 등 다른 아이템은 우클릭 기능(로드아웃 해제 등)을 사용하지 않음
    }

    private void OnSlotPointerEnter(InventorySlot slot)
    {
        if (slot == null || slot.Item == null) return;

        _hoveredSlot = slot;

        if (_infoPanel != null)
        {
            _infoPanel.Open();
            _infoPanel.Setup(slot.Item, IsItemEquipped(slot.Item));
            if (_inventoryPanel != null)
                _infoPanel.ClippingPosition(slot.GetComponent<RectTransform>(), _inventoryPanel, true);
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
        if (item is Item_Scroll scroll) return InventoryManager.Instance.EquippedScroll == scroll;
        if (item is Item_Ink ink) return InventoryManager.Instance.EquippedInk == ink;
        if (item is Item_Pen pen) return InventoryManager.Instance.EquippedPen == pen;
        return false;
    }

    private Sprite GetSlotBackground(ItemInstance item)
    {
        if (_slotBackgrounds == null || _slotBackgrounds.Count == 0) return null;

        if (item == null)
        {
            return _slotBackgrounds[0];
        }

        if (_itemBackgroundMap.TryGetValue(item, out Sprite bg))
        {
            return bg;
        }

        Sprite newBg = _slotBackgrounds[Random.Range(0, _slotBackgrounds.Count)];
        _itemBackgroundMap[item] = newBg;
        return newBg;
    }
}

