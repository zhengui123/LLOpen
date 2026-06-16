using UnityEngine;

[CreateAssetMenu(fileName = "AppearanceConfig", menuName = "llopen/外观配置")]
public class AppearanceConfig : ScriptableObject
{
    public AppearanceType type;
    public string appearanceName;
    public float priceMultiplier;
    public float spawnWeight;
    public float[] probabilityOffsets = new float[5];
}
