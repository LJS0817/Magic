using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PreparationLoadoutUI : PagedUIController<InventorySlot>
{
    [Header("Loadout Setup")]
    [SerializeField] private int _loadoutCapacity = 20;
    [SerializeField] private InventorySlot _wandSlot;
    [SerializeField] private InventorySlot _pouchSlot;

    private InventorySlot _hoveredSlot;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if (_infoPanel != null) _infoPanel.Close();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnLoadoutChanged += RefreshList;
        }

        RefreshList();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
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

        var loadout = InventoryManager.Instance.combatLoadout;
        List<ItemInstance> keysToRemove = new List<ItemInstance>();
        foreach (var key in _itemBackgroundMap.Keys)
        {
            if (!loadout.Contains(key)) keysToRemove.Add(key);
        }
        foreach (var key in keysToRemove) _itemBackgroundMap.Remove(key);

        UpdateWandSlot();
        UpdatePouchSlot();

        base.RefreshList();
    }

    private void UpdateWandSlot()
    {
        if (_wandSlot == null || InventoryManager.Instance == null) return;
        var wand = InventoryManager.Instance.EquippedWand;
        _wandSlot.Initialize(OnWandSlotClicked, OnSlotPointerEnter, OnSlotPointerExit, OnWandSlotDoubleClicked, null);

        if (wand != null)
        {
            _wandSlot.SetInfo(wand, GetSlotBackground(wand), _emptySlotIcon, true);
        }
        else
        {
            _wandSlot.SetInfo(null, GetSlotBackground(null), _emptySlotIcon, false);
        }
    }

    private void OnWandSlotClicked(InventorySlot slot) { }

    private void OnWandSlotDoubleClicked(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || InventoryManager.Instance == null) return;
        InventoryManager.Instance.EquippedWand = null;
        InventoryManager.Instance.NotifyLoadoutChanged();

        if (_infoPanel != null && _hoveredSlot == slot)
        {
            _infoPanel.Close();
        }
    }

    private void UpdatePouchSlot()
    {
        if (_pouchSlot == null || InventoryManager.Instance == null) return;
        var pouch = InventoryManager.Instance.EquippedPouch;
        _pouchSlot.Initialize(OnPouchSlotClicked, OnSlotPointerEnter, OnSlotPointerExit, OnPouchSlotDoubleClicked, null);

        if (pouch != null)
        {
            _pouchSlot.SetInfo(pouch, GetSlotBackground(pouch), _emptySlotIcon, true);
        }
        else
        {
            _pouchSlot.SetInfo(null, GetSlotBackground(null), _emptySlotIcon, false);
        }
    }

    private void OnPouchSlotClicked(InventorySlot slot) { }

    private void OnPouchSlotDoubleClicked(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || InventoryManager.Instance == null) return;
        InventoryManager.Instance.UnequipPouch();

        if (_infoPanel != null && _hoveredSlot == slot)
        {
            _infoPanel.Close();
        }
    }

    protected override int GetTotalCapacity()
    {
        if (InventoryManager.Instance == null) return _loadoutCapacity;
        return Mathf.Max(InventoryManager.Instance.GetMaxCombatLoadoutCapacity(), InventoryManager.Instance.combatLoadout.Count);
    }

    protected override void UpdateSlot(InventorySlot slot, int dataIndex)
    {
        if (InventoryManager.Instance == null) return;
        var loadout = InventoryManager.Instance.combatLoadout;

        slot.Initialize(OnSlotClicked, OnSlotPointerEnter, OnSlotPointerExit, OnSlotDoubleClicked, null);

        if (dataIndex < loadout.Count)
        {
            ItemInstance currentItem = loadout[dataIndex];
            // 로드아웃에 있는 아이템은 "장착됨" 상태이므로 시각적 표시
            slot.SetInfo(currentItem, GetSlotBackground(currentItem), _emptySlotIcon, true);
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
        inv.RemoveFromLoadout(slot.Item);

        if (_infoPanel != null && _hoveredSlot == slot)
        {
            _infoPanel.Close(); // 삭제 시 정보 패널 닫기
        }
    }

    private void OnSlotPointerEnter(InventorySlot slot)
    {
        if (slot == null || slot.Item == null) return;
        _hoveredSlot = slot;

        if (_infoPanel != null)
        {
            _infoPanel.Open();
            _infoPanel.Setup(slot.Item, true, true);

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

