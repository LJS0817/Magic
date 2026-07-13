using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Magic.Inventory
{
    public class InventoryUIController : MonoBehaviour
    {
        [Header("Slot Settings")]
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private List<Sprite> _slotBackgrounds;
        [SerializeField] private Sprite _emptySlotIcon;

        [Header("Info Panel")]
        [SerializeField] private GameObject _infoPanel;
        [SerializeField] private Image _infoIconImage;
        [SerializeField] private TMP_Text _infoNameText;
        [SerializeField] private TMP_Text _infoDescriptionText;
        [SerializeField] private Button _equipButton;
        [SerializeField] private TMP_Text _equipButtonText;

        [Header("Pagination")]
        [SerializeField] private int _slotsPerPage = 20;
        [SerializeField] private Button _prevPageButton;
        [SerializeField] private Button _nextPageButton;
        [SerializeField] private TMP_Text _pageText;

        private List<InventorySlot> _slotPool = new List<InventorySlot>();
        private Dictionary<ItemInstance, Sprite> _itemBackgroundMap = new Dictionary<ItemInstance, Sprite>();
        private int _currentPage = 0;
        private ItemInstance _selectedItem;

        private void Awake()
        {
            if (_prevPageButton != null) _prevPageButton.onClick.AddListener(OnPrevPage);
            if (_nextPageButton != null) _nextPageButton.onClick.AddListener(OnNextPage);
            if (_equipButton != null) _equipButton.onClick.AddListener(OnEquipButtonClicked);
        }

        private void Start()
        {
            if (_infoPanel != null)
                _infoPanel.SetActive(false); // 처음에는 숨김
            
            InventoryManager.Instance.OnInventoryChanged += RefreshInventory;
            InventoryManager.Instance.OnScrollEquipped += OnScrollEquipped;
            InventoryManager.Instance.OnInkEquipped += OnInkEquipped;
            InventoryManager.Instance.OnPenEquipped += OnPenEquipped;
            
            // 처음에 인벤토리에 있는 아이템들을 UI에 반영
            RefreshInventory();
        }

        private void OnScrollEquipped(Item_Scroll item) => RefreshInventory();
        private void OnInkEquipped(Item_Ink item) => RefreshInventory();
        private void OnPenEquipped(Item_Pen item) => RefreshInventory();

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged -= RefreshInventory;
                InventoryManager.Instance.OnScrollEquipped -= OnScrollEquipped;
                InventoryManager.Instance.OnInkEquipped -= OnInkEquipped;
                InventoryManager.Instance.OnPenEquipped -= OnPenEquipped;
            }
        }

        private void OnEnable()
        {
            // UI 창이 켜질 때마다 제일 첫 페이지로 가고 싶다면 _currentPage = 0; 추가 가능
            RefreshInventory();
        }

        public void RefreshInventory()
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

            // 전체 페이지 계산 및 현재 페이지 유효성 검사
            int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)items.Count / _slotsPerPage));
            if (_currentPage >= totalPages) _currentPage = totalPages - 1;
            if (_currentPage < 0) _currentPage = 0;

            int startIndex = _currentPage * _slotsPerPage;
            int endIndex = Mathf.Min(startIndex + _slotsPerPage, items.Count);
            int displayCount = endIndex - startIndex;

            // 현재 페이지의 슬롯들을 전부(최대 _slotsPerPage) 활성화하고 빈 칸에는 null 전달
            for (int i = 0; i < _slotsPerPage; i++)
            {
                InventorySlot slot = GetOrCreateSlot(i);
                slot.gameObject.SetActive(true);
                slot.Initialize(OnSlotClicked);

                if (i < displayCount)
                {
                    int itemIndex = startIndex + i;
                    ItemInstance currentItem = items[itemIndex];
                    bool isEquipped = IsItemEquipped(currentItem);
                    slot.SetInfo(currentItem, GetSlotBackground(currentItem), _emptySlotIcon, isEquipped);
                }
                else
                {
                    // 빈 슬롯
                    slot.SetInfo(null, GetSlotBackground(null), _emptySlotIcon, false);
                }
            }

            // 남는 슬롯들은 비활성화 (풀의 크기는 최대 _slotsPerPage 까지만 커짐)
            for (int i = _slotsPerPage; i < _slotPool.Count; i++)
            {
                _slotPool[i].gameObject.SetActive(false);
            }

            // 페이지 UI 업데이트
            if (_pageText != null) _pageText.text = $"{_currentPage + 1} / {totalPages}";
            if (_prevPageButton != null) _prevPageButton.interactable = _currentPage > 0;
            if (_nextPageButton != null) _nextPageButton.interactable = _currentPage < totalPages - 1;

            UpdateEquipButtonState();
        }

        private void OnPrevPage()
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                RefreshInventory();
            }
        }

        private void OnNextPage()
        {
            if (InventoryManager.Instance == null) return;
            var items = InventoryManager.Instance.items;
            int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)items.Count / _slotsPerPage));
            
            if (_currentPage < totalPages - 1)
            {
                _currentPage++;
                RefreshInventory();
            }
        }

        private InventorySlot GetOrCreateSlot(int index)
        {
            // 인덱스에 해당하는 슬롯이 이미 풀에 있으면 반환
            if (index < _slotPool.Count)
            {
                return _slotPool[index];
            }

            // 부족하면 새로 생성 후 풀에 추가
            GameObject slotObj = Instantiate(_slotPrefab, _slotContainer);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            _slotPool.Add(slot);
            
            return slot;
        }

        private void OnSlotClicked(ItemInstance item)
        {
            _selectedItem = item;

            // 슬롯 클릭 시 우측/하단 등에 정보 패널을 띄우고 데이터 적용
            if (_infoPanel != null)
                _infoPanel.SetActive(true);

            if (_infoIconImage != null)
                _infoIconImage.sprite = item.ItemIcon;

            if (_infoNameText != null)
                _infoNameText.text = item.ItemName;

            if (_infoDescriptionText != null)
                _infoDescriptionText.text = item.ItemDescription;

            UpdateEquipButtonState();
        }

        private void OnEquipButtonClicked()
        {
            if (_selectedItem == null || InventoryManager.Instance == null) return;
            
            var inv = InventoryManager.Instance;
            if (_selectedItem is Item_Scroll scroll) inv.EquippedScroll = scroll;
            else if (_selectedItem is Item_Ink ink) inv.EquippedInk = ink;
            else if (_selectedItem is Item_Pen pen) inv.EquippedPen = pen;
        }

        private bool IsItemEquipped(ItemInstance item)
        {
            if (item == null) return false;
            var inv = InventoryManager.Instance;
            if (inv == null) return false;
            
            if (item is Item_Scroll scroll) return inv.EquippedScroll == scroll;
            if (item is Item_Ink ink) return inv.EquippedInk == ink;
            if (item is Item_Pen pen) return inv.EquippedPen == pen;
            
            return false;
        }

        private void UpdateEquipButtonState()
        {
            if (_equipButton == null || _selectedItem == null) return;
            
            bool isEquipped = IsItemEquipped(_selectedItem);
            
            if (_equipButtonText != null)
            {
                _equipButtonText.text = isEquipped ? "장착 중" : "장착하기";
            }
            
            _equipButton.interactable = !isEquipped;
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
