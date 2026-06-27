using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 广告管理器（MVP Mock：UniTask 模拟 2 秒激励视频）。
/// </summary>
public class AdManager
{
    private int _dailyAdCount;
    private readonly int _maxDailyAds = 10;
    private readonly Dictionary<string, int> _typeCounts = new();

    private readonly Dictionary<string, int> _adLimits = new()
    {
        { "customer_bonus", 5 },
        { "sell_bonus", 5 },
        { "sel_bonus", 5 },
        { "revive", 3 },
        { "daily_buff", 1 },
        { "free_smell", -1 },
        { "refresh_market", -1 }
    };

    public bool CanShowAd(string adType)
    {
        if (_dailyAdCount >= _maxDailyAds)
        {
            return false;
        }

        var normalizedType = NormalizeAdType(adType);
        if (_adLimits.TryGetValue(normalizedType, out var limit) && limit != -1)
        {
            return GetTypeCount(normalizedType) < limit;
        }

        return true;
    }

    public async UniTask<bool> ShowRewardedAd(string adType)
    {
        var normalizedType = NormalizeAdType(adType);
        if (!CanShowAd(normalizedType))
        {
            return false;
        }

        // MVP Mock：模拟微信激励视频播放 2 秒
        await UniTask.Delay(2000);
        var success = true;

        if (success)
        {
            _dailyAdCount++;
            PlayerData.Instance.DailyAdCount = _dailyAdCount;
            IncrementTypeCount(normalizedType);
            GrantReward(normalizedType);
        }

        return success;
    }

    private void GrantReward(string adType)
    {
        switch (adType)
        {
            case "customer_bonus":
            case "sell_bonus":
            case "sel_bonus":
                EventBus.Publish(new AdRewardEvent
                {
                    Type = AdRewardType.SellBonus,
                    Value = 0.2f
                });
                break;
            case "revive":
                EventBus.Publish(new AdRewardEvent
                {
                    Type = AdRewardType.Revive,
                    Value = 0f
                });
                break;
            case "daily_buff":
                var buffValue = Random.Range(0f, 0.05f);
                PlayerData.Instance.DailyBuff += buffValue;
                EventBus.Publish(new AdRewardEvent
                {
                    Type = AdRewardType.DailyBuff,
                    Value = buffValue
                });
                break;
            case "free_smell":
                EventBus.Publish(new AdRewardEvent
                {
                    Type = AdRewardType.Clue,
                    Value = 0f
                });
                break;
        }
    }

    private static string NormalizeAdType(string adType)
    {
        return adType switch
        {
            "sell_bonus" or "sel_bonus" => "customer_bonus",
            _ => adType
        };
    }

    private int GetTypeCount(string adType)
    {
        return _typeCounts.TryGetValue(adType, out var count) ? count : 0;
    }

    private void IncrementTypeCount(string adType)
    {
        _typeCounts.TryGetValue(adType, out var count);
        _typeCounts[adType] = count + 1;
    }
}
