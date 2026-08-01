using System.Collections.Generic;
using UnityEngine;

public class DungeonMapGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject hexTilePrefab;
    
    [Header("Grid Settings")]
    public int radius = 3;
    public float hexSize = 1f;

    private Dictionary<Vector2Int, HexTile> tileMap = new Dictionary<Vector2Int, HexTile>();

    public Dictionary<Vector2Int, HexTile> GenerateMap(Transform parent)
    {
        // 기존 타일 삭제
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
        tileMap.Clear();

        // 헥사곤 그리드 생성 (육각 형태)
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                Vector3 worldPos = HexToWorld(q, r);
                GameObject tileObj = Instantiate(hexTilePrefab, worldPos, Quaternion.identity, parent);
                tileObj.name = $"Hex_{q}_{r}";
                
                HexTile tile = tileObj.GetComponent<HexTile>();
                
                // 랜덤 타입 부여 (시작 지점은 0,0)
                HexTileEventData randomData = GetRandomEventData(q, r);
                tile.Init(q, r, randomData);
                
                tileMap.Add(new Vector2Int(q, r), tile);
            }
        }
        
        return tileMap;
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

    private Vector3 HexToWorld(int q, int r)
    {
        float x = hexSize * Mathf.Sqrt(3) * (q + r / 2f);
        float y = hexSize * 3f / 2f * r;
        return new Vector3(x, y, 0); // 2D 환경 기준
    }
}
