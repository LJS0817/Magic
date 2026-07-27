using UnityEngine;
using System.Collections.Generic;

public class StoreUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform spawnArea;
    [SerializeField] private GameObject orderPrefab;
    [SerializeField] private ChatUI chatUI;
    [SerializeField] private OrderContainerUI orderContainer;

    [Header("Pooling Settings")]
    [SerializeField] private int _poolingCount = 10;
    private Queue<OrderUI> _orderPool = new Queue<OrderUI>();
    private bool _isPoolInitialized = false;

    bool isOpen = false;
    public bool IsOpened => isOpen;

    private void Start()
    {
        if (!chatUI.gameObject.activeInHierarchy) chatUI.gameObject.SetActive(true);
    }

    private void InitializePool()
    {
        if (_isPoolInitialized || orderPrefab == null || spawnArea == null) return;
        _isPoolInitialized = true;

        int count = StoreManager.Instance != null ? StoreManager.Instance.PoolingCount : _poolingCount;
        for (int i = 0; i < count; i++)
        {
            GameObject orderObj = Instantiate(orderPrefab, spawnArea);
            orderObj.SetActive(false);
            OrderUI orderUI = orderObj.GetComponent<OrderUI>();
            if (orderUI != null)
            {
                _orderPool.Enqueue(orderUI);
            }
        }
    }

    public void SpawnOrder(CustomerOrder newOrder)
    {
        if (spawnArea == null || orderPrefab == null)
        {
            Debug.LogWarning("Spawn Area or Order Prefab is not assigned.");
            return;
        }

        if (!_isPoolInitialized) InitializePool();

        OrderUI orderUI = GetOrCreateOrderUI();
        if (orderUI != null)
        {
            orderUI.Initialize(newOrder, OpenChatForOrder, ReturnToPool);
        }
    }

    private OrderUI GetOrCreateOrderUI()
    {
        if (_orderPool.Count > 0)
        {
            OrderUI orderUI = _orderPool.Dequeue();
            orderUI.gameObject.SetActive(true);
            orderUI.transform.SetAsLastSibling();
            return orderUI;
        }
        else
        {
            // 풀에 대기 중인 오브젝트가 부족할 경우 새롭게 생성 (동적 확장)
            Debug.Log("[StoreUIController] 풀에 대기 중인 오브젝트가 부족하여 새 OrderUI를 인스턴스화합니다.");
            GameObject orderObj = Instantiate(orderPrefab, spawnArea);
            return orderObj.GetComponent<OrderUI>();
        }
    }

    public void ReturnToPool(OrderUI orderUI)
    {
        if (orderUI != null)
        {
            orderUI.Deselect(immediate: true);
            orderUI.gameObject.SetActive(false);
            _orderPool.Enqueue(orderUI);
        }
    }

    // Overload for testing purposes
    [ContextMenu("Test Spawn Order")]
    private void TestSpawnOrder()
    {
        CustomerOrder dummyOrder = new CustomerOrder(
            "I need a fire spell", 
            "Fireball", 
            SpellElement.Fire, 
            100, 
            120, 
            100, 
            false, 
            CustomerFaction.Peasant
        );
        SpawnOrder(dummyOrder);
    }

    public void OpenChatForOrder(OrderUI orderUI)
    {
        if (chatUI != null)
        {
            // Bring ChatUI to front if it's on the same canvas level
            chatUI.transform.SetAsLastSibling();
            chatUI.OpenChat(orderUI);
        }
    }

    public void ToggleUI()
    {
        if (isOpen) Close();
        else Open();
        isOpen = !isOpen;
    }

    public void Open()
    {
        orderContainer.Open();

        if (StoreManager.Instance != null) StoreManager.Instance.IsOrderContainerOpen = true;
        if (InventoryManager.Instance != null) InventoryManager.Instance.NotifyInventoryChanged();
    }

    public void Close()
    {
        chatUI.CloseChat();
        orderContainer.Close();

        if (StoreManager.Instance != null)
        {
            StoreManager.Instance.IsOrderContainerOpen = false;
            StoreManager.Instance.SelectedOrderItem = null;
        }
        if (InventoryManager.Instance != null) InventoryManager.Instance.NotifyInventoryChanged();
    }

    public bool SelectItem(ItemInstance item)
    {
        if (chatUI != null)
        {
            return chatUI.SelectItem(item);
        }
        return false;
    }
}

