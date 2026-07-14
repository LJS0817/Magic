using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Magic.Inventory;

namespace Magic.Upgrade
{
    public class UpgradeUIController : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] CanvasGroup _canvasGroup;
        Canvas _canvas;
        public RectTransform nodeContainer; // Where node prefabs are spawned
        public RectTransform lineContainer; // Where lines are drawn (should be behind nodes)

        [Header("Prefabs")]
        public GameObject nodePrefab;
        public GameObject linePrefab;

        [Header("Detail Panel")]
        public GameObject detailPanel;
        public TMP_Text detailNameText;
        public TMP_Text detailDescText;
        public TMP_Text detailCostText;
        public Button purchaseButton;

        private List<UpgradeNodeUI> spawnedNodes = new List<UpgradeNodeUI>();
        private UpgradeNodeUI selectedNodeUI;

        private void Start()
        {
            if (detailPanel != null)
                detailPanel.SetActive(false);

            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);

            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnUpgradeUnlocked += RefreshAllNodes;
                GenerateSkillTree();
            }
            if (!_canvasGroup.gameObject.activeInHierarchy) _canvasGroup.gameObject.SetActive(true);
            _canvas = _canvasGroup.GetComponent<Canvas>();
            Close();
        }

        public void Open()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _canvas.enabled = true;
            Magic.Drawing.DrawingManager.IsDrawingBlocked = true;
        }

        public void Close()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvas.enabled = false;
            Magic.Drawing.DrawingManager.IsDrawingBlocked = false;
        }

        private void OnDestroy()
        {
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnUpgradeUnlocked -= RefreshAllNodes;
            }
        }

        private void GenerateSkillTree()
        {
            if (UpgradeManager.Instance == null || UpgradeManager.Instance.allNodes == null) return;

            float minX = 0f, maxX = 0f, minY = 0f, maxY = 0f;

            // Spawn Nodes
            foreach (var nodeData in UpgradeManager.Instance.allNodes)
            {
                if (nodeData.uiPosition.x < minX) minX = nodeData.uiPosition.x;
                if (nodeData.uiPosition.x > maxX) maxX = nodeData.uiPosition.x;
                if (nodeData.uiPosition.y < minY) minY = nodeData.uiPosition.y;
                if (nodeData.uiPosition.y > maxY) maxY = nodeData.uiPosition.y;

                GameObject nodeObj = Instantiate(nodePrefab, nodeContainer);
                UpgradeNodeUI nodeUI = nodeObj.GetComponent<UpgradeNodeUI>();
                if (nodeUI != null)
                {
                    nodeUI.Setup(nodeData, this);
                    spawnedNodes.Add(nodeUI);
                }
            }

            // Automatically resize the containers so they fit inside a Scroll View perfectly
            float totalWidth = (maxX - minX) + 500f; // 500f is padding
            float totalHeight = (maxY - minY) + 500f;

            if (nodeContainer != null) 
            {
                nodeContainer.anchorMin = new Vector2(0.5f, 0.5f);
                nodeContainer.anchorMax = new Vector2(0.5f, 0.5f);
                nodeContainer.pivot = new Vector2(0.5f, 0.5f);
                nodeContainer.sizeDelta = new Vector2(totalWidth, totalHeight);
                nodeContainer.localPosition = Vector3.zero;
            }
            if (lineContainer != null) 
            {
                lineContainer.anchorMin = new Vector2(0.5f, 0.5f);
                lineContainer.anchorMax = new Vector2(0.5f, 0.5f);
                lineContainer.pivot = new Vector2(0.5f, 0.5f);
                lineContainer.sizeDelta = new Vector2(totalWidth, totalHeight);
                lineContainer.localPosition = Vector3.zero;
            }

            // Draw Lines (Parents to Children)
            foreach (var nodeUI in spawnedNodes)
            {
                var nodeData = nodeUI.nodeData;
                if (nodeData.requiredParents != null)
                {
                    foreach (var parent in nodeData.requiredParents)
                    {
                        DrawLineBetween(parent, nodeData);
                    }
                }
            }
        }

        private void DrawLineBetween(UpgradeNodeSO parentData, UpgradeNodeSO childData)
        {
            GameObject lineObj = Instantiate(linePrefab, lineContainer);
            UILineConnection lineConn = lineObj.GetComponent<UILineConnection>();
            
            if (lineConn != null)
            {
                // We assume line is always grey for simplicity, but could be dynamic based on unlock state
                lineConn.DrawLine(parentData.uiPosition, childData.uiPosition, 4f, Color.white * 0.5f);
            }
        }

        public void SelectNode(UpgradeNodeUI nodeUI)
        {
            selectedNodeUI = nodeUI;
            UpdateDetailPanel();
        }

        private void UpdateDetailPanel()
        {
            if (selectedNodeUI == null || detailPanel == null) return;
            
            detailPanel.SetActive(true);
            var data = selectedNodeUI.nodeData;

            if (detailNameText != null) detailNameText.text = data.nodeName;
            if (detailDescText != null) detailDescText.text = data.description;
            
            bool isUnlocked = UpgradeManager.Instance.IsNodeUnlocked(data);
            bool isUnlockable = UpgradeManager.Instance.IsNodeUnlockable(data);
            bool hasMoney = CurrencyManager.Instance.HasEnoughCurrency(CurrencyType.Gem, data.costAmount, false); // Only use Gem

            if (isUnlocked)
            {
                if (detailCostText != null) detailCostText.text = "Unlocked";
                if (purchaseButton != null) purchaseButton.interactable = false;
            }
            else
            {
                if (detailCostText != null) detailCostText.text = $"Cost: {data.costAmount} Gem";
                if (purchaseButton != null) purchaseButton.interactable = (isUnlockable && hasMoney);
            }
        }

        private void OnPurchaseButtonClicked()
        {
            if (selectedNodeUI == null) return;

            if (UpgradeManager.Instance.TryUnlockNode(selectedNodeUI.nodeData))
            {
                // It will trigger OnUpgradeUnlocked which calls RefreshAllNodes()
                // Update detail panel to reflect new state
                UpdateDetailPanel();
            }
        }

        private void RefreshAllNodes()
        {
            foreach (var nodeUI in spawnedNodes)
            {
                nodeUI.RefreshVisuals();
            }
            UpdateDetailPanel();
        }

        // Methods to Open/Close the UI from the Button
        public void ToggleUI()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        private void OnEnable()
        {
            RefreshAllNodes();
        }
    }
}
