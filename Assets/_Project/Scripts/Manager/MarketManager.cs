using UnityEngine;

/// <summary>
/// 市场管理器：刷新榴莲、购买、查询外观价格系数。
/// </summary>
public class MarketManager
{
    private readonly DurianGeneratorSystem _generator;

    public DurianData[] CurrentMarketDurians { get; private set; }

    public MarketManager(DurianGeneratorSystem generator)
    {
        _generator = generator;
    }

    public void RefreshMarket(VarietyType variety)
    {
        CurrentMarketDurians = _generator.GenerateMarketDurians(variety);
        EventBus.Publish(new MarketRefreshedEvent { Durians = CurrentMarketDurians });
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
