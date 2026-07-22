namespace Magic.Upgrade
{
    public enum UpgradeType
    {
        MaxHealth,
        MaxMana,
        InventoryCapacity,
        SellPriceMultiplier,
        HealthRegeneration,
        ManaRegeneration,
        SpellDamageMultiplier,
        ShopDiscount,           // 구매 가격 낮춤 (최대 20%)
        ElementDamageBonus,     // 특정 원소 추가 대미지 (최대 30%)
        ShapeSellBonus,         // 특정 도형 스펠 판매가 증가
        ShapeDamageBonus,       // 특정 도형 스펠 대미지 증가 (최대 15%)
        DropRateBonus,          // 아이템 드롭 확률 증가
        FreeInkChance,          // 잉크 소모 없음 확률
        CombatLoadoutCapacity   // 전투 로드아웃 칸 수 증가
    }
}
