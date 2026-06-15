/// <summary>
/// 商店管理器（MVP 仅 2 级）：升级费用与售卖加成由 ShopConfig 驱动。
/// </summary>
public class ShopManager
{
    private static readonly int[] DefaultUpgradeCosts = { 0, 500 };
    private static readonly float[] DefaultSellBonuses = { 0f, 0.2f };

    private readonly ShopConfig _config;

    public int CurrentLevel { get; private set; } = 1;
    public int MaxLevel => 2;

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
