using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("Dungeon Settings")]
    public int initialAP = 20;
    
    [Header("Current State")]
    public int currentAP;
    public Vector3Int currentPos;
    
    [Header("References")]
    public DungeonMapGenerator mapGenerator;
    
    private Dictionary<Vector3Int, HexTileData> tileMap;
    private HashSet<ItemInstance> dungeonLoot = new HashSet<ItemInstance>();
    
    public int apCostReductionBuffTurns = 0;
    
    public event System.Action<int> OnAPChanged;
    public event System.Action<HexTileData> OnTileEventTriggered;

    private void Awake()
    {
        Instance = this;
        EnterDungeon();
    }

    private void Update()
    {
        // UI 클릭 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (tileMap == null || mapGenerator == null || mapGenerator.dungeonTilemap == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = mapGenerator.dungeonTilemap.WorldToCell(mousePos);
            cellPos.z = 0;
            
            if (tileMap.ContainsKey(cellPos))
            {
                MoveTo(cellPos);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = mapGenerator.dungeonTilemap.WorldToCell(mousePos);
            cellPos.z = 0;
            
            if (tileMap.ContainsKey(cellPos))
            {
                InspectTile(cellPos);
            }
        }
    }

    public void EnterDungeon()
    {
        currentAP = initialAP;
        OnAPChanged?.Invoke(currentAP);
        
        dungeonLoot.Clear();
        
        tileMap = mapGenerator.GenerateMap();
        
        // 시작 위치 0,0,0
        currentPos = Vector3Int.zero;
        apCostReductionBuffTurns = 0;
        UpdateFogOfWar();
        
        Debug.Log("<color=green>[던전] 던전에 입장했습니다!</color>");
    }
    
    public void AddLoot(ItemInstance loot)
    {
        if (InventoryManager.Instance.AddDungeonLootToLoadout(loot))
        {
            dungeonLoot.Add(loot);
            Debug.Log($"<color=cyan>[던전] 아이템 획득: {loot.ItemName}</color>");
        }
    }

    public void MoveTo(Vector3Int targetPos)
    {
        if (currentAP <= 0) return;
        
        if (GetDistance(currentPos, targetPos) == 1)
        {
            int moveCost = 1;
            if (apCostReductionBuffTurns > 0)
            {
                moveCost = 0;
                apCostReductionBuffTurns--;
                if (apCostReductionBuffTurns == 0)
                    Debug.Log("<color=yellow>[버프] AP 소모 감소(공중부양) 효과가 끝났습니다.</color>");
            }
            
            currentAP -= moveCost;
            OnAPChanged?.Invoke(currentAP);
            
            // MP 회복
            PlayerDataManager.Instance.currentMana += 5f;
            if (PlayerDataManager.Instance.currentMana > PlayerDataManager.Instance.GetMaxMana())
                PlayerDataManager.Instance.currentMana = PlayerDataManager.Instance.GetMaxMana();
            PlayerDataManager.Instance.NotifyManaChanged();

            currentPos = targetPos;
            UpdateFogOfWar();
            
            CheckTileEvent(targetPos);
            
            if (currentAP <= 0)
            {
                DieInDungeon();
            }
        }
    }

    private void CheckTileEvent(Vector3Int pos)
    {
        if (tileMap.TryGetValue(pos, out HexTileData tile))
        {
            if (!tile.IsEventCleared && tile.Type != HexTileType.Empty && tile.Type != HexTileType.Start)
            {
                if (tile.Type == HexTileType.Trap && !tile.isTrapRevealed)
                {
                    tile.RevealTrap();
                    Debug.Log("<color=red>[이벤트] 숨겨진 기습 함정이 정체를 드러냈습니다!</color>");
                    // 함정 타일로 시각적 업데이트 필요 시 이 부분에서 SetTile을 다시 호출할 수 있습니다.
                    if (mapGenerator.dungeonTilemap != null)
                    {
                        mapGenerator.dungeonTilemap.SetTile(pos, mapGenerator.GetRevealedTile(tile));
                    }
                }
                
                if (tile.Type == HexTileType.Sight)
                {
                    RevealSight(pos, 3);
                    tile.ClearEvent();
                    Debug.Log("<color=yellow>[이벤트] 시야 관측소: 주변의 안개가 넓게 걷힙니다!</color>");
                }
                else if (tile.Type == HexTileType.RandomPortal)
                {
                    TeleportToRandomUnknown();
                    tile.ClearEvent();
                }
                else
                {
                    OnTileEventTriggered?.Invoke(tile);
                }
            }
        }
    }

    public void ResolveEvent(HexTileData tile, bool success, int rewardMultiplier = 1)
    {
        if (success)
        {
            tile.ClearEvent();
            if (tile.Type == HexTileType.NPC)
            {
                SpawnSecretRoom();
            }

            if (tile.EventData.rewardAmount > 0)
            {
                int totalReward = tile.EventData.rewardAmount * rewardMultiplier;
                Debug.Log($"<color=cyan>[보상] 던전 소재 {totalReward}개를 획득했습니다! (임시 가방에 추가됨)</color>");
                // 실제 연동 시: AddLoot(new ItemInstance(someMaterialData, totalReward));
            }

            if (tile.Type == HexTileType.Exit)
            {
                EscapeDungeon();
            }
        }
        else
        {
            // 실패 페널티 (AP 감소 등)
            currentAP -= 5;
            OnAPChanged?.Invoke(currentAP);
            if (currentAP <= 0) DieInDungeon();
        }
    }

    private void UpdateFogOfWar()
    {
        RevealSight(currentPos, 1);
    }

    private void RevealSight(Vector3Int center, int rad)
    {
        for (int q = -rad; q <= rad; q++)
        {
            int r1 = Mathf.Max(-rad, -q - rad);
            int r2 = Mathf.Min(rad, -q + rad);
            for (int r = r1; r <= r2; r++)
            {
                Vector3Int p = new Vector3Int(center.x + q, center.y + r, 0);
                if (tileMap.TryGetValue(p, out HexTileData t))
                {
                    if (!t.IsDiscovered)
                    {
                        t.Discover();
                        if (mapGenerator.dungeonTilemap != null)
                        {
                            // 안개 타일 대신 탐색 완료된 타일(기본 바닥, 이벤트 아이콘 등)로 교체합니다.
                            mapGenerator.dungeonTilemap.SetTile(p, mapGenerator.GetRevealedTile(t));
                        }
                    }
                }
            }
        }
    }

    private void TeleportToRandomUnknown()
    {
        List<Vector3Int> unknowns = new List<Vector3Int>();
        foreach(var kv in tileMap)
        {
            if (!kv.Value.IsDiscovered) unknowns.Add(kv.Key);
        }
        
        if (unknowns.Count > 0)
        {
            Vector3Int target = unknowns[Random.Range(0, unknowns.Count)];
            currentPos = target;
            UpdateFogOfWar();
            Debug.Log($"<color=magenta>[이벤트] 알 수 없는 텔레포트에 의해 다른 곳으로 이동되었습니다!</color>");
            CheckTileEvent(target);
        }
    }

    private void EscapeDungeon()
    {
        Debug.Log("<color=yellow>[던전] 무사히 탈출했습니다! 전리품을 정산합니다.</color>");
        
        foreach (var item in dungeonLoot)
        {
            if (!InventoryManager.Instance.items.Contains(item))
            {
                InventoryManager.Instance.AddItem(item);
            }
        }
        
        InventoryManager.Instance.NotifyInventoryChanged();
    }

    private void DieInDungeon()
    {
        Debug.Log("<color=red>[던전] AP가 모두 소진되어 쓰러졌습니다... 전리품과 임시 가방의 일부를 잃어버립니다.</color>");
        
        foreach (var item in InventoryManager.Instance.combatLoadout)
        {
            if (InventoryManager.Instance.items.Contains(item))
            {
                InventoryManager.Instance.items.Remove(item);
            }
        }
        InventoryManager.Instance.combatLoadout.Clear();
        InventoryManager.Instance.NotifyInventoryChanged();
        InventoryManager.Instance.NotifyLoadoutChanged();
    }

    private int GetDistance(Vector3Int a, Vector3Int b)
    {
        return (Mathf.Abs(a.x - b.x) 
              + Mathf.Abs(a.x + a.y - b.x - b.y) 
              + Mathf.Abs(a.y - b.y)) / 2;
    }

    public void ApplyAltarBuff()
    {
        apCostReductionBuffTurns = 5;
        Debug.Log("<color=yellow>[제단] 5턴 동안 이동 시 AP가 소모되지 않습니다!</color>");
    }

    public void InspectTile(Vector3Int targetPos)
    {
        if (GetDistance(currentPos, targetPos) == 1)
        {
            if (tileMap.TryGetValue(targetPos, out HexTileData tile))
            {
                if (!tile.IsDiscovered)
                {
                    Debug.Log("<color=yellow>[탐색] 타일이 아직 밝혀지지 않았습니다.</color>");
                }
                else
                {
                    if (tile.Type == HexTileType.Trap && !tile.isTrapRevealed)
                    {
                        tile.RevealTrap();
                        Debug.Log("<color=red>[탐색] 숨겨진 함정을 발견했습니다!</color>");
                        if (mapGenerator.dungeonTilemap != null)
                        {
                            mapGenerator.dungeonTilemap.SetTile(targetPos, mapGenerator.GetRevealedTile(tile));
                        }
                    }
                    else
                    {
                        Debug.Log($"<color=cyan>[탐색] 정찰 결과: {tile.EventData.eventTitle} 입니다.</color>");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("[탐색] 인접한 1칸 거리의 타일만 정찰할 수 있습니다.");
        }
    }

    private void SpawnSecretRoom()
    {
        List<Vector3Int> unknownTiles = new List<Vector3Int>();
        foreach (var kvp in tileMap)
        {
            if (!kvp.Value.IsDiscovered && kvp.Value.Type == HexTileType.Empty)
            {
                unknownTiles.Add(kvp.Key);
            }
        }
        
        if (unknownTiles.Count > 0)
        {
            Vector3Int secretPos = unknownTiles[Random.Range(0, unknownTiles.Count)];
            
            HexTileEventData secretData = new HexTileEventData();
            secretData.type = HexTileType.Resource;
            secretData.eventTitle = "비밀 보물 방";
            secretData.eventDescription = "NPC가 알려준 숨겨진 엄청난 보물입니다!";
            secretData.rewardAmount = 10;
            
            HexTileData secretTile = new HexTileData(secretPos, secretData);
            tileMap[secretPos] = secretTile;
            
            RevealSight(secretPos, 0); // 핀포인트 시야 확보
            
            Debug.Log("<color=cyan>[NPC] 감사의 표시로 맵 어딘가에 있는 비밀 보물 방의 위치를 알려주었습니다!</color>");
        }
    }
}
