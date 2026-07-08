using UnityEngine;
using System.Collections.Generic;

namespace Magic.Inventory
{
    public class InventoryDebugUI : MonoBehaviour
    {
        private Rect windowRect = new Rect(20, 20, 300, 400);
        private bool showWindow = true;

        void OnGUI()
        {
            if (!showWindow)
            {
                if (GUI.Button(new Rect(20, 20, 100, 30), "인벤토리 열기"))
                {
                    showWindow = true;
                }
                return;
            }

            windowRect = GUI.Window(0, windowRect, DrawInventoryWindow, "글로벌 인벤토리 (테스트)");
        }

        void DrawInventoryWindow(int windowID)
        {
            if (InventoryManager.Instance == null)
            {
                GUILayout.Label("InventoryManager 인스턴스가 없습니다.\n먼저 씬에 추가하거나 플레이 모드를 재시작하세요.");
                if (GUILayout.Button("닫기")) showWindow = false;
                GUI.DragWindow();
                return;
            }

            Magic.Drawing.DrawingManager dm = Object.FindObjectOfType<Magic.Drawing.DrawingManager>();
            var items = dm != null ? dm.inventory : InventoryManager.Instance.items;
            
            GUILayout.Label(dm != null && dm is Magic.Combat.CombatDrawingManager ? "현재 로드아웃 아이템" : "전체 아이템");
            GUILayout.Label($"총 개수: {items.Count}");
            GUILayout.Space(10);

            // 스크롤 목록 출력
            GUILayout.Label("<b>[스크롤]</b>");
            int scrollCount = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is Item_Scroll scroll)
                {
                    scrollCount++;
                    string state = scroll.isEmpty ? "빈 스크롤" : $"스크롤 [{scroll.spellName}]";
                    string selectedMark = (dm != null && dm.selectedScrollIndex == i) ? " <color=green>[선택됨]</color>" : "";
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"- {state} | 남은 수명: {scroll.currentDurability}{selectedMark}");
                    if (dm != null && GUILayout.Button("선택", GUILayout.Width(50)))
                    {
                        dm.SelectScroll(i);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            if (scrollCount == 0) GUILayout.Label("- 없음");

            GUILayout.Space(10);

            // 잉크 목록 출력
            GUILayout.Label("<b>[잉크]</b>");
            int inkCount = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is Item_Ink ink)
                {
                    inkCount++;
                    string selectedMark = (dm != null && dm.selectedInkIndex == i) ? " <color=green>[선택됨]</color>" : "";
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"- {ink.itemName} | 용량: {(int)ink.currentAmount}/{(int)ink.maxAmount}{selectedMark}");
                    if (dm != null && GUILayout.Button("선택", GUILayout.Width(50)))
                    {
                        dm.SelectInk(i);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            if (inkCount == 0) GUILayout.Label("- 없음");

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("닫기"))
            {
                showWindow = false;
            }

            GUI.DragWindow();
        }
    }
}
