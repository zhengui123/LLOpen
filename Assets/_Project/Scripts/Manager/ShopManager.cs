/// <summary>
/// 商店管理器（MVP 仅 2 级）：升级费用与售卖加成由 ShopConfig 驱动。
/// </summary>
public class ShopManager
{
    private static readonly int[] DefaultUpgradeCosts = { 0, 500, 1500 };
    private static readonly float[] DefaultSellBonuses = { 0f, 0.2f, 0.35f };

    private readonly ShopConfig _config;

    public int CurrentLevel { get; private set; } = 1;
    public int MaxLevel => 3;

    public ShopManager(ShopConfig config)
    {
        _config = config;
    }

    public bool CanUpgrade()
    {
        if (CurrentLevel >= MaxLevel)
        {
            return false;
        }

        var costs = GetUpgradeCosts();
        return PlayerData.Instance.Gold >= costs[CurrentLevel];
    }

    public void Upgrade()
    {
        if (!CanUpgrade())
        {
            return;
        }

        var costs = GetUpgradeCosts();
        PlayerData.Instance.Gold -= costs[CurrentLevel];
        CurrentLevel++;
        EventBus.Publish(new ShopUpgradedEvent { NewLevel = CurrentLevel });
    }

    public float GetSellBonus()
    {
        var bonuses = GetSellBonuses();
        var index = CurrentLevel - 1;
        if (index < 0 || index >= bonuses.Length)
        {
            return 0f;
        }

        return bonuses[index];
    }

    /// <summary>
    /// 下一级商店等级；已满级时返回当前等级。
    /// </summary>
    public int GetNextLevel()
    {
        return CurrentLevel >= MaxLevel ? CurrentLevel : CurrentLevel + 1;
    }

    /// <summary>
    /// 从当前等级升到下一级所需金币；已满级返回 0。
    /// </summary>
    public int GetUpgradeCost()
    {
        if (CurrentLevel >= MaxLevel)
        {
            return 0;
        }

        var costs = GetUpgradeCosts();
        return costs[CurrentLevel];
    }

    /// <summary>
    /// 升级后下一级的售卖加成（用于未满级时的效果预告）。
    /// </summary>
    public float GetNextLevelSellBonus()
    {
        if (CurrentLevel >= MaxLevel)
        {
            return GetSellBonus();
        }

        var bonuses = GetSellBonuses();
        var nextIndex = CurrentLevel;
        if (nextIndex < 0 || nextIndex >= bonuses.Length)
        {
            return 0f;
        }

        return bonuses[nextIndex];
    }

    private int[] GetUpgradeCosts()
    {
        return _config != null && _config.upgradeCosts != null && _config.upgradeCosts.Length > 0
            ? _config.upgradeCosts
            : DefaultUpgradeCosts;
    }

    private float[] GetSellBonuses()
    {
        return _config != null && _config.sellBonuses != null && _config.sellBonuses.Length > 0
            ? _config.sellBonuses
            : DefaultSellBonuses;
    }
}
