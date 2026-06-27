using UnityEngine;

/// <summary>
/// 市场管理器：刷新榴莲、购买、换一批。
/// </summary>
public class MarketManager
{
    private readonly DurianGeneratorSystem _generator;
    private readonly GameEconomyConfig _economyConfig;

    public DurianData[] CurrentMarketDurians { get; private set; }
    public VarietyType CurrentVariety { get; private set; } = VarietyType.JinZheng;

    public MarketManager(DurianGeneratorSystem generator, GameEconomyConfig economyConfig)
    {
        _generator = generator;
        _economyConfig = economyConfig;
    }

    public void RefreshMarket(VarietyType variety)
    {
        CurrentVariety = variety;
        CurrentMarketDurians = _generator.GenerateMarketDurians(variety);
        EventBus.Publish(new MarketRefreshedEvent { Durians = CurrentMarketDurians });
    }

    public bool TryRefreshWithGold()
    {
        var cost = _economyConfig.MarketRefreshCost;
        if (PlayerData.Instance.Gold < cost)
        {
            return false;
        }

        PlayerData.Instance.Gold -= cost;
        RefreshMarket(CurrentVariety);
        return true;
    }

    public void BuyDurian(int index)
    {
        if (CurrentMarketDurians == null
            || index < 0
            || index >= CurrentMarketDurians.Length)
        {
            return;
        }

        var purchased = CurrentMarketDurians[index];
        var cost = purchased.finalPrice;

        if (PlayerData.Instance.Gold < cost)
        {
            return;
        }

        PlayerData.Instance.Gold -= cost;
        EventBus.Publish(new DurianPurchasedEvent { Durian = purchased });
    }

    public float GetPriceMultiplier(AppearanceType appearance)
    {
        return appearance switch
        {
            AppearanceType.Poor => 0.8f,
            AppearanceType.Good => 1.5f,
            AppearanceType.Premium => 2.2f,
            _ => 1.0f
        };
    }
}
