using UnityEngine;

[CreateAssetMenu(fileName = "ShopConfig", menuName = "llopen/商店配置")]
public class ShopConfig : ScriptableObject
{
    public int[] upgradeCosts = { 0, 500 };
    public float[] sellBonuses = { 0f, 0.2f };
}
