#if UNITY_EDITOR
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 验证 DurianSpriteConfig 中所有 Sprite 字段是否已挂载。
/// </summary>
public static class TextureValidator
{
    private const string ConfigPath = "Assets/_Project/Data/DurianSpriteConfig.asset";

    [MenuItem("Tools/llopen/验证贴图配置")]
    public static void ValidateDurianSpriteConfig()
    {
        var config = AssetDatabase.LoadAssetAtPath<DurianSpriteConfig>(ConfigPath);
        if (config == null)
        {
            Debug.LogError($"[TextureValidator] 未找到 {ConfigPath}，请先创建贴图配置。");
            return;
        }

        var fields = typeof(DurianSpriteConfig).GetFields(BindingFlags.Public | BindingFlags.Instance);
        var assigned = 0;
        var missing = 0;
        var report = new StringBuilder();
        report.AppendLine("[TextureValidator] DurianSpriteConfig 检查结果：");

        foreach (var field in fields)
        {
            if (field.FieldType != typeof(Sprite))
            {
                continue;
            }

            var sprite = field.GetValue(config) as Sprite;
            if (sprite != null)
            {
                assigned++;
                report.AppendLine($"  ✓ {field.Name} → {sprite.name}");
            }
            else
            {
                missing++;
                report.AppendLine($"  ✗ {field.Name} (缺失)");
            }
        }

        report.AppendLine($"合计：已挂载 {assigned}，缺失 {missing}，共 {assigned + missing} 个 Sprite 字段。");
        report.AppendLine("说明：S-01/S-02 场景背景在 GameUIRoot，不在 DurianSpriteConfig。");

        if (missing == 0)
        {
            Debug.Log(report.ToString());
        }
        else
        {
            Debug.LogWarning(report.ToString());
        }

        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);
    }
}
#endif
