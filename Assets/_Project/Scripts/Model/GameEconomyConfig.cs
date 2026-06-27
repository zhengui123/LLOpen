using System;
using UnityEngine;

/// <summary>
/// 游戏经济数值配置，从 Resources/GameEconomyConfig.json 加载。
/// 使用嵌套对象字段（JsonUtility 不支持 JSON 数组）。
/// </summary>
[Serializable]
public class SellYieldMultiplierConfig
{
    public float empty = 0.2f;
    public float low = 0.6f;
    public float normal = 1f;
    public float high = 1.35f;
    public float perfect = 1.85f;
}

[Serializable]
public class VarietyBasePriceConfig
{
    public int jinZheng = 50;
    public int ganYao = 100;
    public int maoShanWang = 180;
}

[Serializable]
public class GameEconomyConfigData
{
    public int initialGold = 400;
    public int marketRefreshCost = 25;
    public SellYieldMultiplierConfig sellYieldMultipliers = new SellYieldMultiplierConfig();
    public VarietyBasePriceConfig varietyBasePrices = new VarietyBasePriceConfig();
}

public class GameEconomyConfig
{
    private readonly GameEconomyConfigData _data;

    public GameEconomyConfig(GameEconomyConfigData data)
    {
        _data = data ?? new GameEconomyConfigData();
    }

    public int InitialGold => _data.initialGold;
    public int MarketRefreshCost => _data.marketRefreshCost;

    public static GameEconomyConfig LoadDefault()
    {
        var asset = Resources.Load<TextAsset>("GameEconomyConfig");
        if (asset == null || string.IsNullOrWhiteSpace(asset.text))
        {
            Debug.LogWarning("[GameEconomyConfig] 未找到 Resources/GameEconomyConfig.json，使用内置默认值。");
            return new GameEconomyConfig(new GameEconomyConfigData());
        }

        try
        {
            var json = asset.text.Trim();
            if (!json.StartsWith("{", StringComparison.Ordinal))
            {
                Debug.LogError(
                    "[GameEconomyConfig] 加载到的不是 JSON（请确认 Resources 下无同名 .md 等非 JSON 资源）。");
                return new GameEconomyConfig(new GameEconomyConfigData());
            }

            var data = JsonUtility.FromJson<GameEconomyConfigData>(json);
            if (data == null)
            {
                Debug.LogWarning("[GameEconomyConfig] JSON 解析结果为空，使用内置默认值。");
                return new GameEconomyConfig(new GameEconomyConfigData());
            }

            return new GameEconomyConfig(data);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameEconomyConfig] JSON 解析失败: {ex.Message}，使用内置默认值。");
            return new GameEconomyConfig(new GameEconomyConfigData());
        }
    }

    public float GetSellMultiplier(YieldGrade grade)
    {
        var multipliers = _data.sellYieldMultipliers;
        if (multipliers == null)
        {
            return 1f;
        }

        return grade switch
        {
            YieldGrade.Empty => multipliers.empty,
            YieldGrade.Low => multipliers.low,
            YieldGrade.Normal => multipliers.normal,
            YieldGrade.High => multipliers.high,
            YieldGrade.Perfect => multipliers.perfect,
            _ => 1f
        };
    }

    public int GetVarietyBasePrice(VarietyType variety, int fallback)
    {
        var prices = _data.varietyBasePrices;
        if (prices == null)
        {
            return fallback;
        }

        return variety switch
        {
            VarietyType.JinZheng => prices.jinZheng,
            VarietyType.GanYao => prices.ganYao,
            VarietyType.MaoShanWang => prices.maoShanWang,
            _ => fallback
        };
    }
}
