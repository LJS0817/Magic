using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("Dungeon Settings")]
    public int initialAP = 20;
    
    [Header("Current State")]
    public int currentAP;
    public Vector2Int currentPos;
    
    [Header("References")]
    public DungeonMapGenerator mapGenerator;
    public Transform hexBoardParent;
    
    private Dictionary<Vector2Int, HexTile> tileMap;
    private HashSet<ItemInstance> dungeonLoot = new HashSet<ItemInstance>();
    
    public int apCostReductionBuffTurns = 0;
    
    public event System.Action<int> OnAPChanged;
    public event System.Action<HexTile> OnTileEventTriggered;

    private void Awake()
    {
        Instance = this;
    }

    public void EnterDungeon()
    {
        currentAP = initialAP;
        OnAPChanged?.Invoke(currentAP);
        
        dungeonLoot.Clear();
        
        tileMap = mapGenerator.GenerateMap(hexBoardParent);
        
        // 시작 위치 0,0
        currentPos = Vector2Int.zero;
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

    public void MoveTo(Vector2Int targetPos)
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

    private void CheckTileEvent(Vector2Int pos)
    {
        if (tileMap.TryGetValue(pos, out HexTile tile))
        {
            if (!tile.IsEventCleared && tile.Type != HexTileType.Empty && tile.Type != HexTileType.Start)
            {
                if (tile.Type == HexTileType.Trap && !tile.isTrapRevealed)
                {
                    tile.RevealTrap();
                    Debug.Log("<color=red>[이벤트] 숨겨진 기습 함정이 정체를 드러냈습니다!</color>");
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

    public void ResolveEvent(HexTile tile, bool success, int rewardMultiplier = 1)
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

    private void RevealSight(Vector2Int center, int rad)
    {
        for (int q = -rad; q <= rad; q++)
        {
            int r1 = Mathf.Max(-rad, -q - rad);
            int r2 = Mathf.Min(rad, -q + rad);
            for (int r = r1; r <= r2; r++)
            {
                Vector2Int p = new Vector2Int(center.x + q, center.y + r);
                if (tileMap.TryGetValue(p, out HexTile t))
                {
                    t.Discover();
                }
            }
        }
    }

    private void TeleportToRandomUnknown()
    {
        List<Vector2Int> unknowns = new List<Vector2Int>();
        foreach(var kv in tileMap)
        {
            if (!kv.Value.IsDiscovered) unknowns.Add(kv.Key);
        }
        
        if (unknowns.Count > 0)
        {
            Vector2Int target = unknowns[Random.Range(0, unknowns.Count)];
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

    private int GetDistance(Vector2Int a, Vector2Int b)
    {
        return (Mathf.Abs(a.x - b.x) 
              + Mathf.Abs(a.x + a.y - b.x - b.y) 
              + Mathf.Abs(a.y - b.y)) / 2;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        return new List<Vector2Int>
        {
            new Vector2Int(pos.x + 1, pos.y), new Vector2Int(pos.x + 1, pos.y - 1),
            new Vector2Int(pos.x, pos.y - 1), new Vector2Int(pos.x - 1, pos.y),
            new Vector2Int(pos.x - 1, pos.y + 1), new Vector2Int(pos.x, pos.y + 1)
        };
    }

    public void ApplyAltarBuff()
    {
        apCostReductionBuffTurns = 5;
        Debug.Log("<color=yellow>[제단] 5턴 동안 이동 시 AP가 소모되지 않습니다!</color>");
    }

    public void InspectTile(Vector2Int targetPos)
    {
        if (GetDistance(currentPos, targetPos) == 1)
        {
            if (tileMap.TryGetValue(targetPos, out HexTile tile))
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
        List<Vector2Int> unknownTiles = new List<Vector2Int>();
        foreach (var kvp in tileMap)
        {
            if (!kvp.Value.IsDiscovered && kvp.Value.Type == HexTileType.Empty)
            {
                unknownTiles.Add(kvp.Key);
            }
        }
        
        if (unknownTiles.Count > 0)
        {
            Vector2Int secretPos = unknownTiles[Random.Range(0, unknownTiles.Count)];
            HexTile secretTile = tileMap[secretPos];
            
            HexTileEventData secretData = new HexTileEventData();
            secretData.type = HexTileType.Resource;
            secretData.eventTitle = "비밀 보물 방";
            secretData.eventDescription = "NPC가 알려준 숨겨진 엄청난 보물입니다!";
            secretData.rewardAmount = 10;
            
            secretTile.Init(secretPos.x, secretPos.y, secretData);
            RevealSight(secretPos, 0); // 핀포인트 시야 확보
            
            Debug.Log("<color=cyan>[NPC] 감사의 표시로 맵 어딘가에 있는 비밀 보물 방의 위치를 알려주었습니다!</color>");
        }
    }
}
