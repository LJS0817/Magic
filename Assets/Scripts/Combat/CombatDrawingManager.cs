using UnityEngine;
using System.Collections.Generic;
using Magic.Drawing;
using Magic.Inventory;
using DG.Tweening;

namespace Magic.Combat
{
    [System.Serializable]
    public struct ChantRecipe
    {
        public SpellElement element;
        public List<KeyCode> combo;
    }

    public class CombatDrawingManager : DrawingManager
    {
        [Header("Combat References")]
        public PlayerController player;

        [Header("Resources")]
        public float maxMana = 100f;
        public float currentMana = 100f;
        public float manaCostPerSpell = 30f;

        [Header("Chanting System")]
        public List<ChantRecipe> chantRecipes = new List<ChantRecipe>();
        public List<KeyCode> currentChantInput = new List<KeyCode>();
        public SpellElement currentElement = SpellElement.None;
        private KeyCode[] chantKeys = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F };

        [Header("Chanting Phase")]
        public float chantTimeLimit = 1.5f;
        public float currentChantTimer = 0f;
        public bool isChantingPhase = false;
        private string cachedSpellName = "";
        private float cachedSpellScore = 0f;
        private SpellType cachedSpellType = SpellType.Attack;
        private Item_Scroll cachedScroll = null;

        [Header("UI Animations")]
        private float chantUIScale = 1f;
        private float parryUIAlpha = 1f;
        private bool wasParryOpen = false;
        private Tween parryTween;

        // 화면 중앙 그리기 캔버스 영역
        public Rect scrollCanvasRect; 

        protected override void Start()
        {
            base.Start();
            if (player == null) player = FindObjectOfType<PlayerController>();

            if (chantRecipes.Count == 0)
            {
                // 기본 레시피 등록 (4원소 4키 조합)
                chantRecipes.Add(new ChantRecipe { element = SpellElement.Fire, combo = new List<KeyCode> { KeyCode.W, KeyCode.R, KeyCode.A, KeyCode.Q } });
                chantRecipes.Add(new ChantRecipe { element = SpellElement.Ice, combo = new List<KeyCode> { KeyCode.Q, KeyCode.E, KeyCode.W, KeyCode.S } });
                chantRecipes.Add(new ChantRecipe { element = SpellElement.Lightning, combo = new List<KeyCode> { KeyCode.E, KeyCode.D, KeyCode.R, KeyCode.F } });
                chantRecipes.Add(new ChantRecipe { element = SpellElement.Earth, combo = new List<KeyCode> { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.W } });
            }

            // 기본 장비 설정: 로드아웃의 첫 번째 스크롤/잉크로 장착
            if (InventoryManager.Instance != null)
            {
                if (InventoryManager.Instance.EquippedScroll == null)
                    InventoryManager.Instance.CycleScroll(1);
                if (InventoryManager.Instance.EquippedInk == null)
                    InventoryManager.Instance.CycleInk(1);
            }
        }

        protected override void Update()
        {
            if (CombatManager.Instance == null) return;

            bool canAct = CombatManager.Instance.CurrentState == CombatState.PlayerTurn || CombatManager.Instance.IsParryWindowOpen;

            // 패링 윈도우 UI 애니메이션 트리거 (DOTween)
            if (CombatManager.Instance.IsParryWindowOpen && !wasParryOpen)
            {
                wasParryOpen = true;
                parryUIAlpha = 0f;
                parryTween = DOTween.To(() => parryUIAlpha, x => parryUIAlpha = x, 1f, 0.2f).SetLoops(-1, LoopType.Yoyo);
            }
            else if (!CombatManager.Instance.IsParryWindowOpen && wasParryOpen)
            {
                wasParryOpen = false;
                parryTween?.Kill();
                parryUIAlpha = 1f;
            }

            // 잉크/스크롤 전환 (숫자키 등 단축키를 쓰거나 UI 클릭)
            // 임시로 마우스 휠로 스크롤 전환
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if (scrollWheel != 0f)
            {
                if (InventoryManager.Instance != null)
                    InventoryManager.Instance.CycleScroll(scrollWheel > 0f ? -1 : 1);
                ClearDrawing();
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                if (InventoryManager.Instance != null)
                    InventoryManager.Instance.CycleInk(1);
            }

            // 캔버스 영역 계산 (가로 전체, 세로 절반, 중앙 배치)
            float canvasWidth = Screen.width;
            float canvasHeight = Screen.height * 0.5f;
            scrollCanvasRect = new Rect(0, (Screen.height - canvasHeight) / 2f, canvasWidth, canvasHeight);

            bool isChanting = CombatManager.Instance.CurrentState == CombatState.PlayerChanting;

            if (isChanting)
            {
                currentChantTimer -= Time.deltaTime;
                HandleChantInput();

                if (currentChantTimer <= 0)
                {
                    Debug.Log("<color=red>[영창] 영창 시간이 초과되었습니다!</color>");
                    ExecuteSpell(); // 시간 초과 시 현재까지의 속성(또는 None)으로 마법 발동
                }
                return; // 영창 중에는 다른 액션(그리기 등) 불가
            }

            Item_Scroll currentScroll = InventoryManager.Instance != null ? InventoryManager.Instance.EquippedScroll : null;

            if (canAct && currentScroll != null)
            {
                if (currentScroll.isEmpty)
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
                    if (currentScroll.isEmpty)
                    {
                        MatchCombo();
                    }
                    else if (currentScroll.ScrollData != null)
                    {
                        // 완성된 스크롤 즉시 발사
                        OnSpellMatched(currentScroll.ScrollData.spellName, currentScroll.ScrollData.accuracyScore);
                        // 완성된 스크롤 발사 후 다시 빈 스크롤 상태로 되돌림 (임시)
                        currentScroll.isEmpty = true;
                        currentScroll.ScrollData.spellName = "";
                    }
                }
            }
            else
            {
                if (isDrawing) EndStroke();

                // 자신의 턴(마법진 사용 가능 시간)이 아닐 때는 미리 모아둔 영창을 모두 초기화
                if (currentElement != SpellElement.None || currentChantInput.Count > 0)
                {
                    currentElement = SpellElement.None;
                    currentChantInput.Clear();
                }
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

            SpellRecipeAsset matchedAsset = null;
            if (drawingDatabase != null && drawingDatabase.recipes != null)
            {
                foreach (var r in drawingDatabase.recipes)
                {
                    if (r.SpellName == matchedSpell)
                    {
                        matchedAsset = r;
                        break;
                    }
                }
            }
            SpellType sType = matchedAsset != null ? matchedAsset.Type : SpellType.Attack;

            Item_Scroll currentScroll = InventoryManager.Instance != null ? InventoryManager.Instance.EquippedScroll : null;
            if (currentScroll != null && currentScroll.isEmpty)
            {
                cachedSpellName = matchedSpell;
                cachedSpellScore = averageScore;
                cachedSpellType = sType;
                cachedScroll = currentScroll;
                
                // 기존에 잘못 입력된 영창 초기화
                currentChantInput.Clear();
                currentElement = SpellElement.None;

                if (sType == SpellType.Defense)
                {
                    // 방어 마법은 영창(속성 부여) 없이 즉시 발사
                    ExecuteSpell();
                }
                else
                {
                    // 공격 마법 등은 영창 페이즈로 돌입
                    isChantingPhase = true;
                    currentChantTimer = chantTimeLimit;
                    CombatManager.Instance.StartPlayerChanting();

                    // 영창 UI 애니메이션 (통통 튀는 스케일)
                    chantUIScale = 0f;
                    DOTween.To(() => chantUIScale, x => chantUIScale = x, 1f, 0.5f).SetEase(Ease.OutBack);
                }
            }
            else if (currentScroll != null && !currentScroll.isEmpty)
            {
                // 이미 속성과 마법이 완성된 스크롤은 영창 없이 즉시 발사
                cachedSpellName = matchedSpell;
                cachedSpellScore = averageScore;
                cachedScroll = currentScroll;
                ExecuteSpell();
            }
        }

        private void ExecuteSpell()
        {
            isChantingPhase = false;
            string rank = cachedSpellScore >= 0.85f ? "[대성공]" : "[성공]";
            string elementText = currentElement == SpellElement.None ? "" : $" [{currentElement}]";
            Debug.Log($"<color=cyan>[전투] 마법 발동! {rank}{elementText} [{cachedSpellName}] (Score: {cachedSpellScore:F2})</color>");
            
            // 마나 체크 (실제로는 마나 소모가 필요)
            if (currentMana >= manaCostPerSpell)
            {
                currentMana -= manaCostPerSpell;

                // 상태에 따른 처리
                if (CombatManager.Instance.IsParryWindowOpen)
                {
                    CombatManager.Instance.EvaluateParryClash(cachedSpellName, cachedSpellType, currentElement);
                }
                else
                {
                    if (cachedSpellType == SpellType.Defense)
                    {
                        Debug.Log("<color=cyan>[전투] 방어 마법 전개! (자신에게 방어막 부여)</color>");
                        // TODO: 실제 버프/쉴드 로직 구현 필요
                    }
                    else
                    {
                        CombatManager.Instance.enemy.TakeDamage(25f, currentElement);
                    }
                    CombatManager.Instance.EndPlayerTurn();
                }

                ConsumeScrollDurability(cachedScroll);
            }
            else
            {
                Debug.Log("<color=red>[전투] 마나가 부족합니다!</color>");
            }

            // 속성 영창 초기화
            currentElement = SpellElement.None;
            currentChantInput.Clear();

            ClearDrawing();
        }

        private void HandleChantInput()
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode key in chantKeys)
                {
                    if (Input.GetKeyDown(key))
                    {
                        currentChantInput.Add(key);
                        CheckChantRecipe();
                        
                        // 영창 길이가 너무 길어지면 초기화 (예: 4개 키 초과)
                        if (currentChantInput.Count > 4)
                        {
                            currentChantInput.Clear();
                        }
                        break;
                    }
                }
            }
        }

        private void CheckChantRecipe()
        {
            foreach (var recipe in chantRecipes)
            {
                if (recipe.combo.Count == currentChantInput.Count)
                {
                    bool isMatch = true;
                    for (int i = 0; i < recipe.combo.Count; i++)
                    {
                        if (recipe.combo[i] != currentChantInput[i])
                        {
                            isMatch = false;
                            break;
                        }
                    }

                    if (isMatch)
                    {
                        currentElement = recipe.element;
                        Debug.Log($"<color=orange>[영창 완성] {currentElement} 속성이 부여되었습니다!</color>");
                        currentChantInput.Clear();
                        
                        // 영창이 완성되면 즉시 마법 발동
                        ExecuteSpell();
                        return;
                    }
                }
            }
        }

        private void ConsumeScrollDurability(Item_Scroll item)
        {
            item.currentDurability--;
            if (item.currentDurability <= 0)
            {
                Debug.LogWarning("[전투] 스크롤이 파괴되었습니다!");
                InventoryManager.Instance.RemoveItem(item); // 인벤토리 + 로드아웃에서 제거 및 자동 장착
            }
        }

        protected override bool CanDrawWithPen(Item_Pen pen)
        {
            if (pen == null) return false;
            if (pen.PenData != null && pen.PenData.consumesMana)
            {
                return currentMana > 0f;
            }
            return pen.currentInkCapacity > 0f;
        }

        protected override void ConsumePenResource(Item_Pen pen)
        {
            if (pen == null) return;
            float rate = pen.PenData != null ? pen.PenData.inkConsumptionRate : 0f;
            if (pen.PenData != null && pen.PenData.consumesMana)
            {
                currentMana -= rate * Time.deltaTime;
                if (currentMana < 0) currentMana = 0;
            }
            else
            {
                pen.currentInkCapacity -= rate * Time.deltaTime;
                if (pen.currentInkCapacity < 0) pen.currentInkCapacity = 0;
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
            {
                Color oldColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, parryUIAlpha);
                GUI.Label(new Rect(20, 80, 400, 30), $"<color=yellow>Parry Window: {eTime:F1}s!</color>", style);
                GUI.color = oldColor;
            }

            if (isChantingPhase)
            {
                Vector2 center = new Vector2(20 + 200, 110 + 15);
                GUIUtility.ScaleAroundPivot(new Vector2(chantUIScale, chantUIScale), center);
                GUI.Label(new Rect(20, 110, 400, 30), $"<color=magenta>CHANTING PHASE: {currentChantTimer:F1}s</color>", style);
                GUIUtility.ScaleAroundPivot(new Vector2(1f / chantUIScale, 1f / chantUIScale), center); // Reset scale
            }

            // 2. 중앙 그리기 캔버스 (화면 세로의 1/2)
            GUI.color = new Color(0.8f, 0.9f, 1f, 0.3f);
            GUI.DrawTexture(scrollCanvasRect, Texture2D.whiteTexture);
            GUI.color = Color.black;
            GUI.Label(new Rect(scrollCanvasRect.x + 10, scrollCanvasRect.y + 10, 300, 30), "[그리기 영역] 스페이스바로 마법 발동");
            GUI.color = Color.white;

            // 3. 좌측: 잉크 (세로 리스트)
            Item_Ink equippedInk = InventoryManager.Instance != null ? InventoryManager.Instance.EquippedInk : null;
            var loadout = InventoryManager.Instance != null ? InventoryManager.Instance.combatLoadout : new List<ItemInstance>();

            float leftX = 20f;
            float bottomYStart = Screen.height - 200f;
            GUI.Label(new Rect(leftX, bottomYStart, 200, 30), "--- Ink ---", style);
            int inkCount = 0;
            for (int i = 0; i < loadout.Count; i++)
            {
                if (loadout[i] is Item_Ink ink)
                {
                    string prefix = ink == equippedInk ? "<color=cyan>▶ </color>" : "  ";
                    GUI.Label(new Rect(leftX, bottomYStart + 30 + (inkCount * 30), 200, 30), $"{prefix}{ink.ItemName}", style);
                    inkCount++;
                }
            }

            // 4. 하단 중앙: 활성화된 스크롤 및 영창 상태
            Item_Scroll activeScroll = InventoryManager.Instance != null ? InventoryManager.Instance.EquippedScroll : null;
            if (activeScroll != null)
            {
                GUIStyle centerStyle = new GUIStyle(GUI.skin.label);
                centerStyle.fontSize = 24;
                centerStyle.alignment = TextAnchor.MiddleCenter;
                
                Rect bottomCenter = new Rect(0, Screen.height - 80, Screen.width, 70);
                GUI.color = new Color(0, 0, 0, 0.6f);
                GUI.DrawTexture(bottomCenter, Texture2D.whiteTexture);
                GUI.color = Color.white;
                
                string spell = activeScroll.ScrollData != null ? activeScroll.ScrollData.spellName : "Unknown";
                string scrollName = activeScroll.isEmpty ? activeScroll.ItemName : spell;
                string chantStr = "";
                foreach (var k in currentChantInput) chantStr += $"[{k}] ";
                
                string elementStr = currentElement == SpellElement.None ? "" : $"<color=orange> (속성: {currentElement})</color>";

                if (isChantingPhase)
                {
                    Vector2 center = new Vector2(Screen.width / 2f, Screen.height - 60f);
                    GUIUtility.ScaleAroundPivot(new Vector2(chantUIScale, chantUIScale), center);
                    GUI.Label(new Rect(0, Screen.height - 75, Screen.width, 30), $"<color=magenta>영창을 입력하세요!</color> {chantStr}{elementStr}", centerStyle);
                    GUIUtility.ScaleAroundPivot(new Vector2(1f / chantUIScale, 1f / chantUIScale), center); // Reset
                }
                else
                {
                    GUI.Label(new Rect(0, Screen.height - 75, Screen.width, 30), $"영창: {chantStr}{elementStr}", centerStyle);
                }
                
                GUI.Label(new Rect(0, Screen.height - 40, Screen.width, 30), $"[Active Scroll] {scrollName} (내구도: {activeScroll.currentDurability})", centerStyle);
            }

            // 5. 우측: 스크롤 인벤토리 (그리드 형태)
            Item_Scroll equippedScroll = InventoryManager.Instance != null ? InventoryManager.Instance.EquippedScroll : null;
            float rightX = Screen.width - 250f;
            GUI.Label(new Rect(rightX, bottomYStart, 200, 30), "--- Scrolls ---", style);
            int scrollCount = 0;
            for (int i = 0; i < loadout.Count; i++)
            {
                if (loadout[i] is Item_Scroll scroll)
                {
                    string prefix = scroll == equippedScroll ? "<color=yellow>▶ </color>" : "  ";
                    string spell = scroll.ScrollData != null ? scroll.ScrollData.spellName : "Unknown";
                    string scrollName = scroll.isEmpty ? scroll.ItemName : spell;
                    GUI.Label(new Rect(rightX, bottomYStart + 30 + (scrollCount * 30), 200, 30), $"{prefix}{scrollName}", style);
                    scrollCount++;
                }
            }
        }
    }
}
