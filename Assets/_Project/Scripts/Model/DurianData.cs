using System;

/// <summary>
/// 榴莲运行时数据。
/// </summary>
[Serializable]
public struct DurianData
{
    public int id;
    public VarietyType variety;
    public AppearanceType appearance;
    public float appearancePriceMultiplier;
    public int basePrice;
    public int finalPrice;
    public float yieldRate;
    public int roomCount;
    public bool[] roomResults;
    public YieldGrade yieldGrade;
}
