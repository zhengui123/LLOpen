using System;
using UnityEngine;

/// <summary>
/// MVP 贴图集中配置：品种/外观/开果/UI/评级 Sprite 与查询方法。
/// 放在 Assets/_Project/Data/，由 GameLifetimeScope Inspector 拖引用。
/// </summary>
[CreateAssetMenu(fileName = "DurianSpriteConfig", menuName = "llopen/贴图配置")]
public class DurianSpriteConfig : ScriptableObject
{
    [Header("未开 - 普通外表")]
    public Sprite jinZhengNormal;
    public Sprite ganYaoNormal;
    public Sprite maoShanWangNormal;

    [Header("金枕 - 外观变体")]
    public Sprite jinZhengPoor;
    public Sprite jinZhengGood;
    public Sprite jinZhengPremium;

    [Header("干尧 - 外观变体")]
    public Sprite ganYaoPoor;
    public Sprite ganYaoGood;
    public Sprite ganYaoPremium;

    [Header("猫山王 - 外观变体")]
    public Sprite maoShanWangPoor;
    public Sprite maoShanWangGood;
    public Sprite maoShanWangPremium;

    [Header("金枕 - 已开")]
    public Sprite jinZhengEmpty;
    public Sprite jinZhengPerfect;

    [Header("干尧 - 已开")]
    public Sprite ganYaoEmpty;
    public Sprite ganYaoPerfect;

    [Header("猫山王 - 已开")]
    public Sprite maoShanWangEmpty;
    public Sprite maoShanWangPerfect;

    [Header("开果动画")]
    public Sprite knifeSprite;
    [Obsolete("v1.2 改为开裂叠加方案，不再使用左右半壳")]
    public Sprite shellLeftHalf;     // SH-01 废弃
    [Obsolete("v1.2 改为开裂叠加方案，不再使用左右半壳")]
    public Sprite shellRightHalf;    // SH-02 废弃
    public Sprite fleshPiece;
    public Sprite emptyPiece;

    [Header("开果动画 - 开裂阶段（v1.2新增）")]
    public Sprite crackStage1;   // CP-01 裂纹初现
    public Sprite crackStage2;   // CP-02 裂纹扩大
    public Sprite crackStage3;   // CP-03 半开裂
    public Sprite crackStage4;   // CP-04 大半开裂
    public Sprite crackStage5;   // CP-05 完全开裂

    [Header("外观等级图标")]
    public Sprite poorIcon;
    public Sprite normalIcon;
    public Sprite goodIcon;
    public Sprite premiumIcon;

    [Header("UI 图标")]
    public Sprite goldCoinIcon;
    public Sprite watchAdIcon;
    public Sprite swipeGuideIcon;
    public Sprite backArrowIcon;
    public Sprite varietyBtnBg;

    [Header("评级图标")]
    public Sprite emptyRating;
    public Sprite lowRating;
    public Sprite normalRating;
    public Sprite highRating;
    public Sprite perfectRating;
    public Sprite kingRating;

    [Header("v1.2 新增UI")]
    public Sprite refreshIcon;   // UI-10 换一批按钮
    public Sprite emptyBagIllust; // UI-11 空背包插图
    public Sprite goldCoinParticle; // EF-01 金币粒子
    public Sprite upgradeEffect;  // EF-02 升级光柱

    [Header("市场框架")]
    public Sprite marketFrame;

    /// <summary>根据开裂进度(0.0~1.0)获取对应的开裂阶段贴图。</summary>
    public Sprite GetCrackStage(float progress)
    {
        if (progress < 0.2f) return crackStage1;
        if (progress < 0.4f) return crackStage2;
        if (progress < 0.6f) return crackStage3;
        if (progress < 0.8f) return crackStage4;
        return crackStage5;
    }

    /// <summary>根据品种和外观获取未开榴莲贴图。</summary>
    public Sprite GetUnopenedSprite(VarietyType variety, AppearanceType appearance)
    {
        return (variety, appearance) switch
        {
            (VarietyType.JinZheng, AppearanceType.Poor) => jinZhengPoor,
            (VarietyType.JinZheng, AppearanceType.Normal) => jinZhengNormal,
            (VarietyType.JinZheng, AppearanceType.Good) => jinZhengGood,
            (VarietyType.JinZheng, AppearanceType.Premium) => jinZhengPremium,
            (VarietyType.GanYao, AppearanceType.Poor) => ganYaoPoor,
            (VarietyType.GanYao, AppearanceType.Normal) => ganYaoNormal,
            (VarietyType.GanYao, AppearanceType.Good) => ganYaoGood,
            (VarietyType.GanYao, AppearanceType.Premium) => ganYaoPremium,
            (VarietyType.MaoShanWang, AppearanceType.Poor) => maoShanWangPoor,
            (VarietyType.MaoShanWang, AppearanceType.Normal) => maoShanWangNormal,
            (VarietyType.MaoShanWang, AppearanceType.Good) => maoShanWangGood,
            (VarietyType.MaoShanWang, AppearanceType.Premium) => maoShanWangPremium,
            _ => jinZhengNormal
        };
    }

    /// <summary>根据品种和出肉率档位获取已开榴莲贴图（MVP 空壳/爆肉两极端）。</summary>
    public Sprite GetOpenedSprite(VarietyType variety, YieldGrade grade)
    {
        var empty = variety switch
        {
            VarietyType.JinZheng => jinZhengEmpty,
            VarietyType.GanYao => ganYaoEmpty,
            VarietyType.MaoShanWang => maoShanWangEmpty,
            _ => jinZhengEmpty
        };

        var perfect = variety switch
        {
            VarietyType.JinZheng => jinZhengPerfect,
            VarietyType.GanYao => ganYaoPerfect,
            VarietyType.MaoShanWang => maoShanWangPerfect,
            _ => jinZhengPerfect
        };

        return grade switch
        {
            YieldGrade.Empty or YieldGrade.Low => empty,
            YieldGrade.Perfect or YieldGrade.High => perfect,
            _ => perfect
        };
    }

    /// <summary>获取外观等级图标。</summary>
    public Sprite GetAppearanceIcon(AppearanceType appearance)
    {
        return appearance switch
        {
            AppearanceType.Poor => poorIcon,
            AppearanceType.Good => goodIcon,
            AppearanceType.Premium => premiumIcon,
            _ => normalIcon
        };
    }

    /// <summary>根据 YieldRatingUtil 文案获取评级图标。</summary>
    public Sprite GetRatingSprite(string ratingText)
    {
        return ratingText switch
        {
            "空壳" => emptyRating,
            "小亏" => lowRating,
            "回本" => normalRating,
            "小赚" => highRating,
            "大赚" => perfectRating,
            "榴莲之王" => kingRating,
            _ => normalRating
        };
    }

    /// <summary>根据出肉率档位获取评级图标。</summary>
    public Sprite GetRatingSprite(YieldGrade grade)
    {
        return grade switch
        {
            YieldGrade.Empty => emptyRating,
            YieldGrade.Low => lowRating,
            YieldGrade.Normal => normalRating,
            YieldGrade.High => highRating,
            YieldGrade.Perfect => perfectRating,
            _ => normalRating
        };
    }
}
