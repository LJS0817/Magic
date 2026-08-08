using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PreparationInventoryUI : PagedUIController<InventorySlot>
{
    private InventorySlot _hoveredSlot;
    private List<ItemInstance> _filteredItems = new List<ItemInstance>();

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if (_infoPanel != null) _infoPanel.Close();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshList;
            InventoryManager.Instance.OnLoadoutChanged += RefreshList; // 로드아웃 변경 시 선택 표시 동기화
        }

        RefreshList();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshList;
            InventoryManager.Instance.OnLoadoutChanged -= RefreshList;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public override void RefreshList()
    {
        if (InventoryManager.Instance == null) return;

        var items = InventoryManager.Instance.items;
        _filteredItems.Clear();
        foreach (var item in items)
        {
            if (item is Item_Scroll scroll && !scroll.isEmpty)
            {
                _filteredItems.Add(scroll);
            }
            else if (item is Item_Wand wand)
            {
                _filteredItems.Add(wand);
            }
            else if (item is Item_Potion potion)
            {
                _filteredItems.Add(potion);
            }
            else if (item is Item_Pouch pouch)
            {
                _filteredItems.Add(pouch);
            }
        }

        List<ItemInstance> keysToRemove = new List<ItemInstance>();
        foreach (var key in _itemBackgroundMap.Keys)
        {
            if (!_filteredItems.Contains(key)) keysToRemove.Add(key);
        }
        foreach (var key in keysToRemove) _itemBackgroundMap.Remove(key);

        base.RefreshList();
    }

    protected override int GetTotalCapacity()
    {
        if (InventoryManager.Instance == null) return 0;
        return Mathf.Max(InventoryManager.Instance.GetMaxCapacity(), _filteredItems.Count);
    }

    protected override void UpdateSlot(InventorySlot slot, int dataIndex)
    {
        if (InventoryManager.Instance == null) return;

        slot.Initialize(OnSlotClicked, OnSlotPointerEnter, OnSlotPointerExit, OnSlotDoubleClicked, null);

        if (dataIndex < _filteredItems.Count)
        {
            ItemInstance currentItem = _filteredItems[dataIndex];
            // 로드아웃에 포함되어 있으면 '장착(선택)' 마크를 활성화
            bool isSelected = InventoryManager.Instance.combatLoadout.Contains(currentItem) || InventoryManager.Instance.EquippedWand == currentItem;
            slot.SetInfo(currentItem, GetSlotBackground(currentItem), _emptySlotIcon, isSelected);
        }
        else
        {
            slot.SetInfo(null, GetSlotBackground(null), _emptySlotIcon, false);
        }
    }

    private void OnSlotClicked(InventorySlot slot) { }

    private void OnSlotDoubleClicked(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || InventoryManager.Instance == null) return;

        var inv = InventoryManager.Instance;
        if (slot.Item is Item_Wand wand)
        {
            if (inv.EquippedWand == wand) inv.EquippedWand = null;
            else inv.EquippedWand = wand;
            inv.NotifyLoadoutChanged();
        }
        else if (slot.Item is Item_Pouch pouch)
        {
            inv.EquipPouch(pouch);
        }
        else
        {
            if (inv.combatLoadout.Contains(slot.Item))
            {
                inv.RemoveFromLoadout(slot.Item);
            }
            else
            {
                inv.AddToLoadout(slot.Item);
            }
        }

        if (_infoPanel != null && _hoveredSlot == slot)
        {
            _infoPanel.Setup(slot.Item, inv.combatLoadout.Contains(slot.Item), true);
        }
    }

    private void OnSlotPointerEnter(InventorySlot slot)
    {
        if (slot == null || slot.Item == null) return;
        _hoveredSlot = slot;

        if (_infoPanel != null)
        {
            _infoPanel.Open();
            bool isSelected = InventoryManager.Instance != null && (InventoryManager.Instance.combatLoadout.Contains(slot.Item) || InventoryManager.Instance.EquippedWand == slot.Item);
            _infoPanel.Setup(slot.Item, isSelected, true);

        }
    }

    private void OnSlotPointerExit(InventorySlot slot)
    {
        if (_hoveredSlot == slot) _hoveredSlot = null;
        StartCoroutine(CheckAndClosePanel());
    }

    private System.Collections.IEnumerator CheckAndClosePanel()
    {
        yield return null;
        if (_infoPanel != null && _hoveredSlot == null) _infoPanel.Close();
    }

}

