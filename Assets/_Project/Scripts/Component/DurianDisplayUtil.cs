using UnityEngine;
using UnityEngine.UI;

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

    /// <summary>无 DurianSpriteConfig 时的调色回退，数值与配置一致。</summary>
    public static Color GetAppearanceColor(AppearanceType appearance)
    {
        return appearance switch
        {
            AppearanceType.Poor => new Color(0.75f, 0.75f, 0.75f, 1f),
            AppearanceType.Normal => Color.white,
            AppearanceType.Good => new Color(1.10f, 1.05f, 0.90f, 1f),
            AppearanceType.Premium => new Color(1.20f, 1.10f, 0.70f, 1f),
            _ => Color.white
        };
    }

    /// <summary>v1.5：品种基底贴图 + 外观调色（市场/背包/开果页共用）。</summary>
    public static void ApplyUnopenedDurianVisual(
        Image image,
        DurianSpriteConfig config,
        VarietyType variety,
        AppearanceType appearance)
    {
        if (image == null)
        {
            return;
        }

        if (config != null)
        {
            image.sprite = config.GetUnopenedSprite(variety);
            image.color = config.GetAppearanceColor(appearance);
            image.preserveAspect = true;
        }
        else
        {
            image.sprite = null;
            image.color = GetAppearanceColor(appearance);
        }
    }

    /// <summary>外观等级角标（U-01~U-04），不受榴莲调色影响。</summary>
    public static void ApplyAppearanceIcon(Image icon, DurianSpriteConfig config, AppearanceType appearance)
    {
        if (icon == null)
        {
            return;
        }

        if (config != null)
        {
            icon.sprite = config.GetAppearanceIcon(appearance);
            icon.color = Color.white;
            icon.preserveAspect = true;
            icon.gameObject.SetActive(true);
        }
        else
        {
            icon.gameObject.SetActive(false);
        }
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
