#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 批量配置 MVP 贴图导入参数（Sprite、尺寸、PPU、压缩）。
/// </summary>
public static class MvpTextureImportSettingsEditor
{
    private const string TextureRoot = "Assets/Art/MVP";

    [MenuItem("Tools/llopen/Configure Texture Import Settings")]
    public static void ConfigureAll()
    {
        if (!AssetDatabase.IsValidFolder(TextureRoot))
        {
            Debug.LogError($"[MvpTextureImport] 目录不存在：{TextureRoot}");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { TextureRoot });
        var processed = 0;
        var skipped = 0;

        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!TryConfigure(assetPath))
            {
                skipped++;
            }
            else
            {
                processed++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[MvpTextureImport] 完成：已处理 {processed} 张，跳过 {skipped} 张。（目录 {TextureRoot}）");
    }

    private static bool TryConfigure(string assetPath)
    {
        var extension = Path.GetExtension(assetPath).ToLowerInvariant();
        if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
        {
            return false;
        }

        var fileName = Path.GetFileName(assetPath);
        var category = Classify(fileName);
        if (category == TextureCategory.Unknown)
        {
            Debug.LogWarning($"[MvpTextureImport] 未识别命名规则，已跳过：{assetPath}");
            return false;
        }

        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return false;
        }

        ApplyImportSettings(importer, category, extension);
        importer.SaveAndReimport();
        return true;
    }

    private static void ApplyImportSettings(TextureImporter importer, TextureCategory category, string extension)
    {
        var isJpeg = extension is ".jpg" or ".jpeg";

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;

        var textureSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(textureSettings);
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.alphaIsTransparency = !isJpeg;
        importer.alphaSource = isJpeg ? TextureImporterAlphaSource.None : TextureImporterAlphaSource.FromInput;
        importer.sRGBTexture = true;

        var maxSize = category switch
        {
            TextureCategory.Scene => 2048,
            TextureCategory.Durian => 1024,
            _ => 512
        };

        importer.maxTextureSize = maxSize;

        if (category == TextureCategory.Durian)
        {
            importer.spritePixelsPerUnit = 512f;
        }
        else if (category == TextureCategory.SmallIcon)
        {
            importer.spritePixelsPerUnit = 256f;
        }

        ApplyPlatformCompression(importer, maxSize);
    }

    private static void ApplyPlatformCompression(TextureImporter importer, int maxSize)
    {
        // 先清空历史平台覆盖，避免旧的 DXT5/BC3 残留继续报错
        importer.ClearPlatformTextureSettings("DefaultTexturePlatform");
        importer.ClearPlatformTextureSettings("Standalone");
        importer.ClearPlatformTextureSettings("Android");
        importer.ClearPlatformTextureSettings("iPhone");
        importer.ClearPlatformTextureSettings("WebGL");

        // Sprite 类型不能手动指定 DXT1/DXT5，需用 Automatic 由 Unity 选择兼容格式
        var defaultSettings = importer.GetDefaultPlatformTextureSettings();
        defaultSettings.overridden = true;
        defaultSettings.maxTextureSize = maxSize;
        defaultSettings.textureCompression = TextureImporterCompression.Compressed;
        defaultSettings.format = TextureImporterFormat.Automatic;
        importer.SetPlatformTextureSettings(defaultSettings);
    }

    private static TextureCategory Classify(string fileName)
    {
        if (fileName.StartsWith("S-"))
        {
            return TextureCategory.Scene;
        }

        if (fileName.StartsWith("D-O")
            || fileName.StartsWith("D-A")
            || fileName.StartsWith("D-")
            || fileName.StartsWith("SH-"))
        {
            return TextureCategory.Durian;
        }

        if (fileName.StartsWith("UI-")
            || fileName.StartsWith("U-")
            || fileName.StartsWith("K-")
            || fileName.StartsWith("FL-")
            || fileName.StartsWith("R-")
            || fileName.StartsWith("F-"))
        {
            return TextureCategory.SmallIcon;
        }

        return TextureCategory.Unknown;
    }

    private enum TextureCategory
    {
        Unknown,
        Scene,
        Durian,
        SmallIcon
    }
}
#endif
