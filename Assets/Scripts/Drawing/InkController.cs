using System;
using System.Collections.Generic;
using UnityEngine;
using Magic.Inventory;
using DG.Tweening;

namespace Magic.Drawing
{
    public class InkController : MonoBehaviour
    {
        [Header("Visuals")]
        public RectTransform inkBottleVisual;
        public RectTransform inkAmountVisual;
        const float inkAmountVisualMinHeight = 19.05f;
        float inkAmountVisualMaxHeight = 100f;

        public Item_Ink CurrentInk
        {
            get
            {
                return InventoryManager.Instance != null ? InventoryManager.Instance.EquippedInk : null;
            }
        }

        void Start()
        {
            inkAmountVisualMaxHeight = inkAmountVisual.rect.height;
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInkEquipped += HandleInkEquipped;
            }
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInkEquipped -= HandleInkEquipped;
            }
        }

        private void HandleInkEquipped(Item_Ink newInk)
        {
            if (newInk != null && inkAmountVisual != null)
            {
                float maxAmount = newInk.InkData != null ? newInk.InkData.maxAmount : 100f;
                float targetRatio = maxAmount > 0f ? Mathf.Clamp01(newInk.currentAmount / maxAmount) : 0f;
                float targetHeight = Mathf.Lerp(inkAmountVisualMinHeight, inkAmountVisualMaxHeight, targetRatio);

                // immediately change height
                inkAmountVisual.sizeDelta = new Vector2(inkAmountVisual.sizeDelta.x, targetHeight);

                // if amount > 0, instantly show (alpha 1)
                if (newInk.currentAmount > 0)
                {
                    SetInkVisualAlpha(1f, 0f);
                }
                else
                {
                    SetInkVisualAlpha(0f, 0f);
                }
            }
        }

        private void SetInkVisualAlpha(float targetAlpha, float duration)
        {
            if (inkAmountVisual == null) return;
            
            UnityEngine.UI.Image img = inkAmountVisual.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                if (duration > 0f)
                    img.DOFade(targetAlpha, duration);
                else
                {
                    Color c = img.color;
                    c.a = targetAlpha;
                    img.color = c;
                }
            }
            else
            {
                CanvasGroup cg = inkAmountVisual.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    if (duration > 0f)
                        cg.DOFade(targetAlpha, duration);
                    else
                        cg.alpha = targetAlpha;
                }
            }
        }

        public void TryRefillPen(Item_Pen pen)
        {
            if (pen == null || (pen.PenData != null && pen.PenData.consumesMana)) return;
            Item_Ink ink = CurrentInk;
            if (ink == null || ink.currentAmount <= 0) return;

            float maxCap = pen.PenData != null ? pen.PenData.maxInkCapacity : 0f;
            float amountNeeded = maxCap - pen.currentInkCapacity;
            if (amountNeeded <= 0) return;

            ExecuteRefillLogic(pen, ink, amountNeeded);
        }

        private void ExecuteRefillLogic(Item_Pen pen, Item_Ink ink, float amountNeeded)
        {
            if (ink.currentAmount >= amountNeeded)
            {
                ink.currentAmount -= amountNeeded;
                pen.currentInkCapacity += amountNeeded;
            }
            else
            {
                pen.currentInkCapacity += ink.currentAmount;
                ink.currentAmount = 0;
            }

            float maxAmount = ink.InkData != null ? ink.InkData.maxAmount : 100f;
            float targetRatio = maxAmount > 0f ? Mathf.Clamp01(ink.currentAmount / maxAmount) : 0f;
            float targetHeight = Mathf.Lerp(inkAmountVisualMinHeight, inkAmountVisualMaxHeight, targetRatio);

            if (inkAmountVisual != null)
            {
                inkAmountVisual.DOSizeDelta(new Vector2(inkAmountVisual.sizeDelta.x, targetHeight), 0.5f)
                    .SetEase(Ease.OutQuad);

                if (ink.currentAmount <= 0)
                {
                    SetInkVisualAlpha(0f, 0.5f);
                }
            }
        }

        public void ConsumeInkBottle(Item_Ink inkToRemove = null)
        {
            Item_Ink ink = inkToRemove ?? CurrentInk;
            if (ink != null && ink.currentAmount <= 0)
            {
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.RemoveItem(ink);
                }
            }
        }

        public Color GetLineColor(Item_Pen currentPen)
        {
            if (currentPen != null && currentPen.PenData != null && currentPen.PenData.consumesMana)
            {
                return Color.cyan;
            }
            
            Item_Ink currentInk = CurrentInk;
            if (currentInk != null && currentInk.InkData != null)
            {
                return currentInk.InkData.inkColor;
            }
            
            return Color.black; // 기본 색상
        }
    }
}
