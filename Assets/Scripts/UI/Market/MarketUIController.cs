using UnityEngine;
using System.Collections.Generic;

public class MarketUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject marketPanel;
    public Transform contentParent;
    public GameObject marketItemSlotPrefab;
    public GameObject categoryHeaderPrefab; // Optional: prefab for category title (e.g. TextMeshProUGUI)

    private bool _isOpened = false;
    private List<GameObject> _spawnedSlots = new List<GameObject>();

    private void Start()
    {
        if (marketPanel != null)
            marketPanel.SetActive(false);
    }

    public void Open()
    {
        if (_isOpened) return;
        _isOpened = true;

        if (marketPanel != null)
            marketPanel.SetActive(true);

        RefreshMarketItems();
    }

    public void Close()
    {
        if (!_isOpened) return;
        _isOpened = false;

        if (marketPanel != null)
            marketPanel.SetActive(false);
    }

    public void Toggle()
    {
        if (_isOpened) Close();
        else Open();
    }

    private void RefreshMarketItems()
    {
        // Clear existing
        foreach (var slot in _spawnedSlots)
        {
            Destroy(slot);
        }
        _spawnedSlots.Clear();

        var db = InventoryManager.Instance != null ? InventoryManager.Instance.itemDatabase : null;
        if (db == null)
        {
            Debug.LogWarning("[MarketUIController] ItemDatabase를 찾을 수 없습니다.");
            return;
        }

        // Populate categories
        PopulateCategory("스크롤", db.scrolls.ToArray());
        PopulateCategory("잉크", db.inks.ToArray());
        PopulateCategory("마법 펜", db.pens.ToArray());
        PopulateCategory("지팡이", db.wands.ToArray());
        PopulateCategory("물약", db.potions.ToArray());
        PopulateCategory("주머니", db.pouches.ToArray());
    }

    private void PopulateCategory<T>(string categoryName, T[] items) where T : ItemDataSO
    {
        if (items == null || items.Length == 0) return;

        // Optional: Spawn Category Header
        if (categoryHeaderPrefab != null && contentParent != null)
        {
            GameObject header = Instantiate(categoryHeaderPrefab, contentParent);
            var textComp = header.GetComponentInChildren<TMPro.TMP_Text>();
            if (textComp != null)
            {
                textComp.text = $"--- {categoryName} ---";
            }
            _spawnedSlots.Add(header);
        }

        // Spawn Slots
        foreach (var item in items)
        {
            if (item == null) continue;

            if (marketItemSlotPrefab != null && contentParent != null)
            {
                GameObject slotObj = Instantiate(marketItemSlotPrefab, contentParent);
                MarketItemSlotUI slotUI = slotObj.GetComponent<MarketItemSlotUI>();
                if (slotUI != null)
                {
                    slotUI.Setup(item);
                }
                _spawnedSlots.Add(slotObj);
            }
        }
    }
}

