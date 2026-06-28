#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 验证 DurianSpriteConfig 中所有 Sprite 字段是否已挂载；支持从 v1.5texture 一键绑定。
/// </summary>
public static class TextureValidator
{
    private const string ConfigPath = "Assets/_Project/Data/DurianSpriteConfig.asset";
    private const string TextureRoot = "Assets/Art/v1.5texture/";
    private const string MvpTextureRoot = "Assets/Art/MVP/";

    [InitializeOnLoadMethod]
    private static void AutoFixTexturesOnLoad()
    {
        EditorApplication.delayCall += TryAutoFixTexturesOnLoad;
    }

    private static void TryAutoFixTexturesOnLoad()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        var config = AssetDatabase.LoadAssetAtPath<DurianSpriteConfig>(ConfigPath);
        if (config == null || config.jinZhengNormal != null)
        {
            return;
        }

        Debug.Log("[TextureValidator] 检测到 DurianSpriteConfig 未挂载贴图，自动执行修复与绑定…");
        FixImportAndBindV15Textures();
    }

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

    [MenuItem("Tools/llopen/修复 v1.5 贴图导入并绑定到配置")]
    public static void FixImportAndBindV15Textures()
    {
        FixV15TextureImportSettings();
        BindV15Textures();
    }

    [MenuItem("Tools/llopen/绑定 v1.5 贴图到 DurianSpriteConfig")]
    public static void BindV15Textures()
    {
        var config = AssetDatabase.LoadAssetAtPath<DurianSpriteConfig>(ConfigPath);
        if (config == null)
        {
            Debug.LogError($"[TextureValidator] 未找到 {ConfigPath}。");
            return;
        }

        var so = new SerializedObject(config);
        Bind(so, "jinZhengNormal", "D-01_金枕_调色基底.png");
        Bind(so, "ganYaoNormal", "D-02_干尧_完整.png");
        Bind(so, "maoShanWangNormal", "D-03_猫山王_完整.png");
        Bind(so, "rcJzP1", "RC-JZ-P1_顶部中央壳盖.png");
        Bind(so, "rcJzP2", "RC-JZ-P2_左上壳盖.png");
        Bind(so, "rcJzP3", "RC-JZ-P3_右上壳盖.png");
        Bind(so, "rcJzP4", "RC-JZ-P4_左下壳盖.png");
        Bind(so, "rcJzP5", "RC-JZ-P5_右下壳盖.png");
        Bind(so, "rf00", "RF-00_果肉0%.png");
        Bind(so, "rf20", "RF-20_果肉20%.png");
        Bind(so, "rf50", "RF-50_果肉50%.png");
        Bind(so, "rf80", "RF-80_果肉80%.png");
        Bind(so, "rf100", "RF-100_果肉100%.png");
        Bind(so, "knifeSprite", "K-01_水果刀.png");
        BindMvp(so, "fleshPiece", "Animation/FL-01_果肉块.png");
        BindMvp(so, "emptyPiece", "Animation/FL-02_空壳块.png");
        Bind(so, "poorIcon", "U-01_劣质外表图标.png");
        Bind(so, "normalIcon", "U-02_普通外表图标.png");
        Bind(so, "goodIcon", "U-03_优质外表图标.png");
        Bind(so, "premiumIcon", "U-04_极品外表图标.png");
        Bind(so, "goldCoinIcon", "UI-05_金币图标.png");
        Bind(so, "watchAdIcon", "UI-06_看广告图标.png");
        Bind(so, "swipeGuideIcon", "UI-07_滑动手势.png");
        Bind(so, "backArrowIcon", "UI-08_返回箭头.png");
        Bind(so, "varietyBtnBg", "UI-09_品种按钮底图.png");
        Bind(so, "emptyRating", "R-01_空壳评级.png");
        Bind(so, "lowRating", "R-02_小亏评级.png");
        Bind(so, "normalRating", "R-03_回本评级.png");
        Bind(so, "highRating", "R-04_小赚评级.png");
        Bind(so, "perfectRating", "R-05_大赚评级.png");
        Bind(so, "kingRating", "R-06_榴莲之王评级.png");
        Bind(so, "marketFrame", "F-01_市场框架.png");
        BindMvp(so, "refreshIcon", "UI_Icons/UI-10_换一批.png");
        BindMvp(so, "emptyBagIllust", "UI_Icons/UI-11_空背包.png");
        BindMvp(so, "goldCoinParticle", "Effects/EF-01_金币粒子.png");
        BindMvp(so, "upgradeEffect", "Effects/EF-02_升级光柱.png");

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        Debug.Log("[TextureValidator] v1.5 贴图已绑定到 DurianSpriteConfig（图鉴/连击/EF 占位字段未绑，可后续补图）。");
        ValidateDurianSpriteConfig();
    }

    private static void Bind(SerializedObject so, string fieldName, string fileName)
    {
        AssignSprite(so, fieldName, LoadSprite($"{TextureRoot}{fileName}"), $"{TextureRoot}{fileName}");
    }

    private static void BindMvp(SerializedObject so, string fieldName, string relativePath)
    {
        AssignSprite(so, fieldName, LoadSprite($"{MvpTextureRoot}{relativePath}"), $"{MvpTextureRoot}{relativePath}");
    }

    private static void AssignSprite(SerializedObject so, string fieldName, Sprite sprite, string pathForLog)
    {
        if (sprite == null)
        {
            Debug.LogWarning($"[TextureValidator] 未找到 Sprite：{pathForLog}");
            return;
        }

        var prop = so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogWarning($"[TextureValidator] 配置无字段：{fieldName}");
            return;
        }

        prop.objectReferenceValue = sprite;
    }

    /// <summary>v1.5texture 默认导入为 Default，需改为 Sprite 才能在 UI 中引用。</summary>
    public static void FixV15TextureImportSettings()
    {
        var root = Path.Combine(Application.dataPath, "Art/v1.5texture");
        if (!Directory.Exists(root))
        {
            Debug.LogError("[TextureValidator] 目录不存在：Assets/Art/v1.5texture");
            return;
        }

        var fixedCount = 0;
        foreach (var fullPath in Directory.GetFiles(root, "*.png", SearchOption.TopDirectoryOnly))
        {
            var assetPath = ToAssetPath(fullPath);
            if (ConfigureAsSprite(assetPath))
            {
                fixedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[TextureValidator] 已检查 v1.5texture 根目录贴图，修正导入为 Sprite：{fixedCount} 张。");
    }

    private static bool ConfigureAsSprite(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return false;
        }

        var needReimport = importer.textureType != TextureImporterType.Sprite
            || importer.spriteImportMode != SpriteImportMode.Single
            || !importer.alphaIsTransparency;

        if (!needReimport)
        {
            return false;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.spritePixelsPerUnit = 0.001f;
        importer.SaveAndReimport();
        return true;
    }

    private static string ToAssetPath(string fullPath)
    {
        fullPath = fullPath.Replace('\\', '/');
        var dataPath = Application.dataPath.Replace('\\', '/');
        return "Assets" + fullPath.Substring(dataPath.Length);
    }

    private static Sprite LoadSprite(string assetPath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite != null)
        {
            return sprite;
        }

        var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (var asset in assets)
        {
            if (asset is Sprite loaded)
            {
                return loaded;
            }
        }

        if (ConfigureAsSprite(assetPath))
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        return null;
    }
}
#endif
