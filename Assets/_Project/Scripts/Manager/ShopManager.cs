/// <summary>
/// 商店管理器（v1.5 3 级）：升级费用与售卖加成由 ShopConfig 驱动，不足时回退默认值。
/// </summary>
public class ShopManager
{
    private static readonly int[] DefaultUpgradeCosts = { 0, 500, 1500 };
    private static readonly float[] DefaultSellBonuses = { 0f, 0.2f, 0.35f };

    private readonly ShopConfig _config;

    public int CurrentLevel { get; private set; } = 1;
    public int MaxLevel => DefaultUpgradeCosts.Length;

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

        return PlayerData.Instance.Gold >= GetUpgradeCost();
    }

    public void Upgrade()
    {
        if (!CanUpgrade())
        {
            return;
        }

        PlayerData.Instance.Gold -= GetUpgradeCost();
        CurrentLevel++;
        EventBus.Publish(new ShopUpgradedEvent { NewLevel = CurrentLevel });
    }

    public float GetSellBonus()
    {
        var index = CurrentLevel - 1;
        return GetSellBonusAtIndex(index);
    }

    /// <summary>下一级商店等级；已满级时返回当前等级。</summary>
    public int GetNextLevel()
    {
        return CurrentLevel >= MaxLevel ? CurrentLevel : CurrentLevel + 1;
    }

    /// <summary>从当前等级升到下一级所需金币；已满级返回 0。</summary>
    public int GetUpgradeCost()
    {
        if (CurrentLevel >= MaxLevel)
        {
            return 0;
        }

        return GetUpgradeCostAtIndex(CurrentLevel);
    }

    /// <summary>升级后下一级的售卖加成（用于未满级时的效果预告）。</summary>
    public float GetNextLevelSellBonus()
    {
        if (CurrentLevel >= MaxLevel)
        {
            return GetSellBonus();
        }

        return GetSellBonusAtIndex(CurrentLevel);
    }

    private int GetUpgradeCostAtIndex(int levelIndex)
    {
        var costs = GetUpgradeCosts();
        if (levelIndex >= 0 && levelIndex < costs.Length)
        {
            return costs[levelIndex];
        }

        return 0;
    }

    private float GetSellBonusAtIndex(int bonusIndex)
    {
        var bonuses = GetSellBonuses();
        if (bonusIndex >= 0 && bonusIndex < bonuses.Length)
        {
            return bonuses[bonusIndex];
        }

        return 0f;
    }

    private int[] GetUpgradeCosts()
    {
        return MergeIntArray(DefaultUpgradeCosts, _config?.upgradeCosts);
    }

    private float[] GetSellBonuses()
    {
        return MergeFloatArray(DefaultSellBonuses, _config?.sellBonuses);
    }

    private static int[] MergeIntArray(int[] defaults, int[] overrides)
    {
        var result = new int[defaults.Length];
        for (var i = 0; i < defaults.Length; i++)
        {
            result[i] = defaults[i];
        }

        if (overrides == null)
        {
            return result;
        }

        for (var i = 0; i < overrides.Length && i < result.Length; i++)
        {
            result[i] = overrides[i];
        }

        return result;
    }

    private static float[] MergeFloatArray(float[] defaults, float[] overrides)
    {
        var result = new float[defaults.Length];
        for (var i = 0; i < defaults.Length; i++)
        {
            result[i] = defaults[i];
        }

        if (overrides == null)
        {
            return result;
        }

        for (var i = 0; i < overrides.Length && i < result.Length; i++)
        {
            result[i] = overrides[i];
        }

        return result;
    }
}
