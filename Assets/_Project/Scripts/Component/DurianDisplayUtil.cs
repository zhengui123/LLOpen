using UnityEngine;

/// <summary>
/// 榴莲展示文案与外观色块（市场/背包/OpenPage 共用）。
/// </summary>
public static class DurianDisplayUtil
{
    public static string GetVarietyName(VarietyType variety)
    {
        return variety switch
        {
            VarietyType.JinZheng => "金枕",
            VarietyType.GanYao => "干尧",
            VarietyType.MaoShanWang => "猫山王",
            _ => variety.ToString()
        };
    }

    public static string GetAppearanceName(AppearanceType appearance)
    {
        return appearance switch
        {
            AppearanceType.Poor => "劣质",
            AppearanceType.Normal => "普通",
            AppearanceType.Good => "优质",
            AppearanceType.Premium => "极品",
            _ => appearance.ToString()
        };
    }

    public static Color GetAppearanceColor(AppearanceType appearance)
    {
        return appearance switch
        {
            AppearanceType.Poor => new Color(0.45f, 0.35f, 0.25f),
            AppearanceType.Good => new Color(0.9f, 0.55f, 0.1f),
            AppearanceType.Premium => new Color(1f, 0.84f, 0.2f),
            _ => new Color(0.3f, 0.65f, 0.35f)
        };
    }

    /// <summary>
    /// 背包卡片简要信息：购买价 + 未开出肉率占位。
    /// </summary>
    public static string GetBagBriefInfo(DurianData durian)
    {
        return $"购价 {durian.finalPrice} · 出肉 ?";
    }

    public static string GetBagCardLabel(DurianData durian)
    {
        return $"{GetVarietyName(durian.variety)}\n{GetAppearanceName(durian.appearance)}";
    }
}
