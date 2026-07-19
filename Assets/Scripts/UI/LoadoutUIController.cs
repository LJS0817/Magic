using System.Collections.Generic;
using UnityEngine;
using Magic.Inventory;

namespace Magic.UI
{
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
                slot.SetInfo(currentItem, GetSlotBackground(currentItem), _emptySlotIcon, false);
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

            // 더블 클릭 시 로드아웃에서 해제
            InventoryManager.Instance.RemoveFromLoadout(slot.Item);
            
            if (_infoPanel != null) _infoPanel.Close();
        }

        private void OnSlotRightClicked(InventorySlot slot)
        {
            if (slot == null || slot.Item == null || InventoryManager.Instance == null) return;

            // 전투 중인지 확인
            var combatManager = FindAnyObjectByType<Magic.Combat.CombatManager>();
            if (combatManager != null && combatManager.CurrentState != Magic.Combat.CombatState.BattleEnd)
            {
                if (slot.Item is Item_Scroll scroll && !scroll.isEmpty && scroll.ScrollData != null)
                {
                    // 전투 중 완성된 스크롤 마법 발사
                    var drawingManager = FindAnyObjectByType<Magic.Combat.CombatDrawingManager>();
                    if (drawingManager != null)
                    {
                        drawingManager.CastSpellFromScroll(scroll);
                        return; // 마법 사용 후 로드아웃에서 해제하지 않음
                    }
                }
            }

            // 우클릭 시 로드아웃에서 해제
            InventoryManager.Instance.RemoveFromLoadout(slot.Item);
            
            if (_hoveredSlot == slot)
            {
                _hoveredSlot = null;
                if (_infoPanel != null) _infoPanel.Close();
            }
        }

        private void OnSlotPointerEnter(InventorySlot slot)
        {
            if (slot == null || slot.Item == null) return;

            _hoveredSlot = slot;

            if (_infoPanel != null)
            {
                _infoPanel.Open();
                _infoPanel.Setup(slot.Item, false);
                if (_inventoryPanel != null)
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
}
