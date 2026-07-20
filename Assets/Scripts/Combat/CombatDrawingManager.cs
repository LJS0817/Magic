using UnityEngine;
using System.Collections.Generic;
using Magic.Drawing;
using Magic.Inventory;
using DG.Tweening;
using Magic.UI;
using Magic.Data;

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

            if (InventoryManager.Instance != null)
            {
                if (InventoryManager.Instance.EquippedScroll == null)
                    InventoryManager.Instance.CycleScroll(1);
                if (InventoryManager.Instance.EquippedInk == null)
                    InventoryManager.Instance.CycleInk(1);
            }
            
            if (manaSlider != null && PlayerDataManager.Instance != null)
                manaSlider.SetValue(PlayerDataManager.Instance.currentMana, PlayerDataManager.Instance.GetMaxMana());
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

            // 잉크/스크롤 장착 및 전환은 이제 LoadoutUI에서 더블클릭으로 처리합니다.

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
                    if (IsDrawingBlocked)
                    {
                        if (isDrawing) EndStroke();
                    }
                    else
                    {
                        Vector2 mousePosGUI = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                        if (scrollCanvasRect.Contains(mousePosGUI))
                        {
                            if (isOutsideArea)
                            {
                                isOutsideArea = false;
                                ReturnPenToMouse();
                            }

                            if (penController != null && !isRefilling)
                            {
                                penController.TrackMouse(mainCamera);
                            }

                            if (Input.GetMouseButtonDown(0)) StartStroke();
                            else if (Input.GetMouseButton(0)) UpdateStroke();
                            else if (Input.GetMouseButtonUp(0)) EndStroke();
                        }
                        else
                        {
                            if (!isOutsideArea)
                            {
                                isOutsideArea = true;
                                RefillPen();
                            }

                            if (Input.GetMouseButtonUp(0) && isDrawing)
                            {
                                EndStroke();
                            }
                        }
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

        public void CastSpellFromScroll(Item_Scroll scroll)
        {
            if (scroll == null || scroll.isEmpty || scroll.ScrollData == null) return;
            
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.EquippedScroll = scroll;
            }
            
            OnSpellMatched(scroll.ScrollData.spellName, scroll.ScrollData.accuracyScore);
            
            scroll.isEmpty = true;
            scroll.ScrollData.spellName = "";
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
            float spellCost = matchedAsset != null ? matchedAsset.manaCost : 30f;

            Item_Scroll currentScroll = InventoryManager.Instance != null ? InventoryManager.Instance.EquippedScroll : null;
            if (currentScroll != null && currentScroll.isEmpty)
            {
                float drawCost = spellCost * 0.5f;
                if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.currentMana < drawCost)
                {
                    Debug.Log($"<color=red>[전투] 마나가 부족하여 마법을 스크롤에 새길 수 없습니다! (필요 마나: {drawCost:F1})</color>");
                    ClearDrawing();
                    return;
                }

                PlayerDataManager.Instance.currentMana -= drawCost;
                if (manaSlider != null) manaSlider.SetValue(PlayerDataManager.Instance.currentMana, PlayerDataManager.Instance.GetMaxMana());

                currentScroll.isEmpty = false;
                if (currentScroll.ScrollData != null)
                {
                    currentScroll.ScrollData.spellName = matchedSpell;
                    currentScroll.ScrollData.accuracyScore = averageScore;
                    
                    Magic.Combat.SpellElement penElem = Magic.Combat.SpellElement.None;
                    Magic.Combat.SpellElement inkElem = Magic.Combat.SpellElement.None;
                    Magic.Combat.SpellElement baseScrollElem = currentScroll.ScrollData.scrollElement;

                    if (InventoryManager.Instance != null)
                    {
                        if (InventoryManager.Instance.EquippedPen != null && InventoryManager.Instance.EquippedPen.PenData != null)
                            penElem = InventoryManager.Instance.EquippedPen.PenData.penElement;
                        
                        if (InventoryManager.Instance.EquippedInk != null && InventoryManager.Instance.EquippedInk.InkData != null)
                            inkElem = InventoryManager.Instance.EquippedInk.InkData.inkElement;
                    }

                    currentScroll.ScrollData.scrollElement = DetermineFinalElement(penElem, inkElem, baseScrollElem);
                }
                
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.NotifyInventoryChanged();
                }

                if (PlayerDataManager.Instance != null)
                {
                    PlayerDataManager.Instance.UnlockRecipe(matchedSpell);
                }

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

        private Magic.Combat.SpellElement DetermineFinalElement(Magic.Combat.SpellElement e1, Magic.Combat.SpellElement e2, Magic.Combat.SpellElement e3)
        {
            int fireCount = 0, iceCount = 0, lightningCount = 0, earthCount = 0;
            
            void AddCount(Magic.Combat.SpellElement e) {
                if (e == Magic.Combat.SpellElement.Fire) fireCount++;
                else if (e == Magic.Combat.SpellElement.Ice) iceCount++;
                else if (e == Magic.Combat.SpellElement.Lightning) lightningCount++;
                else if (e == Magic.Combat.SpellElement.Earth) earthCount++;
            }

            AddCount(e1); AddCount(e2); AddCount(e3);

            int maxCount = 0;
            Magic.Combat.SpellElement winner = Magic.Combat.SpellElement.None;
            bool tie = false;

            void CheckMax(int count, Magic.Combat.SpellElement e) {
                if (count > maxCount) {
                    maxCount = count;
                    winner = e;
                    tie = false;
                } else if (count > 0 && count == maxCount) {
                    tie = true;
                }
            }

            CheckMax(fireCount, Magic.Combat.SpellElement.Fire);
            CheckMax(iceCount, Magic.Combat.SpellElement.Ice);
            CheckMax(lightningCount, Magic.Combat.SpellElement.Lightning);
            CheckMax(earthCount, Magic.Combat.SpellElement.Earth);

            return tie ? Magic.Combat.SpellElement.None : winner;
        }

        private void ExecuteSpell()
        {
            isChantingPhase = false;
            string rank = cachedSpellScore >= 0.85f ? "[대성공]" : "[성공]";
            string elementText = currentElement == SpellElement.None ? "" : $" [{currentElement}]";
            Debug.Log($"<color=cyan>[전투] 마법 발동! {rank}{elementText} [{cachedSpellName}] (Score: {cachedSpellScore:F2})</color>");
            float spellCost = 30f;
            if (drawingDatabase != null && drawingDatabase.recipes != null)
            {
                foreach (var r in drawingDatabase.recipes)
                {
                    if (r.SpellName == cachedSpellName)
                    {
                        spellCost = r.manaCost;
                        break;
                    }
                }
            }
            float castCost = spellCost * 0.5f;

            // 마나 체크 (실제로는 마나 소모가 필요)
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.currentMana >= castCost)
            {
                PlayerDataManager.Instance.currentMana -= castCost;
                if (manaSlider != null) manaSlider.SetValue(PlayerDataManager.Instance.currentMana, PlayerDataManager.Instance.GetMaxMana());

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

    }
}
