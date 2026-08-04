using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventPopupUI : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    [SerializeField] CanvasGroup content;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    
    [SerializeField] private Button btnOptionA; // 스크롤 사용
    [SerializeField] private Button btnOptionB; // 즉석 마법 (그리기)
    [SerializeField] private Button btnOptionC; // 맨몸 돌파

    [SerializeField] CanvasGroup drawingContent;
    [SerializeField] private EventDrawingManager eventDrawingManager;

    private HexTileData currentTile;
    private int currentPhaseIndex = 0;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnTileEventTriggered += ShowPopup;
            Debug.Log("[EventPopupUI] Successfully subscribed to OnTileEventTriggered.");
        }
        else
        {
            Debug.LogError("[EventPopupUI] DungeonManager.Instance is NULL during Start()!");
        }
        
        if (btnOptionA != null) btnOptionA.onClick.AddListener(OnOptionAClicked);
        if (btnOptionB != null) btnOptionB.onClick.AddListener(OnOptionBClicked);
        if (btnOptionC != null) btnOptionC.onClick.AddListener(OnOptionCClicked);
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnScrollEquipped += HandleScrollEquipped;
        }
        
        HidePopupUI();
    }

    private void OnDestroy()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnTileEventTriggered -= ShowPopup;
        }
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnScrollEquipped -= HandleScrollEquipped;
        }
    }

    private void ShowPopupUI()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private void HidePopupUI()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        if (eventDrawingManager != null) eventDrawingManager.enabled = false;
    }

    private void ShowPopup(HexTileData tile)
    {
        Debug.Log($"[EventPopupUI] ShowPopup called for tile {tile.EventData.eventTitle}");
        currentTile = tile;
        currentPhaseIndex = 0;
        
        if (content != null)
        {
            content.alpha = 1f;
            content.interactable = true;
            content.blocksRaycasts = true;
        }
        if (drawingContent != null)
        {
            drawingContent.alpha = 0f;
            drawingContent.interactable = false;
            drawingContent.blocksRaycasts = false;
        }
        if (eventDrawingManager != null) eventDrawingManager.enabled = false;
        
        UpdatePopupUI();
        ShowPopupUI();
    }

    private void UpdatePopupUI()
    {
        if (titleText != null) titleText.text = $"이벤트 발생! ({currentTile.EventData.eventTitle})";
        
        string reqText = "";
        if (currentTile.Type == HexTileType.Boss || currentTile.Type == HexTileType.NormalMonster)
        {
            if (currentTile.EventData.requiredElements != null && currentTile.EventData.requiredElements.Count > currentPhaseIndex)
            {
                reqText = $"\n<color=orange>(필요 속성: {currentTile.EventData.requiredElements[currentPhaseIndex]})</color>";
                if (currentTile.Type == HexTileType.Boss) reqText += $" - {currentPhaseIndex + 1}/{currentTile.EventData.requiredElements.Count} 페이즈";
            }
        }
        else if (currentTile.Type == HexTileType.Trap || currentTile.Type == HexTileType.Obstacle || currentTile.Type == HexTileType.NPC)
        {
            reqText = $"\n<color=orange>(필요 계열: {currentTile.EventData.requiredSpellType})</color>";
        }

        if (descText != null) descText.text = $"{currentTile.EventData.eventDescription}{reqText}";
        
        var txtA = btnOptionA != null ? btnOptionA.GetComponentInChildren<TextMeshProUGUI>() : null;
        var txtB = btnOptionB != null ? btnOptionB.GetComponentInChildren<TextMeshProUGUI>() : null;
        var txtC = btnOptionC != null ? btnOptionC.GetComponentInChildren<TextMeshProUGUI>() : null;

        if (btnOptionA != null) btnOptionA.gameObject.SetActive(true);
        if (btnOptionB != null) btnOptionB.gameObject.SetActive(true);
        if (btnOptionC != null) btnOptionC.gameObject.SetActive(true);

        if (currentTile.Type == HexTileType.Merchant)
        {
            if (txtA != null) txtA.text = "가진 소재 모두 팔기";
            if (txtB != null) txtB.text = "무시하고 지나가기";
            if (btnOptionC != null) btnOptionC.gameObject.SetActive(false);
            if (btnOptionA != null) btnOptionA.interactable = true;
        }
        else if (currentTile.Type == HexTileType.Altar)
        {
            if (txtA != null) txtA.text = "스크롤 제물 바치기 (AP +5)";
            if (txtB != null) txtB.text = "마나 20 바치기 (AP +5)";
            if (txtC != null) txtC.text = "무시하고 지나가기";
            if (btnOptionA != null) btnOptionA.interactable = true;
        }
        else if (currentTile.Type == HexTileType.Exit)
        {
            if (txtA != null) txtA.text = "탈출하기";
            if (btnOptionB != null) btnOptionB.gameObject.SetActive(false);
            if (btnOptionC != null) btnOptionC.gameObject.SetActive(false);
            if (btnOptionA != null) btnOptionA.interactable = true;
        }
        else if (currentTile.Type == HexTileType.Resource)
        {
            if (txtB != null) txtB.text = "즉석 그리기 (10초, 보너스)";
            if (txtC != null) txtC.text = "기본 채집 (돌파)";
            UpdateScrollButtonState();
        }
        else
        {
            if (txtB != null) txtB.text = "즉석 그리기 (10초)";
            if (txtC != null) txtC.text = "맨몸 돌파 (피해 감수)";
            UpdateScrollButtonState();
        }
    }

    private void HandleScrollEquipped(Item_Scroll scroll)
    {
        if (canvasGroup != null && canvasGroup.alpha > 0f && currentTile != null)
        {
            if (currentTile.Type == HexTileType.Boss || currentTile.Type == HexTileType.NormalMonster ||
                currentTile.Type == HexTileType.Trap || currentTile.Type == HexTileType.Obstacle || 
                currentTile.Type == HexTileType.NPC || currentTile.Type == HexTileType.Resource)
            {
                UpdateScrollButtonState();
            }
        }
    }

    private void UpdateScrollButtonState()
    {
        if (btnOptionA == null) return;
        var txtA = btnOptionA.GetComponentInChildren<TextMeshProUGUI>();

        Item_Scroll equipped = InventoryManager.Instance != null ? InventoryManager.Instance.EquippedScroll : null;
        
        if (equipped != null && !equipped.isEmpty)
        {
            bool meetsRequirement = CheckSpellRequirementSilent(equipped.ScrollData.spellName, equipped.ScrollData.scrollElement);
            if (meetsRequirement)
            {
                if (txtA != null) txtA.text = $"스크롤 사용 ({equipped.ScrollData.spellName})";
                btnOptionA.interactable = true;
            }
            else
            {
                if (txtA != null) txtA.text = "조건 불일치 스크롤";
                btnOptionA.interactable = false;
            }
        }
        else
        {
            if (txtA != null) txtA.text = currentTile.Type == HexTileType.Resource ? "채집 마법 스크롤 사용 (보너스)" : "스크롤 사용";
            btnOptionA.interactable = false;
        }
    }

    private bool CheckSpellRequirementSilent(string spellName, SpellElement element)
    {
        if (currentTile.Type == HexTileType.Boss || currentTile.Type == HexTileType.NormalMonster)
        {
            SpellElement req = currentTile.EventData.requiredElements[currentPhaseIndex];
            if (element != req) return false;
        }
        return true;
    }

    private void OnOptionAClicked()
    {
        if (currentTile.Type == HexTileType.Merchant)
        {
            if (InventoryManager.Instance.combatLoadout.Count > 0)
            {
                var lostItem = InventoryManager.Instance.combatLoadout[0];
                InventoryManager.Instance.combatLoadout.RemoveAt(0);
                InventoryManager.Instance.NotifyInventoryChanged();
                Debug.Log($"<color=green>[상인] {lostItem.ItemName}을(를) 주고 잉크/물약을 획득했습니다!</color>");
            }
            else
            {
                Debug.Log("<color=yellow>[상인] 교환할 소재가 부족합니다!</color>");
            }
            DungeonManager.Instance.ResolveEvent(currentTile, true);
            HidePopupUI();
            return;
        }
        else if (currentTile.Type == HexTileType.Altar)
        {
            Item_Scroll altarScroll = InventoryManager.Instance.EquippedScroll;
            if (altarScroll != null && !altarScroll.isEmpty)
            {
                altarScroll.isEmpty = true;
                DungeonManager.Instance.ApplyAltarBuff();
                DungeonManager.Instance.ResolveEvent(currentTile, true);
                Debug.Log("<color=cyan>[제단] 스크롤을 바쳐 축복을 받았습니다!</color>");
            }
            else
            {
                Debug.LogWarning("[제단] 바칠 완성된 스크롤이 없습니다!");
            }
            HidePopupUI();
            return;
        }
        else if (currentTile.Type == HexTileType.Exit)
        {
            DungeonManager.Instance.ResolveEvent(currentTile, true);
            HidePopupUI();
            return;
        }

        Item_Scroll scroll = InventoryManager.Instance.EquippedScroll;
        if (scroll != null && !scroll.isEmpty)
        {
            if (!CheckSpellRequirement(scroll.ScrollData.spellName, scroll.ScrollData.scrollElement)) return;
            
            scroll.isEmpty = true;
            Debug.Log($"<color=cyan>[이벤트] 스크롤({scroll.ScrollData?.spellName})을 사용하여 돌파했습니다!</color>");
            if (InventoryManager.Instance != null) InventoryManager.Instance.NotifyInventoryChanged();
            ProceedPhase(true);
        }
    }

    private bool CheckSpellRequirement(string spellName, SpellElement element)
    {
        // 몬스터/보스
        if (currentTile.Type == HexTileType.Boss || currentTile.Type == HexTileType.NormalMonster)
        {
            SpellElement req = currentTile.EventData.requiredElements[currentPhaseIndex];
            if (element != req)
            {
                Debug.LogWarning($"[이벤트] 마법 속성({element})이 몬스터 약점({req})과 다릅니다!");
                return false;
            }
        }
        // 장애물, 함정, NPC
        else if (currentTile.Type == HexTileType.Trap || currentTile.Type == HexTileType.Obstacle || currentTile.Type == HexTileType.NPC)
        {
            // 여기서는 스크롤의 마법 종류(Type)를 체크해야 하지만 ScrollData에는 Type이 없음.
            // RecipeAsset을 통해 가져오거나 임시로 이름을 체크함. 여기서는 편의상 성공 처리.
            Debug.Log("<color=yellow>[이벤트] 유틸/방어 마법 발동!</color>");
        }
        return true;
    }

    private void ProceedPhase(bool isMagicUsed)
    {
        if (currentTile.Type == HexTileType.Boss)
        {
            currentPhaseIndex++;
            if (currentPhaseIndex < currentTile.EventData.requiredElements.Count)
            {
                Debug.Log($"<color=orange>[수문장] {currentPhaseIndex} 페이즈 돌파! 다음 약점을 공략하세요.</color>");
                UpdatePopupUI();
                return;
            }
        }
        
        // 보상 처리
        int rewardMultiplier = 1;
        if (currentTile.Type == HexTileType.Resource)
        {
            rewardMultiplier = isMagicUsed ? 3 : 1;
            Debug.Log($"<color=cyan>[채집] 채집 완료! (보너스 배율: x{rewardMultiplier})</color>");
        }
        
        DungeonManager.Instance.ResolveEvent(currentTile, true, rewardMultiplier);
        HidePopupUI();
    }

    private void OnOptionBClicked()
    {
        if (currentTile.Type == HexTileType.Merchant)
        {
            DungeonManager.Instance.ResolveEvent(currentTile, true);
            HidePopupUI();
            return;
        }
        else if (currentTile.Type == HexTileType.Altar)
        {
            if (PlayerDataManager.Instance.currentMana >= 20f)
            {
                PlayerDataManager.Instance.currentMana -= 20f;
                PlayerDataManager.Instance.NotifyManaChanged();
                DungeonManager.Instance.ApplyAltarBuff();
                DungeonManager.Instance.ResolveEvent(currentTile, true);
                Debug.Log("<color=cyan>[제단] 마나를 바쳐 축복을 받았습니다!</color>");
            }
            else
            {
                Debug.LogWarning("[제단] 마나가 부족합니다!");
            }
            HidePopupUI();
            return;
        }

        // 이벤트 즉석 그리기는 마나를 소모하지 않음
        Debug.Log("<color=yellow>[이벤트] 즉석 그리기 모드로 전환합니다!</color>");
        
        if (drawingContent != null)
        {
            drawingContent.alpha = 1f;
            drawingContent.interactable = true;
            drawingContent.blocksRaycasts = true;
        }
        if (content != null)
        {
            content.alpha = 0f;
            content.interactable = false;
            content.blocksRaycasts = false;
        }
        
        if (eventDrawingManager != null)
        {
            eventDrawingManager.enabled = true;
            eventDrawingManager.OnEventSpellMatched -= HandleDrawnSpell;
            eventDrawingManager.OnEventSpellMatched += HandleDrawnSpell;
            
            eventDrawingManager.OnCancelDrawing -= CancelDrawingMode;
            eventDrawingManager.OnCancelDrawing += CancelDrawingMode;
        }
    }

    private void CancelDrawingMode()
    {
        if (eventDrawingManager != null)
        {
            eventDrawingManager.enabled = false;
            eventDrawingManager.OnEventSpellMatched -= HandleDrawnSpell;
            eventDrawingManager.OnCancelDrawing -= CancelDrawingMode;
            eventDrawingManager.ClearDrawing();
        }
        
        if (content != null)
        {
            content.alpha = 1f;
            content.interactable = true;
            content.blocksRaycasts = true;
        }
        if (drawingContent != null)
        {
            drawingContent.alpha = 0f;
            drawingContent.interactable = false;
            drawingContent.blocksRaycasts = false;
        }
    }

    private void HandleDrawnSpell(SpellRecipeAsset recipe, float score)
    {
        if (recipe == null)
        {
            EndDrawingMode(false, null);
            return;
        }
        
        // 해당 마법을 발동하기 위한 마나 체크 및 소비
        if (PlayerDataManager.Instance.currentMana < recipe.manaCost)
        {
            Debug.LogWarning($"[이벤트] 마나가 부족하여 마법({recipe.SpellName})을 발동할 수 없습니다! (필요: {recipe.manaCost})");
            EndDrawingMode(false, recipe);
            return;
        }
        
        PlayerDataManager.Instance.currentMana -= recipe.manaCost;
        PlayerDataManager.Instance.NotifyManaChanged();
        Debug.Log($"<color=cyan>[이벤트] {recipe.SpellName} 발동! 마나 {recipe.manaCost}을 소모했습니다.</color>");
        
        if (currentTile.Type == HexTileType.Boss || currentTile.Type == HexTileType.NormalMonster)
        {
            SpellElement req = currentTile.EventData.requiredElements[currentPhaseIndex];
            if (recipe.Element != req)
            {
                Debug.Log($"<color=red>[이벤트] 마법은 완성했지만 몬스터 약점({req})과 다릅니다!</color>");
                EndDrawingMode(false, recipe);
                return;
            }
        }
        else if (currentTile.Type == HexTileType.Trap || currentTile.Type == HexTileType.Obstacle || currentTile.Type == HexTileType.NPC)
        {
            if (recipe.Type != currentTile.EventData.requiredSpellType)
            {
                Debug.Log($"<color=red>[이벤트] 마법은 완성했지만 필요한 계열({currentTile.EventData.requiredSpellType})이 아닙니다!</color>");
                EndDrawingMode(false, recipe);
                return;
            }
        }
        
        EndDrawingMode(true, recipe);
    }

    private void EndDrawingMode(bool success, SpellRecipeAsset recipe)
    {
        if (eventDrawingManager != null)
        {
            eventDrawingManager.enabled = false;
            eventDrawingManager.OnEventSpellMatched -= HandleDrawnSpell;
            eventDrawingManager.OnCancelDrawing -= CancelDrawingMode;
            eventDrawingManager.ClearDrawing();
        }
        
        if (content != null)
        {
            content.alpha = 1f;
            content.interactable = true;
            content.blocksRaycasts = true;
        }
        if (drawingContent != null)
        {
            drawingContent.alpha = 0f;
            drawingContent.interactable = false;
            drawingContent.blocksRaycasts = false;
        }
        
        if (success)
        {
            Debug.Log($"<color=green>[이벤트] {recipe?.SpellName} 발동 성공! 돌파했습니다!</color>");
            ProceedPhase(true);
        }
        else
        {
            Debug.Log("<color=red>[이벤트] 돌파 실패! 데미지를 입었습니다.</color>");
            DungeonManager.Instance.ResolveEvent(currentTile, false);
            HidePopupUI();
        }
    }

    private void OnOptionCClicked()
    {
        if (currentTile.Type == HexTileType.Altar || currentTile.Type == HexTileType.Merchant)
        {
            DungeonManager.Instance.ResolveEvent(currentTile, true);
            HidePopupUI();
            return;
        }
        
        Debug.Log("<color=orange>[이벤트] 맨몸으로 강행 돌파합니다!</color>");
        
        if (currentTile.Type == HexTileType.Resource)
        {
            ProceedPhase(false);
            return;
        }
        
        if (Random.value > 0.5f && InventoryManager.Instance.combatLoadout.Count > 0)
        {
            var lostItem = InventoryManager.Instance.combatLoadout[Random.Range(0, InventoryManager.Instance.combatLoadout.Count)];
            InventoryManager.Instance.RemoveItem(lostItem);
            Debug.Log($"<color=red>[이벤트] 돌파 중 아이템을 잃어버렸습니다: {lostItem.ItemName}</color>");
        }
        
        DungeonManager.Instance.ResolveEvent(currentTile, false);
        HidePopupUI();
    }
}
