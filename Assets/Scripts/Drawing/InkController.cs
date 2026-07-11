using System;
using System.Collections.Generic;
using UnityEngine;

namespace Magic.Drawing
{
    public class InkController : MonoBehaviour
    {
        [Header("Visuals")]
        public RectTransform inkBottleVisual;



        public Item_Ink CurrentInk
        {
            get
            {
                return InventoryManager.Instance != null ? InventoryManager.Instance.EquippedInk : null;
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

            if (ink.currentAmount <= 0)
            {
                Debug.LogWarning("[InkController] 잉크병이 바닥났습니다! 병이 버려집니다.");
                ConsumeInkBottle();
            }
        }

        public void ConsumeInkBottle()
        {
            Item_Ink ink = CurrentInk;
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
