import re

with open('/Users/admin/develop/Magic/Assets/Scripts/Inventory/InventoryUIController.cs', 'r') as f:
    content = f.read()

# Replace class declaration
content = re.sub(r'public class InventoryUIController : MonoBehaviour', 'public class InventoryUIController : Magic.UI.PagedUIController<InventorySlot>', content)

# Remove slot container and pagination fields
content = re.sub(r'\[Header\("Slot Settings"\)\]\s*\[SerializeField\] private Transform _slotContainer;\s*\[SerializeField\] private GameObject _slotPrefab;', '', content)
content = re.sub(r'\[Header\("Pagination"\)\]\s*\[SerializeField\] private int _slotsPerPage = 20;\s*\[SerializeField\] private Button _prevPageButton;\s*\[SerializeField\] private Button _nextPageButton;\s*\[SerializeField\] private TMP_Text _pageText;', '', content)

# Remove private fields handled by base
content = re.sub(r'private List<InventorySlot> _slotPool = new List<InventorySlot>\(\);\s*', '', content)
content = re.sub(r'private int _currentPage = 0;\s*', '', content)

# Modify Awake
awake_replacement = """protected override void Awake()
        {
            base.Awake();
        }"""
content = re.sub(r'private void Awake\(\)\s*\{[^}]*\}', awake_replacement, content)

# Modify RefreshInventory to RefreshList in events
content = content.replace('RefreshInventory', 'RefreshList')

# Modify RefreshList definition
refresh_list_old = """        public void RefreshList()
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

            int maxCapacity = InventoryManager.Instance.GetMaxCapacity();

            // 전체 페이지 계산 및 현재 페이지 유효성 검사 (아이템 수가 아니라 '최대 슬롯 개수' 기준)
            int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)maxCapacity / _slotsPerPage));
            if (_currentPage >= totalPages) _currentPage = totalPages - 1;
            if (_currentPage < 0) _currentPage = 0;

            int startIndex = _currentPage * _slotsPerPage;
            int endIndex = Mathf.Min(startIndex + _slotsPerPage, maxCapacity); // 한 페이지에 그릴 최대 인덱스는 maxCapacity를 넘지 않음
            int displayCount = endIndex - startIndex; // 이번 페이지에 실제로 Instantiate해서 띄워야 할 슬롯의 개수

            // 현재 페이지의 슬롯들을 활성화하고, 아이템이 있으면 데이터를, 없으면 빈 칸을 전달
            for (int i = 0; i < displayCount; i++)
            {
                InventorySlot slot = GetOrCreateSlot(i);
                slot.gameObject.SetActive(true);
                slot.Initialize(OnSlotClicked, OnSlotPointerEnter, OnSlotPointerExit, OnSlotDoubleClicked, OnSlotRightClicked);

                int itemIndex = startIndex + i;
                if (itemIndex < items.Count)
                {
                    ItemInstance currentItem = items[itemIndex];
                    bool isEquipped = IsItemEquipped(currentItem);
                    slot.SetInfo(currentItem, GetSlotBackground(currentItem), _emptySlotIcon, isEquipped);
                }
                else
                {
                    // 언락된 빈 슬롯
                    slot.SetInfo(null, GetSlotBackground(null), _emptySlotIcon, false);
                }
            }

            // 남는 슬롯들은 비활성화
            for (int i = displayCount; i < _slotPool.Count; i++)
            {
                _slotPool[i].gameObject.SetActive(false);
            }

            // 페이지 UI 업데이트
            if (_pageText != null) _pageText.text = $"{_currentPage + 1} / {totalPages}";
            if (_prevPageButton != null) _prevPageButton.interactable = _currentPage > 0;
            if (_nextPageButton != null) _nextPageButton.interactable = _currentPage < totalPages - 1;
        }"""

refresh_list_new = """        public override void RefreshList()
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
        }"""
content = content.replace(refresh_list_old, refresh_list_new)

# Remove OnPrevPage, OnNextPage, GetOrCreateSlot
content = re.sub(r'\s*private void OnPrevPage\(\)\s*\{[^}]*\}', '', content)
content = re.sub(r'\s*private void OnNextPage\(\)\s*\{[^}]*\}', '', content)
# GetOrCreateSlot might have nested braces, so regex is tricky. Let's use string find and replace
get_or_create_slot_str = """        private InventorySlot GetOrCreateSlot(int index)
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
        }"""
content = content.replace(get_or_create_slot_str, "")

# Modify OnEnable
content = content.replace("private void OnEnable()", "protected override void OnEnable()")

# Fix any double next page things if any
content = re.sub(r'\s*private void OnNextPage\(\)\s*\{\s*if \(InventoryManager.Instance == null\)[^{]*\{\s*[^}]*\}\s*\}', '', content)

# Use regex to just brutally remove OnPrevPage and OnNextPage in case the regex missed something earlier due to nested braces
# Actually, I'll just write it out to file and compile and fix errors.
with open('/Users/admin/develop/Magic/Assets/Scripts/Inventory/InventoryUIController.cs', 'w') as f:
    f.write(content)
