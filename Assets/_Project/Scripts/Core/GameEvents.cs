using System.Collections.Generic;

// 市场相关
public struct MarketRefreshedEvent
{
    public DurianData[] Durians;
}

public struct DurianPurchasedEvent
{
    public DurianData Durian;
}

// 背包相关
public struct BagUpdatedEvent
{
    public List<DurianData> Durians;
}

public struct BagFullEvent
{
}

// 售卖相关
public struct DurianSoldEvent
{
    public DurianData Durian;
    public int Price;
}

// 商店相关
public struct ShopUpgradedEvent
{
    public int NewLevel;
}

// 广告相关
public struct AdRewardEvent
{
    public AdRewardType Type;
    public float Value;
}

// 开榴莲相关
public struct DurianOpenedEvent
{
    public DurianData Durian;
    public string Rating;
    public float YieldRate;
}

// v1.5：连击 / 图鉴 / 中途卖 / 每日目标
public struct StreakUpdatedEvent
{
    public int Combo;
}

public struct CollectionNewEntryEvent
{
    public VarietyType Variety;
    public YieldGrade Grade;
}

public struct DurianMidwaySoldEvent
{
    public DurianData Durian;
    public int EstimatePrice;
}

public struct DailyTargetCompletedEvent
{
}
