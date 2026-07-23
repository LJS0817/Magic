using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Magic.Inventory;
using Magic.Market;

namespace Magic.UI.Market
{
    public class MarketItemSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image iconImage;
        public TMP_Text nameText;
        public TMP_Text priceText;
        public Button buyButton;

        private ItemDataSO _itemData;

        public void Setup(ItemDataSO itemData)
        {
            _itemData = itemData;

            if (_itemData == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (iconImage != null && _itemData.itemIcon != null)
            {
                iconImage.sprite = _itemData.itemIcon;
                iconImage.enabled = true;
            }
            else if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            if (nameText != null)
            {
                nameText.text = _itemData.itemName;
            }

            if (priceText != null)
            {
                int price = _itemData.basePriceInCopper;
                if (price <= 0) price = 100; // Default fallback if not set
                
                // Apply visual discount calculation for display if needed
                if (Magic.Upgrade.UpgradeManager.Instance != null)
                {
                    float discountPercent = Magic.Upgrade.UpgradeManager.Instance.GetTotalUpgradeValue(Magic.Upgrade.UpgradeType.ShopDiscount);
                    float discountMultiplier = 1f - Mathf.Clamp(discountPercent / 100f, 0f, 0.9f);
                    price = Mathf.RoundToInt(price * discountMultiplier);
                }

                priceText.text = $"{price} Cu";
            }

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(OnBuyClicked);
            }
        }

        private void OnBuyClicked()
        {
            if (_itemData != null && MarketManager.Instance != null)
            {
                bool success = MarketManager.Instance.BuyItem(_itemData);
                if (success)
                {
                    // Optional: Play buy sound, show particle, etc.
                }
            }
        }
    }
}
