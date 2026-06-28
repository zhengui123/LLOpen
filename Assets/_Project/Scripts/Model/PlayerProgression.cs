using System;
using UnityEngine;

/// <summary>
/// 玩家长期进度：图鉴、连击纪录、开果统计等（PlayerPrefs 持久化）。
/// </summary>
public class PlayerProgression
{
    public const int CollectionSlotCount = 15;

    public static PlayerProgression Instance { get; } = new PlayerProgression();

    public bool[] CollectionUnlocked = new bool[CollectionSlotCount];
    public int[] CollectionBestPrice = new int[CollectionSlotCount];
    public int BestStreak = 0;
    public int TotalOpens = 0;
    public int TotalGoldEarned = 0;
    public int MaoShanWangOpens = 0;

    private const string SaveKey = "llopen_player_progression_v1";

    private PlayerProgression()
    {
        Load();
    }

    public static int GetCollectionIndex(VarietyType variety, YieldGrade grade)
        => (int)variety * 5 + (int)grade;

    public void UnlockCollection(VarietyType variety, YieldGrade grade, int sellPrice)
    {
        var idx = GetCollectionIndex(variety, grade);
        if (idx < 0 || idx >= CollectionSlotCount)
        {
            return;
        }

        var isNew = !CollectionUnlocked[idx];
        if (isNew)
        {
            CollectionUnlocked[idx] = true;
            CollectionBestPrice[idx] = sellPrice;
            EventBus.Publish(new CollectionNewEntryEvent
            {
                Variety = variety,
                Grade = grade
            });
        }
        else if (sellPrice > CollectionBestPrice[idx])
        {
            CollectionBestPrice[idx] = sellPrice;
        }

        Save();
    }

    public int GetCollectionCount()
    {
        var count = 0;
        for (var i = 0; i < CollectionSlotCount; i++)
        {
            if (CollectionUnlocked[i])
            {
                count++;
            }
        }

        return count;
    }

    public bool IsRowComplete(VarietyType variety)
    {
        var baseIndex = (int)variety * 5;
        for (var i = 0; i < 5; i++)
        {
            if (!CollectionUnlocked[baseIndex + i])
            {
                return false;
            }
        }

        return true;
    }

    public void Save()
    {
        var data = new PlayerProgressionSaveData
        {
            collectionUnlocked = CollectionUnlocked,
            collectionBestPrice = CollectionBestPrice,
            bestStreak = BestStreak,
            totalOpens = TotalOpens,
            totalGoldEarned = TotalGoldEarned,
            maoShanWangOpens = MaoShanWangOpens
        };

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            ResetToDefault();
            return;
        }

        var json = PlayerPrefs.GetString(SaveKey);
        if (string.IsNullOrEmpty(json))
        {
            ResetToDefault();
            return;
        }

        try
        {
            var data = JsonUtility.FromJson<PlayerProgressionSaveData>(json);
            if (data == null)
            {
                ResetToDefault();
                return;
            }

            CollectionUnlocked = NormalizeBoolArray(data.collectionUnlocked, CollectionSlotCount);
            CollectionBestPrice = NormalizeIntArray(data.collectionBestPrice, CollectionSlotCount);
            BestStreak = data.bestStreak;
            TotalOpens = data.totalOpens;
            TotalGoldEarned = data.totalGoldEarned;
            MaoShanWangOpens = data.maoShanWangOpens;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayerProgression] 读档失败，已重置：{e.Message}");
            ResetToDefault();
        }
    }

    private void ResetToDefault()
    {
        CollectionUnlocked = new bool[CollectionSlotCount];
        CollectionBestPrice = new int[CollectionSlotCount];
        BestStreak = 0;
        TotalOpens = 0;
        TotalGoldEarned = 0;
        MaoShanWangOpens = 0;
    }

    private static bool[] NormalizeBoolArray(bool[] source, int length)
    {
        var result = new bool[length];
        if (source == null)
        {
            return result;
        }

        var copyLength = Math.Min(source.Length, length);
        for (var i = 0; i < copyLength; i++)
        {
            result[i] = source[i];
        }

        return result;
    }

    private static int[] NormalizeIntArray(int[] source, int length)
    {
        var result = new int[length];
        if (source == null)
        {
            return result;
        }

        var copyLength = Math.Min(source.Length, length);
        for (var i = 0; i < copyLength; i++)
        {
            result[i] = source[i];
        }

        return result;
    }

    [Serializable]
    private class PlayerProgressionSaveData
    {
        public bool[] collectionUnlocked;
        public int[] collectionBestPrice;
        public int bestStreak;
        public int totalOpens;
        public int totalGoldEarned;
        public int maoShanWangOpens;
    }
}
