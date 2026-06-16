using System.Collections.Generic;

/// <summary>
/// 背包管理器：存储已购买的榴莲，容量上限 10。
/// </summary>
public class BagManager
{
    public List<DurianData> Durians { get; private set; } = new();
    public int MaxCapacity { get; private set; } = 10;

    public void AddDurian(DurianData durian)
    {
        if (Durians.Count >= MaxCapacity)
        {
            EventBus.Publish(new BagFullEvent());
            return;
        }

        Durians.Add(durian);
        EventBus.Publish(new BagUpdatedEvent { Durians = Durians });
    }

    public DurianData RemoveDurian(int index)
    {
        if (index < 0 || index >= Durians.Count)
        {
            return default;
        }

        var durian = Durians[index];
        Durians.RemoveAt(index);
        EventBus.Publish(new BagUpdatedEvent { Durians = Durians });
        return durian;
    }

    /// <summary>
    /// 按 id 替换背包中的榴莲（复活重 roll 后同步数据）。
    /// </summary>
    public void ReplaceDurian(DurianData durian)
    {
        for (var i = 0; i < Durians.Count; i++)
        {
            if (Durians[i].id != durian.id)
            {
                continue;
            }

            Durians[i] = durian;
            EventBus.Publish(new BagUpdatedEvent { Durians = Durians });
            return;
        }
    }
}
