public enum HexTileType
{
    Empty,
    NormalMonster,
    Boss,
    Trap,
    Obstacle,
    Altar,
    Sight,
    RandomPortal,
    Resource,
    Merchant,
    NPC,
    Exit,
    Start
}

[System.Serializable]
public struct HexTileEventData
{
    public HexTileType type;
    public System.Collections.Generic.List<SpellElement> requiredElements;
    public SpellType requiredSpellType;
    public string eventTitle;
    public string eventDescription;
    public int rewardAmount;
}

