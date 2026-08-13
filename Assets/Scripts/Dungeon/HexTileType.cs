public enum HexTileType
{
    Empty,
    NormalMonster,
    Boss,
    Trap,
    Obstacle,
    RandomPortal,
    TreasureBox,
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

