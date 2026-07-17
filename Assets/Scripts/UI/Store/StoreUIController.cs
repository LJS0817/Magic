using UnityEngine;

namespace Magic.Store
{
    public class StoreUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform spawnArea;
        [SerializeField] private GameObject orderPrefab;
        [SerializeField] private ChatUI chatUI;
        [SerializeField] private Canvas parentCanvas;

        [Header("Spawn Settings")]
        [SerializeField] private float minRotationZ = -15f;
        [SerializeField] private float maxRotationZ = 15f;

        private void Start()
        {
            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
            }
        }

        public void SpawnOrder(CustomerOrder newOrder)
        {
            if (spawnArea == null || orderPrefab == null)
            {
                Debug.LogWarning("Spawn Area or Order Prefab is not assigned.");
                return;
            }

            // Instantiate prefab
            GameObject orderObj = Instantiate(orderPrefab, spawnArea);
            RectTransform rect = orderObj.GetComponent<RectTransform>();

            // Randomize position within spawn area
            // Assuming spawnArea's pivot is 0.5, 0.5
            float randomX = Random.Range(spawnArea.rect.xMin + rect.rect.width / 2f, spawnArea.rect.xMax - rect.rect.width / 2f);
            float randomY = Random.Range(spawnArea.rect.yMin + rect.rect.height / 2f, spawnArea.rect.yMax - rect.rect.height / 2f);
            rect.anchoredPosition = new Vector2(randomX, randomY);

            // Randomize rotation
            float randomRotZ = Random.Range(minRotationZ, maxRotationZ);
            rect.localRotation = Quaternion.Euler(0, 0, randomRotZ);

            // Initialize OrderUI
            OrderUI orderUI = orderObj.GetComponent<OrderUI>();
            if (orderUI != null)
            {
                orderUI.Initialize(newOrder, OpenChatForOrder, parentCanvas);
            }
        }

        // Overload for testing purposes
        [ContextMenu("Test Spawn Order")]
        private void TestSpawnOrder()
        {
            CustomerOrder dummyOrder = new CustomerOrder(
                "Test Customer", 
                "I need a fire spell", 
                "Fireball", 
                Magic.Combat.SpellElement.Fire, 
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

        public void SelectItem(Magic.Inventory.ItemInstance item)
        {
            if (chatUI != null)
            {
                chatUI.SelectItem(item);
            }
        }
    }
}
