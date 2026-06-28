using UnityEngine;

/// <summary>
/// MVP 贴图集中配置：品种/外观调色/逐房揭示/UI/评级 Sprite 与查询方法。
/// 放在 Assets/_Project/Data/，由 GameLifetimeScope Inspector 拖引用。
/// </summary>
[CreateAssetMenu(fileName = "DurianSpriteConfig", menuName = "llopen/贴图配置")]
public class DurianSpriteConfig : ScriptableObject
{
    [Header("未开 - 调色基底（v1.5）")]
    public Sprite jinZhengNormal;
    public Sprite ganYaoNormal;
    public Sprite maoShanWangNormal;

    [Header("v1.5 逐房揭示 - 金枕壳盖")]
    public Sprite rcJzP1;
    public Sprite rcJzP2;
    public Sprite rcJzP3;
    public Sprite rcJzP4;
    public Sprite rcJzP5;

    [Header("v1.5 逐房揭示 - 果肉级别")]
    public Sprite rf00;
    public Sprite rf20;
    public Sprite rf50;
    public Sprite rf80;
    public Sprite rf100;

    [Header("开果动画")]
    public Sprite knifeSprite;
    public Sprite fleshPiece;
    public Sprite emptyPiece;

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
    public Sprite refreshIcon;
    public Sprite emptyBagIllust;
    public Sprite goldCoinParticle;
    public Sprite upgradeEffect;

    [Header("市场框架")]
    public Sprite marketFrame;

    [Header("v1.5 图鉴/连击（可占位）")]
    public Sprite collectionBookBg;
    public Sprite collectionLockedSlot;
    public Sprite collectionCompleteStamp;
    public Sprite comboFlameFx;
    public Sprite[] GetRcJz() => new[] { rcJzP1, rcJzP2, rcJzP3, rcJzP4, rcJzP5 };

    public Sprite GetRf(YieldGrade grade) => grade switch
    {
        YieldGrade.Empty => rf00,
        YieldGrade.Low => rf20,
        YieldGrade.Normal => rf50,
        YieldGrade.High => rf80,
        YieldGrade.Perfect => rf100,
        _ => rf50
    };

    public Color GetAppearanceColor(AppearanceType appearance) => appearance switch
    {
        AppearanceType.Poor => new Color(0.75f, 0.75f, 0.75f, 1f),
        AppearanceType.Normal => Color.white,
        AppearanceType.Good => new Color(1.10f, 1.05f, 0.90f, 1f),
        AppearanceType.Premium => new Color(1.20f, 1.10f, 0.70f, 1f),
        _ => Color.white
    };

    public Sprite GetUnopenedSprite(VarietyType variety) => variety switch
    {
        VarietyType.JinZheng => jinZhengNormal,
        VarietyType.GanYao => ganYaoNormal,
        VarietyType.MaoShanWang => maoShanWangNormal,
        _ => jinZhengNormal
    };

    public Sprite GetAppearanceIcon(AppearanceType appearance) => appearance switch
    {
        AppearanceType.Poor => poorIcon,
        AppearanceType.Good => goodIcon,
        AppearanceType.Premium => premiumIcon,
        _ => normalIcon
    };

    /// <summary>根据 YieldRatingUtil 文案获取评级图标。</summary>
    public Sprite GetRatingSprite(string ratingText) => ratingText switch
    {
        "空壳" => emptyRating,
        "小亏" => lowRating,
        "回本" => normalRating,
        "小赚" => highRating,
        "大赚" => perfectRating,
        "榴莲之王" => kingRating,
        _ => normalRating
    };

    /// <summary>根据出肉率档位获取评级图标。</summary>
    public Sprite GetRatingIcon(YieldGrade grade) => grade switch
    {
        YieldGrade.Empty => emptyRating,
        YieldGrade.Low => lowRating,
        YieldGrade.Normal => normalRating,
        YieldGrade.High => highRating,
        YieldGrade.Perfect => perfectRating,
        _ => normalRating
    };

    /// <summary>兼容旧调用，与 GetRatingIcon 相同。</summary>
    public Sprite GetRatingSprite(YieldGrade grade) => GetRatingIcon(grade);
}
