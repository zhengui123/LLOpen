using UnityEngine;

[CreateAssetMenu(fileName = "VarietyConfig", menuName = "榴莲开了/品种配置")]
public class VarietyConfig : ScriptableObject
{
    public VarietyType type;
    public string varietyName;
    public int basePrice;
    public float baseYieldRate;
    public int minRooms;
    public int maxRooms;
    public float[] baseWeights = new float[5];
}
