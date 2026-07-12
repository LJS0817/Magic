using UnityEngine;

namespace Magic.Inventory
{
    public class CurrencyDebugUI : MonoBehaviour
    {
        private Rect windowRect = new Rect(330, 20, 320, 420);
        private bool showWindow = true;
        
        // 소비 테스트 시 자동 변환 기능 토글 여부
        private bool autoConvertToggle = true;

        // UI에 입력할 커스텀 추가/소비 수량
        private string customAmountStr = "10";

        void OnGUI()
        {
            if (!showWindow)
            {
                if (GUI.Button(new Rect(330, 20, 100, 30), "재화 UI 열기"))
                {
                    showWindow = true;
                }
                return;
            }

            windowRect = GUI.Window(1, windowRect, DrawCurrencyWindow, "재화 시스템 (디버그)");
        }

        void DrawCurrencyWindow(int windowID)
        {
            if (CurrencyManager.Instance == null)
            {
                GUILayout.Label("CurrencyManager 인스턴스가 없습니다.\n먼저 씬에 CurrencyManager를 부착하세요.");
                if (GUILayout.Button("닫기")) showWindow = false;
                GUI.DragWindow();
                return;
            }

            var cm = CurrencyManager.Instance;

            GUILayout.Label("<b>[ 현재 보유 재화 ]</b>", GUILayout.Height(20));
            
            // 각 재화별 보유량 표시 (다양한 색상으로 가독성 극대화)
            DrawCurrencyRow("동화 (Copper)", cm.Copper, "#b87333");
            DrawCurrencyRow("은화 (Silver)", cm.Silver, "#c0c0c0");
            DrawCurrencyRow("금화 (Gold)", cm.Gold, "#ffd700");
            DrawCurrencyRow("백금화 (Platinum)", cm.Platinum, "#e5e4e2");
            DrawCurrencyRow("보석 (Gem)", cm.Gem, "#00ffff");

            GUILayout.Space(10);
            
            // 총 Copper 가치 표시 (보석 제외)
            long totalCopper = cm.GetTotalNormalCurrencyAsCopper();
            GUILayout.Label($"<b>총 가치 (동화 환산):</b> {totalCopper:N0} Copper");

            GUILayout.Space(15);
            GUILayout.Label("<b>[ 재화 지급 / 소비 테스트 ]</b>");

            // 수량 입력 필드
            GUILayout.BeginHorizontal();
            GUILayout.Label("수량: ", GUILayout.Width(40));
            customAmountStr = GUILayout.TextField(customAmountStr, GUILayout.Width(80));
            
            // 숫자만 입력 가능하도록 예외 처리
            if (!int.TryParse(customAmountStr, out int amount))
            {
                amount = 0;
            }
            GUILayout.EndHorizontal();

            // 소비 시 자동 변환 여부 토글
            autoConvertToggle = GUILayout.Toggle(autoConvertToggle, "소비 시 자동 화폐 변환 (상위 화폐 깨기 등)");

            GUILayout.Space(10);

            // 각 재화 조작 버튼 배치
            DrawControlButtons(cm, CurrencyType.Copper, amount);
            DrawControlButtons(cm, CurrencyType.Silver, amount);
            DrawControlButtons(cm, CurrencyType.Gold, amount);
            DrawControlButtons(cm, CurrencyType.Platinum, amount);
            DrawControlButtons(cm, CurrencyType.Gem, amount);

            GUILayout.Space(15);

            // 지갑 압축/정리 버튼
            if (GUILayout.Button("지갑 정리 (하위 화폐 자동 승격)", GUILayout.Height(25)))
            {
                cm.CompressCurrency();
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("닫기"))
            {
                showWindow = false;
            }

            GUI.DragWindow();
        }

        private void DrawCurrencyRow(string label, int amount, string hexColor)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<color={hexColor}>■</color> {label}:", GUILayout.Width(130));
            GUILayout.Label($"<b>{amount:N0}</b>");
            GUILayout.EndHorizontal();
        }

        private void DrawControlButtons(CurrencyManager cm, CurrencyType type, int amount)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(type.ToString(), GUILayout.Width(70));
            
            if (GUILayout.Button($"+{amount}", GUILayout.Width(60)))
            {
                cm.AddCurrency(type, amount);
            }
            
            if (GUILayout.Button($"-{amount}", GUILayout.Width(60)))
            {
                bool success = cm.SpendCurrency(type, amount, autoConvertToggle);
                if (!success)
                {
                    Debug.LogWarning($"[CurrencyDebugUI] {type} {amount} 소비 실패! 잔액이 부족합니다.");
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
