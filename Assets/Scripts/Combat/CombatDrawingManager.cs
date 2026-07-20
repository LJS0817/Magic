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
        private SpellElement cachedBaseElement = SpellElement.None;

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
            
            if (manaSlider != null && PlayerDataManager.Instance != null)
                manaSlider.SetValue(PlayerDataManager.Instance.currentMana, PlayerDataManager.Instance.GetMaxMana());

            if (penController != null && InventoryManager.Instance != null)
            {
                penController.forcedTool = InventoryManager.Instance.EquippedWand;
            }
        }

        private bool _isCastingFromScroll = false;
        private Item_Scroll _castingScroll = null;

        public void CastSpellFromScroll(Item_Scroll scroll)
        {
            if (scroll == null || scroll.isEmpty || scroll.ScrollData == null) return;
            
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.EquippedScroll = scroll;
            }
            
            _isCastingFromScroll = true;
            _castingScroll = scroll;
            OnSpellMatched(scroll.ScrollData.spellName, scroll.ScrollData.accuracyScore);
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
            
            float costMultiplier = _isCastingFromScroll ? 0.5f : 1.0f;
            float drawCost = spellCost * costMultiplier;

            if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.currentMana < drawCost)
            {
                Debug.Log($"<color=red>[전투] 마나가 부족하여 마법을 사용할 수 없습니다! (필요 마나: {drawCost:F1})</color>");
                ClearDrawing();
                _isCastingFromScroll = false;
                _castingScroll = null;
                return;
            }

            PlayerDataManager.Instance.currentMana -= drawCost;
            if (manaSlider != null) manaSlider.SetValue(PlayerDataManager.Instance.currentMana, PlayerDataManager.Instance.GetMaxMana());

            cachedBaseElement = _isCastingFromScroll && _castingScroll != null && _castingScroll.ScrollData != null ? _castingScroll.ScrollData.scrollElement : SpellElement.None;

            if (_isCastingFromScroll && _castingScroll != null)
            {
                ConsumeScrollDurability(_castingScroll);
                // 스크롤 빈 상태로 전환 (로드아웃에는 유지)
                _castingScroll.isEmpty = true;
                if (_castingScroll.ScrollData != null)
                {
                    _castingScroll.ScrollData.spellName = "";
                    _castingScroll.ScrollData.scrollElement = SpellElement.None;
                }
                if (InventoryManager.Instance != null) InventoryManager.Instance.NotifyInventoryChanged();
            }

            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.UnlockRecipe(matchedSpell);
            }

            cachedSpellName = matchedSpell;
            cachedSpellScore = averageScore;
            cachedSpellType = sType;
            cachedScroll = _isCastingFromScroll ? _castingScroll : null;
            
            currentChantInput.Clear();
            currentElement = SpellElement.None;
            _isCastingFromScroll = false;
            _castingScroll = null;

            if (sType == SpellType.Defense)
            {
                ExecuteSpell();
            }
            else
            {
                isChantingPhase = true;
                currentChantTimer = chantTimeLimit;
                CombatManager.Instance.StartPlayerChanting();
                chantUIScale = 0f;
                DOTween.To(() => chantUIScale, x => chantUIScale = x, 1f, 0.5f).SetEase(Ease.OutBack);
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
            
            List<SpellElement> appliedElements = new List<SpellElement>();
            if (cachedBaseElement != SpellElement.None) appliedElements.Add(cachedBaseElement);
            // 영창한 속성이 있고, 이미 있는 속성과 중복되지 않거나, 아예 다른 속성일 때 추가 (무속성 제외)
            if (currentElement != SpellElement.None && !appliedElements.Contains(currentElement)) appliedElements.Add(currentElement);
            
            if (appliedElements.Count == 0) appliedElements.Add(SpellElement.None);

            string elementText = " [" + string.Join(", ", appliedElements) + "]";
            Debug.Log($"<color=cyan>[전투] 마법 발동! {rank}{elementText} [{cachedSpellName}] (Score: {cachedSpellScore:F2})</color>");
            // 마나는 이미 OnSpellMatched 에서 소모했으므로 여기서 다시 소모하지 않음
            if (CombatManager.Instance.IsParryWindowOpen)
            {
                CombatManager.Instance.EvaluateParryClash(cachedSpellName, cachedSpellType, appliedElements);
            }
            else
            {
                if (cachedSpellType == SpellType.Defense)
                {
                    Debug.Log("<color=cyan>[전투] 방어 마법 전개! (자신에게 방어막 부여)</color>");
                }
                else
                {
                    foreach (var el in appliedElements)
                    {
                        CombatManager.Instance.enemy.TakeDamage(25f, el);
                    }
                }
                CombatManager.Instance.EndPlayerTurn();
            }

            // 속성 영창 초기화
            currentElement = SpellElement.None;
            cachedBaseElement = SpellElement.None;
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
            if (item == null) return;
            item.currentDurability--;
            if (item.currentDurability <= 0)
            {
                Debug.LogWarning("[전투] 스크롤의 내구도가 다하여 빛이 바랬습니다.");
                // 내구도가 0이 되어도 지우지 않고 유지
            }
        }

        protected override void HandleDrawingInput()
        {
            if (IsDrawingBlocked)
            {
                if (isDrawing) EndStroke();
                return;
            }

            ItemInstance currentTool = penController != null ? penController.CurrentTool : null;
            if (currentTool == null)
            {
                if (isDrawing) EndStroke();
                return;
            }

            // 전투에서는 스크롤 제한을 우회하여 마법을 그릴 수 있게 합니다. (지팡이 사용)
            if (penController != null && !isRefilling)
            {
                penController.TrackMouse(mainCamera);
            }

            if (Input.GetMouseButtonDown(0))
            {
                StartStroke();
            }
            else if (Input.GetMouseButton(0))
            {
                if (!isDrawing && isPenReady)
                {
                    StartStroke();
                }
                else
                {
                    UpdateStroke();
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                EndStroke();
            }
        }

    }
}
