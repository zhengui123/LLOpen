using System;
using UnityEngine;

/// <summary>
/// v1.5 每日目标：当日卖出累计达 500 金可领 50 金奖励。
/// </summary>
public class DailyTarget
{
    public const int TargetGold = 500;
    public const int Reward = 50;

    private const string SaveKey = "llopen_daily_target_v1";

    public int EarnedToday { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool IsClaimed { get; private set; }

    /// <summary>按 yyyy-MM-dd 跨天清零当日进度。</summary>
    public void CheckDailyReset()
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");

        if (!PlayerPrefs.HasKey(SaveKey))
        {
            ResetForNewDay(today);
            return;
        }

        var json = PlayerPrefs.GetString(SaveKey);
        if (string.IsNullOrEmpty(json))
        {
            ResetForNewDay(today);
            return;
        }

        try
        {
            var data = JsonUtility.FromJson<DailyTargetSaveData>(json);
            if (data == null || data.date != today)
            {
                ResetForNewDay(today);
                return;
            }

            EarnedToday = data.earnedToday;
            IsCompleted = data.isCompleted;
            IsClaimed = data.isClaimed;
            PublishUpdated();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DailyTarget] 读档失败，已重置：{e.Message}");
            ResetForNewDay(today);
        }
    }

    /// <summary>卖出金币计入今日目标与总收益统计。</summary>
    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        EarnedToday += amount;
        PlayerProgression.Instance.TotalGoldEarned += amount;
        PlayerProgression.Instance.Save();

        if (!IsCompleted && EarnedToday >= TargetGold)
        {
            IsCompleted = true;
            EventBus.Publish(new DailyTargetCompletedEvent());
        }

        Save();
        PublishUpdated();
    }

    /// <summary>领取每日奖励；成功返回 true。</summary>
    public bool ClaimReward()
    {
        if (!IsCompleted || IsClaimed)
        {
            return false;
        }

        IsClaimed = true;
        PlayerData.Instance.Gold += Reward;
        Save();
        PublishUpdated();
        return true;
    }

    private void PublishUpdated()
    {
        EventBus.Publish(new DailyTargetUpdatedEvent
        {
            EarnedToday = EarnedToday,
            IsCompleted = IsCompleted,
            IsClaimed = IsClaimed
        });
    }

    private void ResetForNewDay(string today)
    {
        EarnedToday = 0;
        IsCompleted = false;
        IsClaimed = false;
        Save(today);
        PublishUpdated();
    }

    private void Save()
    {
        Save(DateTime.Now.ToString("yyyy-MM-dd"));
    }

    private void Save(string date)
    {
        var data = new DailyTargetSaveData
        {
            date = date,
            earnedToday = EarnedToday,
            isCompleted = IsCompleted,
            isClaimed = IsClaimed
        };

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    [Serializable]
    private class DailyTargetSaveData
    {
        public string date;
        public int earnedToday;
        public bool isCompleted;
        public bool isClaimed;
    }
}
