using UnityEngine;

/// <summary>
/// 售卖管理器（MVP 固定价回收 + 商店/广告加成）。
/// </summary>
public class SellManager
{
    private readonly ShopManager _shopManager;
    private float _temporaryAdBonus;

    public SellManager(ShopManager shopManager)
    {
        _shopManager = shopManager;
    }

    public int CalculateSellPrice(DurianData durian)
    {
        var basePrice = GetYieldGradePrice(durian.yieldGrade);
        var shopBonus = _shopManager.GetSellBonus();
        var totalBonus = shopBonus + _temporaryAdBonus;
        return Mathf.RoundToInt(basePrice * (1f + totalBonus));
    }

    public void SellDurian(DurianData durian)
    {
        var price = CalculateSellPrice(durian);
        _temporaryAdBonus = 0f;

        PlayerData.Instance.Gold += price;
        EventBus.Publish(new DurianSoldEvent { Durian = durian, Price = price });
    }

    public void ApplyAdBonus()
    {
        _temporaryAdBonus = 0.2f;
    }

    private static float GetYieldGradePrice(YieldGrade grade)
    {
        return grade switch
        {
            YieldGrade.Empty => 0f,
            YieldGrade.Low => 10f,
            YieldGrade.Normal => 50f,
            YieldGrade.High => 100f,
            YieldGrade.Perfect => 250f,
            _ => 0f
        };
    }
}
