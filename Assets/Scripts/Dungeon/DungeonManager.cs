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
    public DungeonTileManager tileManager;
    
    private Dictionary<Vector3Int, HexTileData> tileMap;
    private HashSet<ItemInstance> dungeonLoot = new HashSet<ItemInstance>();
    
    public int apCostReductionBuffTurns = 0;
    private Coroutine moveCoroutine;
    private List<Vector3Int> currentPath = new List<Vector3Int>();
    
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

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = tileManager.dungeonTilemap.WorldToCell(mousePos);
            cellPos.z = 0;

            if (tileMap.ContainsKey(cellPos))
            {
                Debug.Log("Move");
                if (HexPathfinding.GetNeighbors(currentPos).Contains(cellPos))
                {
                    if (moveCoroutine != null) StopCoroutine(moveCoroutine);
                    ClearPathTiles();
                    MoveTo(cellPos);
                }
                else
                {
                    MoveAlongPath(cellPos);
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = tileManager.dungeonTilemap.WorldToCell(mousePos);
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
        
        tileMap = tileManager.GenerateMap();
        
        // 시작 위치 0,0,0
        currentPos = Vector3Int.zero;
        apCostReductionBuffTurns = 0;
        
        // 시작 지점은 애니메이션 없이 즉시 밝히고 플레이어 배치
        if (tileMap.TryGetValue(currentPos, out HexTileData startTile))
        {
            startTile.Discover();
            tileManager.dungeonTilemap.SetTile(currentPos, tileManager.defaultFloorTile);
        }

        UpdateFogOfWar();
        SetPlayerTile(currentPos);
        
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
        
        if (HexPathfinding.GetNeighbors(currentPos).Contains(targetPos))
        {
            int moveCost = 1;
            if (apCostReductionBuffTurns > 0)
            {
                moveCost = 0;
                apCostReductionBuffTurns--;
                if (apCostReductionBuffTurns == 0)
                    Debug.Log("<color=yellow>[버프] AP 소모 감소(공중부양) 효과가 끝났습니다.</color>");
            }
            
            // currentAP -= moveCost;
            OnAPChanged?.Invoke(currentAP);
            
            // MP 회복
            PlayerDataManager.Instance.currentMana += 5f;
            if (PlayerDataManager.Instance.currentMana > PlayerDataManager.Instance.GetMaxMana())
                PlayerDataManager.Instance.currentMana = PlayerDataManager.Instance.GetMaxMana();
            PlayerDataManager.Instance.NotifyManaChanged();

            RestoreTile(currentPos);

            currentPos = targetPos;
            UpdateFogOfWar();
            
            //CheckTileEvent(targetPos);
            SetPlayerTile(currentPos);
            
            if (currentAP <= 0)
            {
                DieInDungeon();
            }
        }
    }

    private void ClearPathTiles()
    {
        if (currentPath != null)
        {
            foreach (Vector3Int p in currentPath)
            {
                if (p != currentPos)
                {
                    RestoreTile(p);
                }
            }
            currentPath.Clear();
        }
    }

    public void MoveAlongPath(Vector3Int targetPos)
    {
        List<Vector3Int> path = HexPathfinding.FindPath(currentPos, targetPos, tileMap);
        
        if (path != null && path.Count > 0)
        {
            ClearPathTiles();
            currentPath = new List<Vector3Int>(path);

            string pathLog = $"경로 탐색: {currentPos}";
            foreach(Vector3Int p in path)
            {
                pathLog += $" -> {p}";
                if (p != currentPos && tileManager.dungeonTilemap != null && tileManager.pathTile != null)
                {
                    tileManager.FlipToTile(p, tileManager.pathTile);
                }
            }
            Debug.Log(pathLog);

            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }
            moveCoroutine = StartCoroutine(MoveCoroutine(path));
        }
        else
        {
            Debug.LogWarning("갈 수 없는 경로입니다.");
        }
    }

    private System.Collections.IEnumerator MoveCoroutine(List<Vector3Int> path)
    {
        foreach (Vector3Int nextPos in path)
        {
            if (currentAP <= 0) 
            {
                Debug.LogWarning("AP가 부족하여 이동을 멈춥니다.");
                break;
            }
            
            MoveTo(nextPos);
            
            // 이벤트 타일을 밟았을 때 이동을 멈추는 로직입니다.
            // 현재 맵의 70%가 이벤트 타일이므로, 이 조건 때문에 한 칸만 이동하고 멈추게 됩니다.
            // 길찾기 테스트를 위해 임시로 주석 처리합니다.
            /*
            if (tileMap.TryGetValue(nextPos, out HexTileData tile))
            {
                if (!tile.IsEventCleared && tile.Type != HexTileType.Empty && tile.Type != HexTileType.Start)
                {
                    break;
                }
            }
            */

            yield return new WaitForSeconds(1f); 
        }
        
        ClearPathTiles();
        moveCoroutine = null;
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
                    if (tileManager.dungeonTilemap != null)
                    {
                        tileManager.FlipToTile(pos, tileManager.GetRevealedTile(tile));
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

    private void SetPlayerTile(Vector3Int pos)
    {
        if (tileManager.dungeonTilemap != null && tileManager.playerTile != null)
        {
            tileManager.FlipToTile(pos, tileManager.playerTile);
        }
    }

    private void RestoreTile(Vector3Int pos)
    {
        if (tileManager.dungeonTilemap != null && tileMap.TryGetValue(pos, out HexTileData tile))
        {
            if (tile.IsDiscovered)
            {
                tileManager.FlipToTile(pos, tileManager.GetRevealedTile(tile));
            }
            else
            {
                tileManager.FlipToTile(pos, tileManager.fogTile);
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
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<KeyValuePair<Vector3Int, int>> queue = new Queue<KeyValuePair<Vector3Int, int>>();
        
        queue.Enqueue(new KeyValuePair<Vector3Int, int>(center, 0));
        visited.Add(center);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            Vector3Int p = current.Key;
            int dist = current.Value;

            if (tileMap.TryGetValue(p, out HexTileData t))
            {
                if (!t.IsDiscovered)
                {
                    t.Discover();
                    if (tileManager.dungeonTilemap != null)
                    {
                        // 경로 상에 있는 타일이라면 pathTile을 시각적으로 유지하기 위해 덮어씌우지 않습니다.
                        if (currentPath != null && currentPath.Contains(p) && p != currentPos)
                        {
                            // do nothing
                        }
                        else
                        {
                            tileManager.FlipToTile(p, tileManager.GetRevealedTile(t));
                        }
                    }
                }
            }

            if (dist < rad)
            {
                foreach (Vector3Int neighbor in HexPathfinding.GetNeighbors(p))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(new KeyValuePair<Vector3Int, int>(neighbor, dist + 1));
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
            RestoreTile(currentPos);
            currentPos = target;
            UpdateFogOfWar();
            SetPlayerTile(currentPos);
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

    public void ApplyAltarBuff()
    {
        apCostReductionBuffTurns = 5;
        Debug.Log("<color=yellow>[제단] 5턴 동안 이동 시 AP가 소모되지 않습니다!</color>");
    }

    public void InspectTile(Vector3Int targetPos)
    {
        if (HexPathfinding.GetNeighbors(currentPos).Contains(targetPos))
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
                        if (tileManager.dungeonTilemap != null)
                        {
                            tileManager.FlipToTile(targetPos, tileManager.GetRevealedTile(tile));
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
