using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

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

    [Header("Peek Settings")]
    public int peekAPCost = 1;
    private Vector3Int? peekedTilePos;
    private AnimatedTile prePeekTile;

    [Header("Input & Camera Settings")]
    public RectTransform mapRenderRect; 
    public Camera dungeonCamera;
    
    private Vector3 dragStartCameraPos;
    private Vector3 dragStartMousePos;
    private bool isDraggingMap = false;
    private float dragAccumulatedDistance = 0f;

    public bool IsEventActive { get; private set; }

    private void Awake()
    {
        Instance = this;
        EnterDungeon();
    }

    private Camera GetUICamera()
    {
        if (mapRenderRect == null) return null;
        Canvas canvas = mapRenderRect.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            return canvas.worldCamera;
        }
        return null;
    }

    private void Update()
    {
        // 1. 마우스 우클릭을 떼면 무조건 Peek 상태 해제 (UI 밖으로 마우스가 나가더라도 처리)
        if (Input.GetMouseButtonUp(1))
        {
            if (peekedTilePos.HasValue)
            {
                RevertPeek();
            }
        }

        // 이벤트 팝업이 떠있는 등 이벤트 진행 중일 때는 맵 상호작용(이동, Peek 등) 차단
        if (IsEventActive) return;

        bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        bool isPointerInRenderRect = false;
        
        Camera uiCamera = GetUICamera();

        if (mapRenderRect != null)
        {
            isPointerInRenderRect = RectTransformUtility.RectangleContainsScreenPoint(mapRenderRect, Input.mousePosition, uiCamera);
        }

        // 다른 UI 위에 마우스가 있는데 렌더 영역이 아니면 무시
        if (isPointerOverUI && mapRenderRect != null && !isPointerInRenderRect)
            return;
        if (isPointerOverUI && mapRenderRect == null)
            return;

        // 드래그 시작 (좌클릭)
        if (Input.GetMouseButtonDown(0))
        {
            if (mapRenderRect == null || isPointerInRenderRect)
            {
                isDraggingMap = true;
                dragStartMousePos = Input.mousePosition;
                dragAccumulatedDistance = 0f;
                
                Camera targetCam = dungeonCamera != null ? dungeonCamera : Camera.main;
                if (targetCam != null)
                    dragStartCameraPos = targetCam.transform.position;
            }
        }
        
        // 드래그 중
        if (Input.GetMouseButton(0) && isDraggingMap)
        {
            Camera targetCam = dungeonCamera != null ? dungeonCamera : Camera.main;
            if (targetCam != null)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                
                dragAccumulatedDistance += Mathf.Abs(mouseX) + Mathf.Abs(mouseY);
                
                // 드래그가 일정 수준 이상 진행되면 커서를 숨기고 잠금
                if (dragAccumulatedDistance >= 1.0f && Cursor.lockState != CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                
                float screenHeight = mapRenderRect != null ? mapRenderRect.rect.height : Screen.height;
                float screenToWorldRatio = (targetCam.orthographicSize * 2f) / screenHeight;
                
                // Input.GetAxis는 해상도에 직접 비례하지 않으므로 적절한 배율(예: 50f)을 곱해줍니다. 
                // 스크롤 감도가 너무 빠르거나 느리면 이 값을 조절하세요.
                float dragSensitivity = 50f; 
                Vector3 delta = new Vector3(-mouseX, -mouseY, 0) * screenToWorldRatio * dragSensitivity;
                
                targetCam.transform.position += delta;
            }
        }

        // 클릭(이동) 처리
        if (Input.GetMouseButtonUp(0) && isDraggingMap)
        {
            isDraggingMap = false;
            
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            
            // 마우스가 잠겨있을 때는 화면 상의 좌표 이동이 없으므로 누적 이동량을 기준으로 클릭 여부를 판별
            if (dragAccumulatedDistance < 1.0f)
            {
                ProcessLeftClick(dragStartMousePos); // 잠금이 풀리면서 위치가 변할 수 있으므로 처음 클릭한 위치 사용
            }
            else
            {
                // 드래그가 끝났어도 혹시 Peek 중이면 해제 (선택사항)
                if (peekedTilePos.HasValue) RevertPeek();
            }
        }

        // 우클릭 (Peek) 처리
        if (Input.GetMouseButtonDown(1))
        {
            if (mapRenderRect == null || isPointerInRenderRect)
            {
                ProcessRightClick(Input.mousePosition);
            }
        }
    }

    private bool GetTargetCell(Vector2 screenPos, out Vector3Int cellPos)
    {
        cellPos = Vector3Int.zero;
        Vector3 worldPos = Vector3.zero;

        if (mapRenderRect != null && dungeonCamera != null)
        {
            Camera uiCamera = GetUICamera();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(mapRenderRect, screenPos, uiCamera, out Vector2 localPoint))
            {
                Rect rect = mapRenderRect.rect;
                float normalizedX = (localPoint.x - rect.xMin) / rect.width;
                float normalizedY = (localPoint.y - rect.yMin) / rect.height;

                if (normalizedX >= 0 && normalizedX <= 1 && normalizedY >= 0 && normalizedY <= 1)
                {
                    Vector3 viewportPos = new Vector3(normalizedX, normalizedY, Mathf.Abs(dungeonCamera.transform.position.z));
                    worldPos = dungeonCamera.ViewportToWorldPoint(viewportPos);
                }
                else return false;
            }
            else return false;
        }
        else
        {
            Camera targetCam = dungeonCamera != null ? dungeonCamera : Camera.main;
            if (targetCam != null)
            {
                worldPos = targetCam.ScreenToWorldPoint(screenPos);
            }
            else return false;
        }

        worldPos.z = 0;
        if (tileManager.dungeonTilemap != null)
        {
            cellPos = tileManager.dungeonTilemap.WorldToCell(worldPos);
            cellPos.z = 0;
            return true;
        }
        
        return false;
    }

    private void ProcessLeftClick(Vector2 screenPos)
    {
        if (peekedTilePos.HasValue)
        {
            RevertPeek();
        }

        if (GetTargetCell(screenPos, out Vector3Int cellPos))
        {
            if (tileMap.ContainsKey(cellPos))
            {
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
    }

    private void ProcessRightClick(Vector2 screenPos)
    {
        if (GetTargetCell(screenPos, out Vector3Int cellPos))
        {
            if (peekedTilePos.HasValue)
            {
                RevertPeek();
            }

            if (tileMap.ContainsKey(cellPos))
            {
                PeekTile(cellPos);
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
            tileManager.dungeonTilemap.SetAnimationFrame(currentPos, 0);
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
            
            CheckTileEvent(targetPos);
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
            if (tileMap.TryGetValue(nextPos, out HexTileData tile))
            {
                if (!tile.IsEventCleared && tile.Type != HexTileType.Empty && tile.Type != HexTileType.Start)
                {
                    break;
                }
            }

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
                    IsEventActive = true;
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
        IsEventActive = false;
        
        // 어떤 방식으로든(성공이든 맨몸 돌파로 인한 실패든) 이벤트가 처리되었으므로 타일에서 이벤트를 지웁니다.
        // tile.ClearEvent();
        
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
        RevealSight(currentPos, 0);
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

    public void PeekTile(Vector3Int targetPos)
    {
        if (currentAP < peekAPCost)
        {
            Debug.LogWarning("<color=yellow>[Peek] AP가 부족합니다.</color>");
            return;
        }

        if (HexPathfinding.GetNeighbors(currentPos).Contains(targetPos))
        {
            if (tileMap.TryGetValue(targetPos, out HexTileData tile))
            {
                if (tile.IsDiscovered)
                {
                    Debug.Log("<color=yellow>[Peek] 이미 밝혀진 타일은 Peek할 수 없습니다.</color>");
                    return;
                }

                // 이동 중이면 멈춤
                if (moveCoroutine != null)
                {
                    StopCoroutine(moveCoroutine);
                    moveCoroutine = null;
                    ClearPathTiles();
                    Debug.Log("<color=yellow>[Peek] 이동을 멈추고 탐색합니다.</color>");
                }

                // AP 소모
                // currentAP -= peekAPCost;
                OnAPChanged?.Invoke(currentAP);

                prePeekTile = tileManager.dungeonTilemap.GetTile<UnityEngine.Tilemaps.AnimatedTile>(targetPos);
                peekedTilePos = targetPos;

                Debug.Log($"[Peek] Start Peek at {targetPos}. prePeekTile is {(prePeekTile == null ? "NULL" : prePeekTile.name)}");

                AnimatedTile revealedTile = tileManager.GetRevealedTile(tile);
                tileManager.FlipToTile(targetPos, revealedTile);
            }
        }
        else
        {
            Debug.LogWarning("[Peek] 인접한 1칸 거리의 타일만 Peek할 수 있습니다.");
        }
    }

    private void RevertPeek()
    {
        Debug.Log($"[Peek] RevertPeek called. peekedTilePos.HasValue: {peekedTilePos.HasValue}, prePeekTile: {(prePeekTile == null ? "NULL" : prePeekTile.name)}");
        
        if (peekedTilePos.HasValue && prePeekTile != null)
        {
            Vector3Int pos = peekedTilePos.Value;
            tileManager.FlipToTile(pos, prePeekTile);
            
            peekedTilePos = null;
            prePeekTile = null;
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
