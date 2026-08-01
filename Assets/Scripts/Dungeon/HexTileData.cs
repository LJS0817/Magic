using UnityEngine;

public class HexTileData
{
    public Vector3Int position;
    
    public HexTileType Type { get; private set; }
    public HexTileEventData EventData { get; private set; }
    public bool IsDiscovered { get; private set; }
    public bool IsEventCleared { get; private set; }
    public bool isTrapRevealed { get; private set; }

    public HexTileData(Vector3Int pos, HexTileEventData eventData)
    {
        this.position = pos;
        this.EventData = eventData;
        this.Type = eventData.type;
        this.IsDiscovered = false;
        
        // 시작 지점이나 빈 공간 등은 처음부터 클리어된 것으로 간주
        this.IsEventCleared = (Type == HexTileType.Start || Type == HexTileType.Empty || Type == HexTileType.Exit);
    }

    public void Discover()
    {
        IsDiscovered = true;
    }

    public void ClearEvent()
    {
        IsEventCleared = true;
    }
    
    public void RevealTrap()
    {
        isTrapRevealed = true;
    }
}
