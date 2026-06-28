using UnityEngine;

/// <summary>
/// 商店升级配置：v1.5 支持 3 级（0 / 500 / 1500 金，+0% / +20% / +35% 回收）。
/// </summary>
[CreateAssetMenu(fileName = "ShopConfig", menuName = "llopen/商店配置")]
public class ShopConfig : ScriptableObject
{
    public int[] upgradeCosts = { 0, 500, 1500 };
    public float[] sellBonuses = { 0f, 0.2f, 0.35f };
}
