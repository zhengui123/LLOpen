using System.Linq;
using UnityEngine;

/// <summary>
/// 榴莲生成系统：按配置生成市场展示的 3 颗同品种榴莲。
/// </summary>
public class DurianGeneratorSystem
{
    private static int _nextId = 1;

    private readonly AppearanceProbabilitySystem _probabilitySystem;
    private readonly GameEconomyConfig _economyConfig;
    private readonly VarietyConfig[] _varietyConfigs;
    private readonly AppearanceConfig[] _appearanceConfigs;

    public DurianGeneratorSystem(
        AppearanceProbabilitySystem probabilitySystem,
        GameEconomyConfig economyConfig,
        VarietyConfig[] varietyConfigs,
        AppearanceConfig[] appearanceConfigs)
    {
        _probabilitySystem = probabilitySystem;
        _economyConfig = economyConfig;
        _varietyConfigs = varietyConfigs;
        _appearanceConfigs = appearanceConfigs;
    }

    public DurianData[] GenerateMarketDurians(VarietyType variety)
    {
        var varietyConfig = GetVarietyConfig(variety);
        var durians = new DurianData[3];

        for (var i = 0; i < durians.Length; i++)
        {
            var appearance = RandomAppearance();
            var appearanceConfig = GetAppearanceConfig(appearance);
            var probabilities = _probabilitySystem.CalculateFinalProbabilities(
                varietyConfig.baseWeights,
                appearanceConfig.probabilityOffsets);

            var grade = _probabilitySystem.SampleYieldGrade(probabilities);
            var yieldRate = GenerateYieldRate(grade);
            var roomCount = Random.Range(varietyConfig.minRooms, varietyConfig.maxRooms + 1);
            var priceMultiplier = appearanceConfig.priceMultiplier;
            var basePrice = _economyConfig.GetVarietyBasePrice(variety, varietyConfig.basePrice);

            durians[i] = new DurianData
            {
                id = _nextId++,
                variety = variety,
                appearance = appearance,
                appearancePriceMultiplier = priceMultiplier,
                basePrice = basePrice,
                finalPrice = Mathf.RoundToInt(basePrice * priceMultiplier),
                yieldRate = yieldRate,
                roomCount = roomCount,
                roomResults = GenerateRoomResults(roomCount, yieldRate),
                yieldGrade = grade
            };
        }

        return durians;
    }

    /// <summary>
    /// 复活时重新随机出肉率与房位，保留 id、品种、外观与购买价格。
    /// </summary>
    public DurianData RerollOpenResult(DurianData durian)
    {
        var varietyConfig = GetVarietyConfig(durian.variety);
        var appearanceConfig = GetAppearanceConfig(durian.appearance);
        var probabilities = _probabilitySystem.CalculateFinalProbabilities(
            varietyConfig.baseWeights,
            appearanceConfig.probabilityOffsets);

        var grade = _probabilitySystem.SampleYieldGrade(probabilities);
        var yieldRate = GenerateYieldRate(grade);

        durian.yieldGrade = grade;
        durian.yieldRate = yieldRate;
        durian.roomResults = GenerateRoomResults(durian.roomCount, yieldRate);
        return durian;
    }

    private AppearanceType RandomAppearance()
    {
        var totalWeight = _appearanceConfigs.Sum(c => c.spawnWeight);
        var roll = Random.Range(0f, totalWeight);
        var cumulative = 0f;

        foreach (var config in _appearanceConfigs)
        {
            cumulative += config.spawnWeight;
            if (roll <= cumulative)
            {
                return config.type;
            }
        }

        return AppearanceType.Normal;
    }

    private float GenerateYieldRate(YieldGrade grade)
    {
        float min;
        float max;

        switch (grade)
        {
            case YieldGrade.Empty:
                min = 0f;
                max = 15f;
                break;
            case YieldGrade.Low:
                min = 15f;
                max = 30f;
                break;
            case YieldGrade.High:
                min = 45f;
                max = 60f;
                break;
            case YieldGrade.Perfect:
                min = 60f;
                max = 85f;
                break;
            default:
                min = 30f;
                max = 45f;
                break;
        }

        var rate = Random.Range(min, max);
        rate += Random.Range(-5f, 5f);
        return Mathf.Clamp(rate, 0f, 100f);
    }

    private static bool[] GenerateRoomResults(int roomCount, float yieldRate)
    {
        var rooms = new bool[roomCount];
        var meatRooms = Mathf.Clamp(Mathf.RoundToInt(roomCount * yieldRate / 100f), 0, roomCount);

        for (var i = 0; i < roomCount; i++)
        {
            rooms[i] = i < meatRooms;
        }

        // 打乱有房/空房分布，避免总是前几房有肉
        for (var i = roomCount - 1; i > 0; i--)
        {
            var swapIndex = Random.Range(0, i + 1);
            (rooms[i], rooms[swapIndex]) = (rooms[swapIndex], rooms[i]);
        }

        return rooms;
    }

    private VarietyConfig GetVarietyConfig(VarietyType variety)
    {
        return _varietyConfigs.FirstOrDefault(c => c.type == variety)
               ?? _varietyConfigs.FirstOrDefault();
    }

    private AppearanceConfig GetAppearanceConfig(AppearanceType appearance)
    {
        return _appearanceConfigs.FirstOrDefault(c => c.type == appearance)
               ?? _appearanceConfigs.FirstOrDefault();
    }
}
