/// <summary>
/// 根据出肉率返回 GDD 评级文案。
/// </summary>
public static class YieldRatingUtil
{
    public static string GetRating(float yieldRate)
    {
        if (yieldRate < 15f) return "空壳";
        if (yieldRate < 30f) return "小亏";
        if (yieldRate < 45f) return "回本";
        if (yieldRate < 60f) return "小赚";
        if (yieldRate < 75f) return "大赚";
        return "榴莲之王";
    }
}
