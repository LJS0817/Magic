using UnityEngine;
using System.Collections.Generic;

namespace Magic.Inventory
{
    public class StoreDebugUI : MonoBehaviour
    {
        private Rect windowRect = new Rect(660, 20, 350, 500);
        private bool showWindow = true;
        
        private int selectedTab = 0; // 0: 판매, 1: 구매
        private string[] tabNames = new string[] { "손님 주문 (Orders)", "상인 (Merchant)" };

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

        private string askingPriceStr = "150";
        private string haggleMessage = "";
        private string selectedOrderId = "";

        private void DrawSellTab()
        {
            var store = StoreManager.Instance;
            var inv = InventoryManager.Instance;

            if (store.activeOrders != null && store.activeOrders.Count > 0)
            {
                foreach (var kvp in store.activeOrders)
                {
                    var order = kvp.Value;
                    string elementText = order.requestedElement == Magic.Combat.SpellElement.None ? "무속성" : order.requestedElement.ToString();
                    
                    GUILayout.BeginVertical("box");
                    GUILayout.Label($"<b>[ 손님: {order.customerName} ({order.faction}) - 상태: {order.state} ]</b>");
                    GUILayout.Label($"\"<i>{order.orderDescription}</i>\"");
                    GUILayout.Label($"<b>요구 사항:</b> [{elementText}] {order.requestedSpellName} 스크롤");
                    
                    if (order.state == Magic.Store.OrderState.Received || order.state == Magic.Store.OrderState.Haggling)
                    {
                        GUILayout.Label($"<b>제시된 예산:</b> {order.claimedBudget} 동화 (진짜 예산: {order.trueBudget}, 블러핑: {order.isBluffing})");
                        GUILayout.Label($"<b>인내심:</b> {order.patience}");
                        
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("흥정가:", GUILayout.Width(45));
                        askingPriceStr = GUILayout.TextField(askingPriceStr, GUILayout.Width(50));
                        if (GUILayout.Button("가격 제시", GUILayout.Width(80)))
                        {
                            if (int.TryParse(askingPriceStr, out int price))
                            {
                                store.HaggleOrder(order.orderID, price);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    else if (order.state == Magic.Store.OrderState.Pending)
                    {
                        GUILayout.Label($"<b>타결 가격:</b> <color=orange>{order.agreedPrice} 동화</color>");
                        GUILayout.Label("스크롤을 더블 클릭(여기서는 버튼 클릭)하여 배송하세요.");
                        
                        // 제출 가능 스크롤 목록
                        int scrollCount = 0;
                        List<ItemInstance> items = new List<ItemInstance>(inv.items);
                        foreach (var item in items)
                        {
                            if (item is Item_Scroll scroll && !scroll.isEmpty)
                            {
                                scrollCount++;
                                GUILayout.BeginHorizontal();
                                string spellName = scroll.ScrollData != null ? scroll.ScrollData.spellName : "Unknown";
                                string scrollElem = scroll.ScrollData != null && scroll.ScrollData.scrollElement != Magic.Combat.SpellElement.None ? scroll.ScrollData.scrollElement.ToString() : "무속성";
                                
                                GUILayout.Label($"- {item.ItemName} ([{scrollElem}] {spellName})");
                                GUILayout.FlexibleSpace();
                                
                                if (GUILayout.Button("배송", GUILayout.Width(50)))
                                {
                                    bool success = store.DeliverOrder(order.orderID, item);
                                    if (success) haggleMessage = $"<color=green>거래 성사! 배송 완료.</color>";
                                    else haggleMessage = $"<color=red>거래 실패 또는 사기 적발!</color>";
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                        if (scrollCount == 0) GUILayout.Label("- 배송할 수 있는 완성된 스크롤이 없습니다.");
                    }

                    // 채팅 이력 (최근 3개만 표시)
                    if (order.chatHistory.Count > 0)
                    {
                        GUILayout.Label("<color=gray>--- 대화 로그 ---</color>");
                        int startIdx = Mathf.Max(0, order.chatHistory.Count - 3);
                        for (int i = startIdx; i < order.chatHistory.Count; i++)
                        {
                            GUILayout.Label($"<color=silver>{order.chatHistory[i]}</color>");
                        }
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(10);
                }
            }
            else
            {
                GUILayout.Label("현재 대기 중인 주문이 없습니다.");
                if (GUILayout.Button("새 손님 받기"))
                {
                    store.GenerateRandomOrder();
                }
            }

            if (!string.IsNullOrEmpty(haggleMessage))
            {
                GUILayout.Space(10);
                GUILayout.Label(haggleMessage);
            }
        }

        private void DrawBuyTab()
        {
            var db = InventoryManager.Instance.itemDatabase;
            var inv = InventoryManager.Instance;

            GUILayout.Label("<b>[ 전리품 판매 ]</b>");
            int sellableCount = 0;
            List<ItemInstance> itemsToSell = new List<ItemInstance>(inv.items);
            foreach (var item in itemsToSell)
            {
                int price = item.data != null ? item.data.basePriceInCopper : 0;
                if (price > 0 && item.Type == ItemType.Material)
                {
                    sellableCount++;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"- {item.ItemName}");
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"<color=orange>{price} Cu</color>");
                    if (GUILayout.Button("팔기", GUILayout.Width(50)))
                    {
                        InventoryManager.Instance.RemoveItem(item);
                        CurrencyManager.Instance.AddCurrency(CurrencyType.Copper, price);
                        CurrencyManager.Instance.CompressCurrency();
                        Debug.Log($"<color=cyan>[Store] {item.ItemName}을(를) {price} 동화에 판매했습니다.</color>");
                    }
                    GUILayout.EndHorizontal();
                }
            }
            if (sellableCount == 0) GUILayout.Label("- 판매할 전리품이 없습니다.");

            GUILayout.Space(10);

            if (db == null)
            {
                GUILayout.Label("ItemDatabase가 연결되지 않아 상점 물품을 불러올 수 없습니다.");
                return;
            }

            GUILayout.Label("<b>[ 상점 물품 리스트 ]</b>");
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
