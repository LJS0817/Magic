using System.Collections.Generic;
using UnityEngine;

public class HexPathfinding
{
    public class PathNode
    {
        public Vector3Int position;
        public PathNode parent;
        public int gCost;
        public int hCost;
        public int fCost { get { return gCost + hCost; } }

        public PathNode(Vector3Int position)
        {
            this.position = position;
        }
    }

    public static List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int targetPos, Dictionary<Vector3Int, HexTileData> tileMap)
    {
        PathNode startNode = new PathNode(startPos);
        PathNode targetNode = new PathNode(targetPos);

        List<PathNode> openList = new List<PathNode> { startNode };
        HashSet<Vector3Int> closedList = new HashSet<Vector3Int>();

        Dictionary<Vector3Int, PathNode> allNodes = new Dictionary<Vector3Int, PathNode>();
        allNodes.Add(startPos, startNode);

        while (openList.Count > 0)
        {
            PathNode currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost || (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode.position);

            if (currentNode.position == targetPos)
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (Vector3Int neighborPos in GetNeighbors(currentNode.position))
            {
                if (!tileMap.ContainsKey(neighborPos) || closedList.Contains(neighborPos))
                {
                    continue;
                }

                // 이동 불가능한 타일 조건이 있다면 여기에 추가
                // 예: if (tileMap[neighborPos].Type == HexTileType.Obstacle) continue;

                int newMovementCostToNeighbor = currentNode.gCost + 1; // 이동 비용은 1로 가정 (파이썬 코드 참고)

                PathNode neighborNode;
                if (!allNodes.ContainsKey(neighborPos))
                {
                    neighborNode = new PathNode(neighborPos);
                    allNodes.Add(neighborPos, neighborNode);
                }
                else
                {
                    neighborNode = allNodes[neighborPos];
                }

                if (newMovementCostToNeighbor < neighborNode.gCost || !openList.Contains(neighborNode))
                {
                    neighborNode.gCost = newMovementCostToNeighbor;
                    neighborNode.hCost = GetHeuristic(neighborPos, targetPos);
                    neighborNode.parent = currentNode;

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }

        return null; // 경로 없음
    }

    private static List<Vector3Int> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        PathNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    public static List<Vector3Int> GetNeighbors(Vector3Int pos)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>();
        int x = pos.x;
        int y = pos.y;
        
        // Flat Top 육각형 타일맵(Odd-R Offset) 특성에 맞게 이웃 계산
        // y(행)가 홀수일 때와 짝수일 때 x의 연결 방향이 다름
        int skipXIndex = (Mathf.Abs(y) % 2 == 1) ? -1 : 1;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if ((i == 0 && j == 0) || (j == skipXIndex && i != 0)) 
                    continue;

                neighbors.Add(new Vector3Int(x + j, y + i, 0));
            }
        }
        
        return neighbors;
    }

    public static int GetHeuristic(Vector3Int a, Vector3Int b)
    {
        // Odd-R Offset 좌표계를 Axial 좌표계로 변환하여 정확한 육각형 거리를 계산
        Vector3Int axialA = OffsetToAxial(a);
        Vector3Int axialB = OffsetToAxial(b);
        return (Mathf.Abs(axialA.x - axialB.x) 
              + Mathf.Abs(axialA.x + axialA.y - axialB.x - axialB.y) 
              + Mathf.Abs(axialA.y - axialB.y)) / 2;
    }

    private static Vector3Int OffsetToAxial(Vector3Int offset)
    {
        int q = offset.x - (offset.y - (Mathf.Abs(offset.y) % 2)) / 2;
        int r = offset.y;
        return new Vector3Int(q, r, 0);
    }
}
