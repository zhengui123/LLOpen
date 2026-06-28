using UnityEngine;

/// <summary>
/// 售卖管理器（MVP 固定价回收 + 商店/广告加成）。
/// </summary>
public class SellManager
{
    private readonly ShopManager _shopManager;
    private readonly GameEconomyConfig _economyConfig;
    private float _temporaryAdBonus;

    public SellManager(ShopManager shopManager, GameEconomyConfig economyConfig)
    {
        _shopManager = shopManager;
        _economyConfig = economyConfig;
    }

    public int CalculateSellPrice(DurianData durian)
    {
        var purchaseMultiplier = _economyConfig.GetSellMultiplier(durian.yieldGrade);
        var basePrice = Mathf.RoundToInt(durian.finalPrice * purchaseMultiplier);
        var shopBonus = _shopManager.GetSellBonus();
        var totalBonus = shopBonus + _temporaryAdBonus;
        return Mathf.RoundToInt(basePrice * (1f + totalBonus));
    }

    public void SellDurian(DurianData durian)
    {
        SellAtPrice(durian, CalculateSellPrice(durian));
    }

    public void SellAtPrice(DurianData durian, int price)
    {
        _temporaryAdBonus = 0f;
        PlayerData.Instance.Gold += price;
        EventBus.Publish(new DurianSoldEvent { Durian = durian, Price = price });
    }

    public void ApplyAdBonus()
    {
        _temporaryAdBonus = 0.2f;
    }

    /// <summary>
    /// 清除广告临时加成（离开售卖页未卖出时调用）。
    /// </summary>
    public void ClearTemporaryBonus()
    {
        _temporaryAdBonus = 0f;
    }
}
