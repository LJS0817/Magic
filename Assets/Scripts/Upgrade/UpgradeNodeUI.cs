using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradeNodeUI : MonoBehaviour, IPointerClickHandler
{
    public UpgradeNodeSO nodeData { get; private set; }
    
    [Header("UI References")]
    public Image nodeIcon; // Optional icon
    public Image nodeBackground;
    public TMP_Text nodeNameText;

    [Header("Colors")]
    public Color lockedColor = Color.gray;
    public Color unlockableColor = Color.yellow;
    public Color unlockedColor = new Color(0.2f, 0.8f, 1f); // Light blue

    private UpgradeUIController controller;

    public void Setup(UpgradeNodeSO data, UpgradeUIController uiController)
    {
        nodeData = data;
        controller = uiController;

        if (nodeNameText != null)
            nodeNameText.text = data.nodeName;

        // Set anchored position to match the SO design
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = data.uiPosition;
        }

        RefreshVisuals();
    }

    public void RefreshVisuals()
    {
        if (nodeData == null || nodeBackground == null) return;

        bool isUnlocked = UpgradeManager.Instance.IsNodeUnlocked(nodeData);
        bool isUnlockable = UpgradeManager.Instance.IsNodeUnlockable(nodeData);

        if (isUnlocked)
        {
            nodeBackground.color = unlockedColor;
        }
        else if (isUnlockable)
        {
            nodeBackground.color = unlockableColor;
        }
        else
        {
            nodeBackground.color = lockedColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (controller != null)
        {
            controller.SelectNode(this);
        }
    }
}

