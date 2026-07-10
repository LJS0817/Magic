using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Magic.Inventory
{
    public class CombatLoadoutUI : MonoBehaviour
    {
        private Rect windowRect = new Rect(Screen.width / 2 - 400, Screen.height / 2 - 300, 800, 600);
        private bool showUI = false;

        private Vector2 allItemsScroll;
        private Vector2 loadoutScroll;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L)) // L키로 로드아웃 UI 열기/닫기 (임시)
            {
                showUI = !showUI;
            }
        }

        void OnGUI()
        {
            if (!showUI)
            {
                GUI.Label(new Rect(Screen.width / 2 - 100, 10, 300, 30), "[L] 키를 눌러 전투 준비(Loadout) 열기");
                return;
            }

            windowRect = GUI.Window(1, windowRect, DrawLoadoutWindow, "전투 준비 (Combat Loadout)");
        }

        void DrawLoadoutWindow(int windowID)
        {
            if (InventoryManager.Instance == null)
            {
                GUILayout.Label("InventoryManager 인스턴스가 없습니다.");
                if (GUILayout.Button("닫기")) showUI = false;
                return;
            }

            var allItems = InventoryManager.Instance.items;
            var loadout = InventoryManager.Instance.combatLoadout;

            GUILayout.BeginHorizontal();

            // 왼쪽: 전체 인벤토리
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(380));
            GUILayout.Label($"<b>전체 인벤토리 ({allItems.Count})</b>", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
            
            allItemsScroll = GUILayout.BeginScrollView(allItemsScroll);
            foreach (var item in allItems)
            {
                bool isInLoadout = loadout.Contains(item);
                GUI.enabled = !isInLoadout; // 이미 장착 중이면 비활성화

                GUILayout.BeginHorizontal();
                string itemName = GetDisplayName(item);
                GUILayout.Label($"- {itemName}");

                if (GUILayout.Button("장착", GUILayout.Width(60)))
                {
                    InventoryManager.Instance.AddToLoadout(item);
                }
                GUILayout.EndHorizontal();
                
                GUI.enabled = true;
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(20);

            // 오른쪽: 전투 로드아웃
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(380));
            GUILayout.Label($"<b>선택된 로드아웃 ({loadout.Count})</b>", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
            
            loadoutScroll = GUILayout.BeginScrollView(loadoutScroll);
            
            // 리스트에서 삭제할 때 오류 방지를 위해 복사본 순회 또는 역순 순회
            List<ItemInstance> loadoutCopy = new List<ItemInstance>(loadout);
            foreach (var item in loadoutCopy)
            {
                GUILayout.BeginHorizontal();
                string itemName = GetDisplayName(item);
                GUILayout.Label($"- {itemName}");

                if (GUILayout.Button("해제", GUILayout.Width(60)))
                {
                    InventoryManager.Instance.RemoveFromLoadout(item);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("전부 해제", GUILayout.Height(30)))
            {
                InventoryManager.Instance.ClearLoadout();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button("준비 완료 (전투 시작!)", GUILayout.Height(40)))
            {
                showUI = false;
                Debug.Log($"<color=green>[전투 준비] {loadout.Count}개의 아이템을 가지고 전투에 돌입합니다.</color>");
                SceneManager.LoadScene("CombatGameScene");
            }

            GUI.DragWindow();
        }

        private string GetDisplayName(ItemInstance item)
        {
            if (item is Item_Scroll scroll)
            {
                string spell = scroll.ScrollData != null ? scroll.ScrollData.spellName : "Unknown";
                return scroll.isEmpty ? $"[빈 스크롤] 수명: {scroll.currentDurability}" : $"[스크롤] {spell} (수명: {scroll.currentDurability})";
            }
            else if (item is Item_Ink ink)
            {
                float max = ink.InkData != null ? ink.InkData.maxAmount : 0f;
                return $"[잉크] {ink.ItemName} ({(int)ink.currentAmount}/{(int)max})";
            }
            return item.ItemName;
        }
    }
}
