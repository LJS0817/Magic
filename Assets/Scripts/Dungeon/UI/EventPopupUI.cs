using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventPopupUI : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    [SerializeField] CanvasGroup content;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    
    [SerializeField] private Button btnOptionC; // 맨몸 돌파

    [Header("Exit Result UI")]
    [SerializeField] private Transform lootContainer;
    [SerializeField] private GameObject lootItemSlotPrefab;
    [SerializeField] private Sprite emptySlotIcon;

    private System.Collections.Generic.List<GameObject> spawnedLootSlots = new System.Collections.Generic.List<GameObject>();

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
        
        if (btnOptionC != null) btnOptionC.onClick.AddListener(OnOptionCClicked);
        
        HidePopupUI();
    }

    private void OnDestroy()
    {
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.OnTileEventTriggered -= ShowPopup;
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
        
        UpdatePopupUI();
        ShowPopupUI();
    }

    private void UpdatePopupUI()
    {
        ClearLootSlots();

        if (currentTile.Type == HexTileType.Exit)
        {
            if (titleText != null) titleText.text = "<color=yellow><b>던전 출구 도달!</b></color>";

            float elapsedTime = DungeonManager.Instance != null ? DungeonManager.Instance.GetExplorationTime() : 0f;
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            string timeStr = $"{minutes:D2}분 {seconds:D2}초";

            if (descText != null)
            {
                descText.text = $"<b><color=#FFD700>무사히 탐험을 마쳤습니다!</color></b>\n" +
                                $"<size=18><b>탐험 시간:</b> <color=#00FFFF>{timeStr}</color></size>\n\n" +
                                $"<b><color=#A020F0>이번 모험에서 획득한 아이템:</color></b>";
            }

            if (lootContainer != null)
            {
                lootContainer.gameObject.SetActive(true);
                PopulateLootSlots();
            }

            var txtExit = btnOptionC != null ? btnOptionC.GetComponentInChildren<TextMeshProUGUI>() : null;
            if (btnOptionC != null) btnOptionC.gameObject.SetActive(true);
            if (txtExit != null) txtExit.text = "탈출 정산 및 복귀";
            return;
        }

        if (lootContainer != null) lootContainer.gameObject.SetActive(false);

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
        
        var txtC = btnOptionC != null ? btnOptionC.GetComponentInChildren<TextMeshProUGUI>() : null;

        if (btnOptionC != null) btnOptionC.gameObject.SetActive(true);

        if (currentTile.Type == HexTileType.Merchant)
        {
            if (txtC != null) txtC.text = "지나가기 / 거래는 버튼 클릭(미구현)"; 
        }
        else if (currentTile.Type == HexTileType.TreasureBox)
        {
            if (txtC != null) txtC.text = "강제로 열기 (돌파)";
        }
        else
        {
            if (txtC != null) txtC.text = "맨몸 돌파 (피해 감수)";
        }
    }

    private void PopulateLootSlots()
    {
        if (lootContainer == null || DungeonManager.Instance == null) return;
        var lootList = DungeonManager.Instance.GetDungeonLootList();
        if (lootList == null) return;

        foreach (var item in lootList)
        {
            if (lootItemSlotPrefab != null)
            {
                GameObject slotObj = Instantiate(lootItemSlotPrefab, lootContainer);
                InventorySlot slot = slotObj.GetComponent<InventorySlot>();
                if (slot != null)
                {
                    slot.Initialize(null, null, null, null, null);
                    slot.SetInfo(item, null, emptySlotIcon, false);
                }
                spawnedLootSlots.Add(slotObj);
            }
        }
    }

    private void ClearLootSlots()
    {
        foreach (var slot in spawnedLootSlots)
        {
            if (slot != null) Destroy(slot);
        }
        spawnedLootSlots.Clear();
    }

    public bool TrySolveEvent(string spellName, SpellElement element)
    {
        if (currentTile == null || canvasGroup.alpha == 0f) return false;

        // 보스, 일반 몬스터, 함정, 장애물, 자원, NPC 등에 대해서 해결 시도
        if (currentTile.Type == HexTileType.Merchant || currentTile.Type == HexTileType.Exit)
        {
            Debug.LogWarning("[이벤트] 이 타일에서는 마법으로 이벤트를 해결할 수 없습니다.");
            return false;
        }

        if (!CheckSpellRequirementSilent(spellName, element))
        {
            Debug.LogWarning($"[이벤트] 마법 속성({element})이 요구 조건과 맞지 않습니다!");
            return false;
        }

        Debug.Log($"<color=cyan>[이벤트] 외부 마법({spellName})으로 이벤트를 돌파했습니다!</color>");
        ProceedPhase(true);
        return true;
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
        if (currentTile.Type == HexTileType.TreasureBox)
        {
            rewardMultiplier = isMagicUsed ? 3 : 1;
            Debug.Log($"<color=cyan>[보물] 보물상자를 열었습니다! (보너스 배율: x{rewardMultiplier})</color>");
        }
        
        DungeonManager.Instance.ResolveEvent(currentTile, true, rewardMultiplier);
        HidePopupUI();
    }



    private void OnOptionCClicked()
    {
        if (currentTile.Type == HexTileType.Exit)
        {
            DungeonManager.Instance.ResolveEvent(currentTile, true);
            HidePopupUI();
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("StoreGameScene");
            return;
        }

        if (currentTile.Type == HexTileType.Merchant)
        {
            DungeonManager.Instance.ResolveEvent(currentTile, true);
            HidePopupUI();
            return;
        }
        
        Debug.Log("<color=orange>[이벤트] 맨몸으로 강행 돌파합니다!</color>");
        
        if (currentTile.Type == HexTileType.TreasureBox)
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
