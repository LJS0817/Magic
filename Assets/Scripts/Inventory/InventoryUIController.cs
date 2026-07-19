using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Magic.Inventory
{
    public class InventoryUIController : Magic.UI.PagedUIController<InventorySlot>
    {
        
        [SerializeField] private List<Sprite> _slotBackgrounds;
        [SerializeField] private Sprite _emptySlotIcon;

        [Header("Info Panel")]
        [SerializeField] private ItemInfoPanel _infoPanel;
        [SerializeField] private RectTransform _inventoryPanel;

        

        private Dictionary<ItemInstance, Sprite> _itemBackgroundMap = new Dictionary<ItemInstance, Sprite>();
        private ItemInstance _selectedItem;
        private InventorySlot _hoveredSlot;

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
            if (Magic.Store.StoreManager.Instance != null && Magic.Store.StoreManager.Instance.IsOrderContainerOpen)
            {
                if (slot.Item is Item_Scroll selectedScroll)
                {
                    Magic.Store.StoreManager.Instance.SelectItemForOrder(selectedScroll);
                }
                
                // 상점 창이 열려있는 동안에는 일반 장착/장착 해제 로직을 원천 차단합니다.
                return;
            }

            // 2. 일반 장착 로직
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
            
            // 장착 상태가 바뀌었으므로 툴팁 정보 갱신
            if (_infoPanel != null && _hoveredSlot == slot)
            {
                _infoPanel.Setup(slot.Item, IsItemEquipped(slot.Item));
            }
        }

        private void OnSlotRightClicked(InventorySlot slot)
        {
            if (slot == null || slot.Item == null || InventoryManager.Instance == null) return;

            InventoryManager.Instance.RemoveItem(slot.Item);
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

            // 슬롯 호버 시 정보 패널을 띄우고 위치를 슬롯 아래로 이동
            if (_infoPanel != null)
            {
                _infoPanel.Open();
                _infoPanel.Setup(slot.Item, IsItemEquipped(slot.Item));

                RectTransform infoRect = _infoPanel.GetComponent<RectTransform>();
                RectTransform slotRect = slot.GetComponent<RectTransform>();

                if (infoRect != null && slotRect != null && _inventoryPanel != null)
                {
                    // UI 크기 갱신 (ContentSizeFitter 적용 시 정확한 크기 측정)
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(infoRect);

                    Vector3[] slotCorners = new Vector3[4];
                    slotRect.GetWorldCorners(slotCorners);
                    Vector3 bottomCenter = (slotCorners[0] + slotCorners[3]) / 2f;
                    Vector3 topCenter = (slotCorners[1] + slotCorners[2]) / 2f;
                    
                    Vector3[] invCorners = new Vector3[4];
                    _inventoryPanel.GetWorldCorners(invCorners);
                    
                    // 인벤토리 상단에서 75% 지점 (하위 25%)을 기준으로 삼음 (반 + 반의 반)
                    float thresholdY = Mathf.Lerp(invCorners[2].y, invCorners[0].y, 0.75f);

                    // 슬롯이 해당 기준치보다 아래쪽에 있다면 패널을 슬롯 위(Top)로 표시
                    bool showAbove = slotRect.position.y < thresholdY;
                    
                    Vector3[] initialPanelCorners = new Vector3[4];
                    
                    if (showAbove)
                    {
                        // 패널의 Bottom-Center를 슬롯의 Top-Center에 맞춤
                        infoRect.position = topCenter;
                        infoRect.GetWorldCorners(initialPanelCorners);
                        Vector3 panelBottomCenter = (initialPanelCorners[0] + initialPanelCorners[3]) / 2f;
                        Vector3 pivotToBottomShift = infoRect.position - panelBottomCenter;
                        
                        infoRect.position = topCenter + pivotToBottomShift;
                        infoRect.anchoredPosition += new Vector2(0, 5f);
                    }
                    else
                    {
                        // 패널의 Top-Center를 슬롯의 하단(Bottom-Center)에 맞춤
                        infoRect.position = bottomCenter;
                        infoRect.GetWorldCorners(initialPanelCorners);
                        Vector3 panelTopCenter = (initialPanelCorners[1] + initialPanelCorners[2]) / 2f;
                        Vector3 pivotToTopShift = infoRect.position - panelTopCenter;
                        
                        infoRect.position = bottomCenter + pivotToTopShift;
                        infoRect.anchoredPosition += new Vector2(0, -5f);
                    }
                    
                    Vector3[] panelCorners = new Vector3[4];
                    infoRect.GetWorldCorners(panelCorners);
                    
                    Vector3 offset = Vector3.zero;

                    // 좌/우 (Width) 보정 (월드 좌표 차이로 구함)
                    if (panelCorners[0].x < invCorners[0].x)
                        offset.x = invCorners[0].x - panelCorners[0].x;
                    else if (panelCorners[2].x > invCorners[2].x)
                        offset.x = invCorners[2].x - panelCorners[2].x;
                        
                    // 가로 보정치 적용
                    infoRect.position += offset;
                }
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

            if (Magic.Store.StoreManager.Instance != null && Magic.Store.StoreManager.Instance.IsOrderContainerOpen)
            {
                return Magic.Store.StoreManager.Instance.SelectedOrderItem == item;
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
}
