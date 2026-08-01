using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonMapGenerator : MonoBehaviour
{
    [Header("Tilemap")]
    public Tilemap dungeonTilemap;
    
    [Header("Tiles")]
    public TileBase fogTile; // 미탐색 타일
    public TileBase defaultFloorTile; // Empty, Start 타일용
    
    [Header("Event Tiles")]
    public TileBase normalMonsterTile;
    public TileBase bossTile;
    public TileBase trapTile;
    public TileBase obstacleTile;
    public TileBase altarTile;
    public TileBase sightTile;
    public TileBase randomPortalTile;
    public TileBase resourceTile;
    public TileBase merchantTile;
    public TileBase npcTile;
    public TileBase exitTile;
    
    [Header("Grid Settings")]
    public int radius = 3;

    private Dictionary<Vector3Int, HexTileData> tileMapData = new Dictionary<Vector3Int, HexTileData>();

    public Dictionary<Vector3Int, HexTileData> GenerateMap()
    {
        if (dungeonTilemap != null) dungeonTilemap.ClearAllTiles();
        tileMapData.Clear();

        // 헥사곤 그리드 생성 (육각 형태)
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                Vector3Int cellPos = new Vector3Int(q, r, 0);
                
                // 랜덤 타입 부여 (시작 지점은 0,0)
                HexTileEventData randomData = GetRandomEventData(q, r);
                HexTileData tileData = new HexTileData(cellPos, randomData);
                
                tileMapData.Add(cellPos, tileData);

                // 초기 상태는 모두 안개(Fog) 타일로 덮습니다. (단, 시작 지점 제외 로직을 넣으려면 DungeonManager에서 호출 시 처리)
                if (dungeonTilemap != null && fogTile != null)
                {
                    dungeonTilemap.SetTile(cellPos, fogTile);
                }
            }
        }
        
        return tileMapData;
    }

    public TileBase GetRevealedTile(HexTileData tileData)
    {
        switch (tileData.Type)
        {
            case HexTileType.NormalMonster: return normalMonsterTile != null ? normalMonsterTile : defaultFloorTile;
            case HexTileType.Boss:          return bossTile != null ? bossTile : defaultFloorTile;
            case HexTileType.Trap:          return trapTile != null ? trapTile : defaultFloorTile;
            case HexTileType.Obstacle:      return obstacleTile != null ? obstacleTile : defaultFloorTile;
            case HexTileType.Altar:         return altarTile != null ? altarTile : defaultFloorTile;
            case HexTileType.Sight:         return sightTile != null ? sightTile : defaultFloorTile;
            case HexTileType.RandomPortal:  return randomPortalTile != null ? randomPortalTile : defaultFloorTile;
            case HexTileType.Resource:      return resourceTile != null ? resourceTile : defaultFloorTile;
            case HexTileType.Merchant:      return merchantTile != null ? merchantTile : defaultFloorTile;
            case HexTileType.NPC:           return npcTile != null ? npcTile : defaultFloorTile;
            case HexTileType.Exit:          return exitTile != null ? exitTile : defaultFloorTile;
            case HexTileType.Start:
            case HexTileType.Empty:
            default:
                return defaultFloorTile;
        }
    }

    private HexTileEventData GetRandomEventData(int q, int r)
    {
        HexTileEventData data = new HexTileEventData();
        
        if (q == 0 && r == 0)
        {
            data.type = HexTileType.Start;
            return data;
        }
        
        float rand = Random.value;
        if (rand < 0.05f) 
        {
            data.type = HexTileType.Exit;
            data.eventTitle = "탈출구";
            data.eventDescription = "무사히 상점으로 귀환할 수 있습니다.";
        }
        else if (rand < 0.1f)
        {
            data.type = HexTileType.Boss;
            data.requiredElements = new System.Collections.Generic.List<SpellElement>() { 
                (SpellElement)Random.Range(1, 5), 
                (SpellElement)Random.Range(1, 5) 
            };
            data.eventTitle = "수문장";
            data.eventDescription = "강력한 몬스터가 길을 막고 있습니다. 연속으로 마법을 맞혀야 합니다.";
            data.rewardAmount = 5;
        }
        else if (rand < 0.25f)
        {
            data.type = HexTileType.NormalMonster;
            data.requiredElements = new System.Collections.Generic.List<SpellElement>() { (SpellElement)Random.Range(1, 5) };
            data.eventTitle = "일반 몬스터";
            data.eventDescription = "던전 생물이 공격해옵니다.";
            data.rewardAmount = 1;
        }
        else if (rand < 0.35f)
        {
            data.type = HexTileType.Resource;
            data.eventTitle = "채집 노드";
            data.eventDescription = "유용한 소재를 채집할 수 있습니다.";
            data.rewardAmount = 1; // 기본 1개, 마법 쓰면 보너스
        }
        else if (rand < 0.40f)
        {
            data.type = HexTileType.Obstacle;
            data.requiredSpellType = SpellType.Utility;
            data.eventTitle = "환경 장애물";
            data.eventDescription = "가시 덤불이 앞을 가로막습니다. 유틸리티 마법이 필요합니다.";
        }
        else if (rand < 0.45f)
        {
            data.type = HexTileType.Trap;
            data.requiredSpellType = SpellType.Defense;
            data.eventTitle = "기습 함정";
            data.eventDescription = "함정이 발동했습니다! 방어 마법이 필요합니다.";
        }
        else if (rand < 0.5f)
        {
            data.type = HexTileType.Altar;
            data.eventTitle = "고대의 제단";
            data.eventDescription = "마나를 바치면 이동 AP 소모가 감소할지도 모릅니다.";
        }
        else if (rand < 0.55f)
        {
            data.type = HexTileType.Sight;
            data.eventTitle = "시야 관측소";
            data.eventDescription = "주변의 안개가 걷힙니다.";
        }
        else if (rand < 0.6f)
        {
            data.type = HexTileType.RandomPortal;
            data.eventTitle = "무작위 포탈";
            data.eventDescription = "어디로 갈지 모르는 포탈입니다.";
        }
        else if (rand < 0.65f)
        {
            data.type = HexTileType.Merchant;
            data.eventTitle = "방랑 상인";
            data.eventDescription = "소재를 물약으로 교환할 수 있습니다.";
        }
        else if (rand < 0.70f)
        {
            data.type = HexTileType.NPC;
            data.requiredSpellType = SpellType.Utility; // 회복
            data.eventTitle = "조난당한 NPC";
            data.eventDescription = "다친 모험가가 쓰러져 있습니다. 회복 마법이 필요합니다.";
            data.rewardAmount = 3;
        }
        else
        {
            data.type = HexTileType.Empty;
        }
        
        return data;
    }
}
