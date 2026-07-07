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
        public float inkDrainRate = 10f; // 잉크 소모 속도 (초당)

        // 글로벌 인벤토리 시스템 참조 (부모 클래스 사용)
        
        // 상태 관리
        private bool isScrollUIVisible = false;
        
        // 화면 우측 하단의 스크롤 캔버스 영역
        public Rect scrollCanvasRect; // OnGUI용 현재 스크롤 영역

        private Item_Scroll currentlyArmedScroll = null;

        protected override void Start()
        {
            base.Start();
            if (player == null) player = FindObjectOfType<PlayerController>();

            FindNextScroll(1);
            FindNextInk(1);
        }

        protected override void Update()
        {
            // 1. 발사 처리 (스크롤 모드가 아니고, 장전 상태일 때 좌클릭)
            if (!isScrollUIVisible && player.isArmed && Input.GetMouseButtonDown(0))
            {
                if (currentMana >= manaCostPerSpell)
                {
                    currentMana -= manaCostPerSpell;
                    Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    mouseWorldPos.z = 0;
                    player.CastArmedSpell(mouseWorldPos);

                    // 마법 발사 성공 시 스크롤 순환(사이클) 처리
                    if (currentlyArmedScroll != null)
                    {
                        currentlyArmedScroll.currentDurability--;
                        if (currentlyArmedScroll.currentDurability <= 0)
                        {
                            Debug.LogWarning("[전투] 스크롤이 수명을 다해 바스라졌습니다!");
                            int idx = inventory.IndexOf(currentlyArmedScroll);
                            inventory.Remove(currentlyArmedScroll);
                            
                            if (selectedInkIndex > idx) selectedInkIndex--;
                            if (selectedScrollIndex == idx)
                            {
                                selectedScrollIndex = -1;
                                FindNextScroll(1);
                            }
                            else if (selectedScrollIndex > idx) selectedScrollIndex--;
                        }
                        else
                        {
                            currentlyArmedScroll.isEmpty = true;
                            currentlyArmedScroll.spellName = "";
                            currentlyArmedScroll.itemName = "빈 스크롤";
                            Debug.Log($"[전투] 마법 사용 완료! 종이가 하얗게 표백되어 빈 스크롤이 되었습니다. (남은 횟수: {currentlyArmedScroll.currentDurability})");
                        }
                        currentlyArmedScroll = null;
                    }
                }
                else
                {
                    Debug.Log("<color=red>[전투] 마나가 부족하여 발사할 수 없습니다!</color>");
                }
                return;
            }

            // 2. 잉크 순차적 전환 (Shift 키)
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                FindNextInk(1);
            }

            // 3. 스크롤 모드 토글 (Space)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (player.isArmed)
                {
                    Debug.Log("[전투] 장전된 마법을 취소합니다. (스크롤 유지)");
                    player.isArmed = false;
                    player.armedSpellName = "";
                    currentlyArmedScroll = null;
                }

                isScrollUIVisible = !isScrollUIVisible;
                if (isScrollUIVisible)
                {
                    FindNextScroll(1); // 스크롤을 펼칠 때 유효한 스크롤이 있는지 재검사
                    
                    if (selectedScrollIndex == -1)
                    {
                        Debug.LogWarning("[전투] 인벤토리에 스크롤이 하나도 없습니다!");
                        isScrollUIVisible = false;
                    }
                    else
                    {
                        Debug.Log($"[전투] 스크롤 펼침!");
                    }
                }
                else
                {
                    ClearDrawing();
                }
            }

            // 4. 스크롤 모드가 켜져 있을 때의 로직
            if (isScrollUIVisible && selectedScrollIndex != -1)
            {
                // 마우스 휠로 스크롤 전환 (스크롤 아이템만 순회)
                float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
                if (scrollWheel != 0f)
                {
                    FindNextScroll(scrollWheel > 0f ? -1 : 1);
                    ClearDrawing();
                }

                // 스크롤 캔버스 영역 계산
                float canvasWidth = Screen.width * 0.4f;
                float canvasHeight = Screen.height * 0.6f;
                scrollCanvasRect = new Rect(Screen.width - canvasWidth - 20, Screen.height - canvasHeight - 20, canvasWidth, canvasHeight);
                
                Item_Scroll currentScroll = inventory[selectedScrollIndex] as Item_Scroll;

                // 빈 스크롤일 때만 그리기 허용
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

                // 5. 장전 처리 (우클릭)
                if (Input.GetMouseButtonDown(1))
                {
                    ArmAndCloseScroll();
                }
            }
        }

        // ConsumeInkBottle, UpdateStroke, StartStroke are now handled by base DrawingManager

        private void ArmAndCloseScroll()
        {
            if (selectedScrollIndex == -1) return;
            Item_Scroll currentScroll = inventory[selectedScrollIndex] as Item_Scroll;

            if (currentScroll.isEmpty)
            {
                if (drawnShapes.Count == 0)
                {
                    isScrollUIVisible = false;
                    return;
                }

                // 부모 클래스의 매칭 로직 호출 -> OnSpellMatched() 로 연결됨
                MatchCombo();
            }
            else
            {
                player.ArmSpell(currentScroll.spellName);
                
                currentlyArmedScroll = currentScroll;
                Debug.Log($"[전투] 완성된 스크롤 '{currentScroll.spellName}'을(를) 장전했습니다. (발사 시 빈 스크롤로 변환됩니다)");
                
                isScrollUIVisible = false;
                ClearDrawing();
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

            // 성공 시 현재 선택된 스크롤을 마법 스크롤로 변환
            Item_Scroll currentScroll = inventory[selectedScrollIndex] as Item_Scroll;
            if (currentScroll != null && currentScroll.isEmpty)
            {
                // 빈 스크롤 종이 자체를 마법이 새겨진 스크롤로 변환 (아직 종이를 쓴 건 아니므로 내구도는 유지)
                currentScroll.isEmpty = false;
                currentScroll.spellName = matchedSpell;
                currentScroll.accuracyScore = averageScore;
                
                string rank = averageScore >= 0.85f ? "[대성공]" : "[성공]";
                currentScroll.itemName = $"스크롤 {rank} [{matchedSpell}]";

                Debug.Log($"<color=cyan>[전투] 마법 장전 완료! 우클릭하여 발사하세요. {rank} (Score: {averageScore:F2})</color>");
                
                player.ArmSpell(matchedSpell);
                currentlyArmedScroll = currentScroll;
            }

            isScrollUIVisible = false;
            ClearDrawing();
        }

        private void ConsumeScrollDurability(Item_Scroll item)
        {
            item.currentDurability--;
            if (item.currentDurability <= 0)
            {
                Debug.LogWarning("[전투] 빈 스크롤이 파괴되었습니다!");
                int idxToRemove = inventory.IndexOf(item);
                inventory.Remove(item);
                
                // 인덱스 재조정
                if (selectedInkIndex > idxToRemove) selectedInkIndex--;
                
                selectedScrollIndex = -1;
                FindNextScroll(1);
            }
        }

        void OnGUI()
        {
            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 20;
            
            // 상단 자원 바
            GUI.Label(new Rect(20, 20, 300, 30), $"Mana: {currentMana:F0} / {maxMana}", style);

            // 조작법 힌트
            GUI.Label(new Rect(20, 60, 400, 100), 
                "[Space] 스크롤 열기/닫기\n" +
                "[Shift] 다음 잉크 장착\n" +
                "[스크롤 모드] 마우스 휠: 인벤토리의 스크롤만 전환\n" +
                "[일반 모드] 좌클릭: 장전된 마법 발사 (마나소모)");

            // 인벤토리 전체 리스트 출력 (우측 상단)
            GUI.Label(new Rect(Screen.width - 300, 20, 200, 30), $"--- Backpack ---", style);
            for (int i = 0; i < inventory.Count; i++)
            {
                ItemData item = inventory[i];
                string prefix = "  ";
                string colorTag = "<color=white>";
                string details = "";

                if (item is Item_Scroll scroll)
                {
                    if (i == selectedScrollIndex && isScrollUIVisible) 
                    {
                        prefix = "▶ ";
                        colorTag = "<color=yellow>";
                    }
                    details = scroll.isEmpty ? $"(내구도:{scroll.currentDurability})" : "";
                }
                else if (item is Item_Ink ink)
                {
                    if (i == selectedInkIndex)
                    {
                        prefix = "▶ ";
                        colorTag = "<color=cyan>";
                    }
                    details = $"({ink.currentAmount:F0}/{ink.maxAmount:F0}ml)";
                }

                GUI.Label(new Rect(Screen.width - 300, 50 + (i * 30), 300, 30), $"{colorTag}{prefix}[Slot {i+1}] {item.itemName} {details}</color>", style);
            }

            // 스크롤 캔버스 그리기
            if (isScrollUIVisible && selectedScrollIndex != -1)
            {
                Item_Scroll currentScroll = inventory[selectedScrollIndex] as Item_Scroll;

                // 캔버스 배경
                GUI.color = currentScroll.isEmpty ? new Color(1f, 0.9f, 0.7f, 0.8f) : new Color(0.7f, 0.9f, 1f, 0.8f);
                GUI.DrawTexture(scrollCanvasRect, Texture2D.whiteTexture);
                
                GUI.color = Color.black;
                if (currentScroll.isEmpty)
                {
                    GUI.Label(new Rect(scrollCanvasRect.x + 10, scrollCanvasRect.y + 10, 300, 30), $"[빈 스크롤] 좌클릭: 그리기, 우클릭: 장전");
                }
                else
                {
                    GUIStyle hugeStyle = new GUIStyle(GUI.skin.label);
                    hugeStyle.fontSize = 40;
                    hugeStyle.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(new Rect(scrollCanvasRect.x, scrollCanvasRect.y, scrollCanvasRect.width, scrollCanvasRect.height), 
                        $"{currentScroll.spellName}\n<size=20>우클릭으로 즉시 장전!</size>", hugeStyle);
                }
            }
        }
    }
}
