using UnityEngine;
using System.Collections.Generic;

namespace Magic.Inventory
{
    public class StoreDebugUI : MonoBehaviour
    {
        private Rect windowRect = new Rect(660, 20, 350, 500);
        private bool showWindow = true;
        
        private int selectedTab = 0; // 0: 판매, 1: 구매
        private string[] tabNames = new string[] { "판매 (Sell)", "구매 (Buy)" };

        private Vector2 scrollPosition;

        void OnGUI()
        {
            if (!showWindow)
            {
                if (GUI.Button(new Rect(660, 20, 100, 30), "상점 UI 열기"))
                {
                    showWindow = true;
                }
                return;
            }

            windowRect = GUI.Window(2, windowRect, DrawStoreWindow, "마법 상점 (디버그)");
        }

        void DrawStoreWindow(int windowID)
        {
            if (StoreManager.Instance == null || InventoryManager.Instance == null)
            {
                GUILayout.Label("StoreManager 또는 InventoryManager가 없습니다.");
                if (GUILayout.Button("닫기")) showWindow = false;
                GUI.DragWindow();
                return;
            }

            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            GUILayout.Space(10);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            if (selectedTab == 0)
            {
                DrawSellTab();
            }
            else
            {
                DrawBuyTab();
            }

            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("닫기"))
            {
                showWindow = false;
            }

            GUI.DragWindow();
        }

        private void DrawSellTab()
        {
            var inv = InventoryManager.Instance;
            GUILayout.Label("<b>[ 인벤토리 판매 가능 아이템 ]</b>");

            int sellableCount = 0;
            // 리스트 수정 중 오류 방지를 위해 복사본 순회
            List<ItemInstance> itemsToSell = new List<ItemInstance>(inv.items);

            foreach (var item in itemsToSell)
            {
                // 재료나 빈 스크롤 등 판매 가능한 것만 필터링할 수 있지만 지금은 가격이 0보다 큰 건 다 보여줌
                int price = item.data != null ? item.data.basePriceInCopper : 0;
                
                if (price > 0 && item.Type == ItemType.Material) // 주로 마석/전리품
                {
                    sellableCount++;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"- {item.ItemName}");
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"<color=orange>{price} Cu</color>");
                    if (GUILayout.Button("팔기", GUILayout.Width(50)))
                    {
                        StoreManager.Instance.SellItem(item);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            if (sellableCount == 0)
            {
                GUILayout.Label("판매할 전리품이 없습니다.");
            }
        }

        private void DrawBuyTab()
        {
            var db = InventoryManager.Instance.itemDatabase;
            if (db == null)
            {
                GUILayout.Label("ItemDatabase가 연결되지 않아 상점 물품을 불러올 수 없습니다.");
                return;
            }

            GUILayout.Label("<b>[ 상점 물품 리스트 ]</b>");

            // 임시로 DB에 있는 모든 아이템(스크롤, 잉크, 펜)을 판매한다고 가정
            // (실제 게임에서는 판매용 리스트(StoreCatalogSO 등)를 따로 만들어야 합니다)

            DrawCategory<ItemScrollSO>("빈 스크롤 & 완성형 스크롤", db.scrolls.ToArray());
            DrawCategory<ItemInkSO>("마법 잉크", db.inks.ToArray());
            DrawCategory<ItemPenSO>("마법 펜", db.pens.ToArray());
        }

        private void DrawCategory<T>(string title, T[] items) where T : ItemDataSO
        {
            GUILayout.Space(5);
            GUILayout.Label($"<b>{title}</b>");

            if (items == null || items.Length == 0)
            {
                GUILayout.Label("- 상품 없음");
                return;
            }

            foreach (var data in items)
            {
                if (data == null) continue;
                
                int price = data.basePriceInCopper;
                if (price <= 0) price = 100; // 가격이 미설정된 경우 임시 100 동화

                GUILayout.BeginHorizontal();
                GUILayout.Label($"- {data.itemName}");
                GUILayout.FlexibleSpace();
                GUILayout.Label($"<color=orange>{price} Cu</color>");
                if (GUILayout.Button("사기", GUILayout.Width(50)))
                {
                    // DB 원본의 가격을 덮어쓰지 않도록 주의 (실제 기획에서는 에셋에 저장)
                    data.basePriceInCopper = price; 
                    StoreManager.Instance.BuyItem(data);
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}
