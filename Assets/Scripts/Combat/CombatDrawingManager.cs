using UnityEngine;
using System.Collections.Generic;
using Magic.Drawing;
using Magic.Inventory;

namespace Magic.Combat
{
    public class CombatDrawingManager : DrawingManager
    {
        [Header("Combat References")]
        public PlayerController player;

        [Header("Resources")]
        public float maxMana = 100f;
        public float currentMana = 100f;
        public float manaCostPerSpell = 30f;

        // 오버라이드: CombatDrawingManager는 combatLoadout만 사용
        public override List<ItemData> inventory => InventoryManager.Instance != null ? InventoryManager.Instance.combatLoadout : new List<ItemData>();
        
        // 화면 중앙 그리기 캔버스 영역
        public Rect scrollCanvasRect; 

        protected override void Start()
        {
            base.Start();
            if (player == null) player = FindObjectOfType<PlayerController>();

            FindNextScroll(1);
            FindNextInk(1);
        }

        protected override void Update()
        {
            if (CombatManager.Instance == null) return;

            bool canAct = CombatManager.Instance.CurrentState == CombatState.PlayerTurn || CombatManager.Instance.IsParryWindowOpen;

            // 잉크/스크롤 전환 (숫자키 등 단축키를 쓰거나 UI 클릭)
            // 임시로 마우스 휠로 스크롤 전환
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if (scrollWheel != 0f)
            {
                FindNextScroll(scrollWheel > 0f ? -1 : 1);
                ClearDrawing();
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                FindNextInk(1);
            }

            // 캔버스 영역 계산 (가로 전체, 세로 절반, 중앙 배치)
            float canvasWidth = Screen.width;
            float canvasHeight = Screen.height * 0.5f;
            scrollCanvasRect = new Rect(0, (Screen.height - canvasHeight) / 2f, canvasWidth, canvasHeight);

            if (canAct && selectedScrollIndex != -1)
            {
                Item_Scroll currentScroll = inventory[selectedScrollIndex] as Item_Scroll;

                if (currentScroll != null && currentScroll.isEmpty)
                {
                    Vector2 mousePosGUI = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                    if (scrollCanvasRect.Contains(mousePosGUI))
                    {
                        if (Input.GetMouseButtonDown(0)) StartStroke();
                        else if (Input.GetMouseButton(0)) UpdateStroke();
                        else if (Input.GetMouseButtonUp(0)) EndStroke();
                    }
                    else if (Input.GetMouseButtonUp(0) && isDrawing)
                    {
                        EndStroke();
                    }
                }

                // 마법 조합 (빈 스크롤) 또는 완성된 스크롤 즉시 발동
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (currentScroll != null && currentScroll.isEmpty)
                    {
                        MatchCombo();
                    }
                    else if (currentScroll != null && !currentScroll.isEmpty)
                    {
                        // 완성된 스크롤 즉시 발사
                        OnSpellMatched(currentScroll.spellName, currentScroll.accuracyScore);
                        // 완성된 스크롤 발사 후 다시 빈 스크롤로 되돌림
                        currentScroll.isEmpty = true;
                        currentScroll.spellName = "";
                        currentScroll.itemName = "빈 스크롤";
                    }
                }
            }
            else
            {
                if (isDrawing) EndStroke();
            }
        }

        protected override void OnSpellMatched(string matchedSpell, float averageScore)
        {
            if (matchedSpell.Contains("실패") || matchedSpell.Contains("아무것도"))
            {
                Debug.Log($"<color=red>[전투] 마법진 발동 실패: {matchedSpell} (Score: {averageScore:F2})</color>");
                ClearDrawing();
                return;
            }

            Item_Scroll currentScroll = inventory[selectedScrollIndex] as Item_Scroll;
            if (currentScroll != null && currentScroll.isEmpty)
            {
                string rank = averageScore >= 0.85f ? "[대성공]" : "[성공]";
                Debug.Log($"<color=cyan>[전투] 마법 발동! {rank} [{matchedSpell}] (Score: {averageScore:F2})</color>");
                
                // 마나 체크 (실제로는 마나 소모가 필요)
                if (currentMana >= manaCostPerSpell)
                {
                    currentMana -= manaCostPerSpell;

                    // 상태에 따른 처리
                    if (CombatManager.Instance.IsParryWindowOpen)
                    {
                        // 패링 타이밍에 성공적으로 그렸을 때 (방어 마법인지 추가 체크 가능)
                        CombatManager.Instance.SuccessfulParry();
                    }
                    else if (CombatManager.Instance.CurrentState == CombatState.PlayerTurn)
                    {
                        // 플레이어 턴에 공격
                        CombatManager.Instance.enemy.TakeDamage(25f); // 임시 데미지
                        CombatManager.Instance.EndPlayerTurn();
                    }

                    // 스크롤 내구도 소모
                    ConsumeScrollDurability(currentScroll);
                }
                else
                {
                    Debug.Log("<color=red>[전투] 마나가 부족합니다!</color>");
                }
            }

            ClearDrawing();
        }

        private void ConsumeScrollDurability(Item_Scroll item)
        {
            item.currentDurability--;
            if (item.currentDurability <= 0)
            {
                Debug.LogWarning("[전투] 스크롤이 파괴되었습니다!");
                int idxToRemove = inventory.IndexOf(item);
                InventoryManager.Instance.RemoveFromLoadout(item); // Loadout에서 제거
                
                if (selectedInkIndex > idxToRemove) selectedInkIndex--;
                
                selectedScrollIndex = -1;
                FindNextScroll(1);
            }
        }

        protected override void OnGUI()
        {
            if (CombatManager.Instance == null) return;

            // 부모의 판별 표시 시스템(OnGUI) 상속 호출
            base.OnGUI();

            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 20;
            style.alignment = TextAnchor.MiddleLeft;

            // 1. 상단 정보 (턴, 게이지, 시간)
            string turnInfo = CombatManager.Instance.CurrentState.ToString();
            float pATB = CombatManager.Instance.currentPlayerATB;
            float eATB = CombatManager.Instance.currentEnemyATB;
            float pTime = CombatManager.Instance.currentPlayerTurnTimer;
            float eTime = CombatManager.Instance.currentParryTimer;

            GUI.Label(new Rect(20, 20, 400, 30), $"State: {turnInfo}", style);
            GUI.Label(new Rect(20, 50, 400, 30), $"Player ATB: {pATB:F0} | Enemy ATB: {eATB:F0}", style);
            
            if (CombatManager.Instance.CurrentState == CombatState.PlayerTurn)
                GUI.Label(new Rect(20, 80, 400, 30), $"<color=green>Time Left: {pTime:F1}s</color>", style);
            if (CombatManager.Instance.IsParryWindowOpen)
                GUI.Label(new Rect(20, 80, 400, 30), $"<color=yellow>Parry Window: {eTime:F1}s!</color>", style);

            // 2. 중앙 그리기 캔버스 (화면 세로의 1/2)
            GUI.color = new Color(0.8f, 0.9f, 1f, 0.3f);
            GUI.DrawTexture(scrollCanvasRect, Texture2D.whiteTexture);
            GUI.color = Color.black;
            GUI.Label(new Rect(scrollCanvasRect.x + 10, scrollCanvasRect.y + 10, 300, 30), "[그리기 영역] 스페이스바로 마법 발동");
            GUI.color = Color.white;

            // 3. 좌측: 잉크 (세로 리스트)
            float leftX = 20f;
            float bottomYStart = Screen.height - 200f;
            GUI.Label(new Rect(leftX, bottomYStart, 200, 30), "--- Ink ---", style);
            int inkCount = 0;
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] is Item_Ink ink)
                {
                    string prefix = i == selectedInkIndex ? "<color=cyan>▶ </color>" : "  ";
                    GUI.Label(new Rect(leftX, bottomYStart + 30 + (inkCount * 30), 200, 30), $"{prefix}{ink.itemName}", style);
                    inkCount++;
                }
            }

            // 4. 하단 중앙: 활성화된 스크롤
            if (selectedScrollIndex != -1 && inventory[selectedScrollIndex] is Item_Scroll activeScroll)
            {
                GUIStyle centerStyle = new GUIStyle(GUI.skin.label);
                centerStyle.fontSize = 24;
                centerStyle.alignment = TextAnchor.MiddleCenter;
                
                Rect bottomCenter = new Rect(0, Screen.height - 60, Screen.width, 50);
                GUI.color = new Color(0, 0, 0, 0.5f);
                GUI.DrawTexture(bottomCenter, Texture2D.whiteTexture);
                GUI.color = Color.white;
                
                string scrollName = activeScroll.isEmpty ? "빈 스크롤" : activeScroll.spellName;
                GUI.Label(bottomCenter, $"[Active Scroll] {scrollName} (내구도: {activeScroll.currentDurability})", centerStyle);
            }

            // 5. 우측: 스크롤 인벤토리 (그리드 형태)
            float rightX = Screen.width - 250f;
            GUI.Label(new Rect(rightX, bottomYStart, 200, 30), "--- Scrolls ---", style);
            int scrollCount = 0;
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] is Item_Scroll scroll)
                {
                    string prefix = i == selectedScrollIndex ? "<color=yellow>▶ </color>" : "  ";
                    string scrollName = scroll.isEmpty ? "빈 스크롤" : scroll.spellName;
                    GUI.Label(new Rect(rightX, bottomYStart + 30 + (scrollCount * 30), 200, 30), $"{prefix}{scrollName}", style);
                    scrollCount++;
                }
            }
        }
    }
}
