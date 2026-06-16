using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 一键创建《llopen》项目目录、link.xml、程序集定义与启动场景。
/// </summary>
public static class CreateProjectStructure
{
    private const string MenuPath = "Tools/llopen/Create Project Structure";

    private static readonly string[] FolderPaths =
    {
        "Assets/Scripts/App",
        "Assets/Scripts/Core/DI",
        "Assets/Scripts/Core/Event",
        "Assets/Scripts/Core/StateMachine",
        "Assets/Scripts/Core/UI",
        "Assets/Scripts/Core/Resource",
        "Assets/Scripts/Core/Audio",
        "Assets/Scripts/Core/Pool",
        "Assets/Scripts/Core/Save",
        "Assets/Scripts/Core/Util",
        "Assets/Scripts/Model/GameData",
        "Assets/Scripts/Model/Config",
        "Assets/Scripts/Model/Save",
        "Assets/Scripts/Manager/Market",
        "Assets/Scripts/Manager/Opening",
        "Assets/Scripts/Manager/Crafting",
        "Assets/Scripts/Manager/Selling",
        "Assets/Scripts/Manager/Economy",
        "Assets/Scripts/Manager/Social",
        "Assets/Scripts/Manager/Ad",
        "Assets/Scripts/Manager/Progression",
        "Assets/Scripts/Manager/DayCycle",
        "Assets/Scripts/System/DurianGenerator",
        "Assets/Scripts/System/Estimation",
        "Assets/Scripts/System/Processing",
        "Assets/Scripts/System/CustomerAI",
        "Assets/Scripts/System/Achievement",
        "Assets/Scripts/View/Durian",
        "Assets/Scripts/View/Market",
        "Assets/Scripts/View/Crafting",
        "Assets/Scripts/View/Selling",
        "Assets/Scripts/View/Common",
        "Assets/Scripts/Platform",
        "Assets/Scripts/Generated",
        "Assets/Configs",
        "Assets/AddressableAssets/Prefabs",
        "Assets/AddressableAssets/Textures",
        "Assets/AddressableAssets/Audio",
        "Assets/AddressableAssets/Spine",
        "Assets/AddressableAssets/Scenes",
        "Assets/Resources",
        "Assets/Scenes",
        "Assets/Art/UI",
        "Assets/Art/BG",
        "Assets/Art/Effect",
        "Assets/Audio/BGM",
        "Assets/Audio/SFX",
        "Assets/Plugins",
        "Assets/Spine",
    };

    private const string LinkXmlContent = @"<linker>
  <assembly fullname=""UnityEngine.CoreModule"">
    <type fullname=""UnityEngine.GameObject"" preserve=""all""/>
    <type fullname=""UnityEngine.Transform"" preserve=""all""/>
    <type fullname=""UnityEngine.MonoBehaviour"" preserve=""all""/>
  </assembly>
  <assembly fullname=""VContainer"">
    <type fullname=""VContainer.Unity.LifetimeScope"" preserve=""all""/>
  </assembly>
  <assembly fullname=""UniTask"">
    <type fullname=""Cysharp.Threading.Tasks.UniTask"" preserve=""all""/>
  </assembly>
  <assembly fullname=""UniRx"">
    <type fullname=""UniRx.ReactiveProperty`1"" preserve=""all""/>
  </assembly>
  <assembly fullname=""Newtonsoft.Json"">
    <type fullname=""Newtonsoft.Json.JsonConvert"" preserve=""all""/>
  </assembly>
  <assembly fullname=""MemoryPack"">
    <type fullname=""MemoryPack.MemoryPackSerializer"" preserve=""all""/>
  </assembly>
  <assembly fullname=""spine-csharp"" preserve=""all""/>
  <assembly fullname=""spine-unity"">
    <type fullname=""Spine.Unity.SkeletonAnimation"" preserve=""all""/>
    <type fullname=""Spine.Unity.SkeletonGraphic"" preserve=""all""/>
  </assembly>
  <assembly fullname=""Assembly-CSharp"">
    <type fullname=""*"" preserve=""all""/>
  </assembly>
</linker>
";

    [MenuItem(MenuPath)]
    public static void Create()
    {
        CreateFolders();
        WriteLinkXml();
        MigrateAndCleanupLegacyAssets();
        CreateAssemblyDefinitions();
        CreateScenes();
        UpdateBuildSettings();

        AssetDatabase.Refresh();
        Debug.Log("[CreateProjectStructure] 项目结构创建完成。");
    }

    private static void CreateFolders()
    {
        foreach (var path in FolderPaths)
            EnsureFolder(path);

        foreach (var path in FolderPaths)
            EnsureGitKeep(path);
    }

    private static void EnsureFolder(string assetPath)
    {
        if (AssetDatabase.IsValidFolder(assetPath))
            return;

        var parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
        var folderName = Path.GetFileName(assetPath);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
            return;

        if (!AssetDatabase.IsValidFolder(parent))
        {
            var parts = assetPath.Replace('\\', '/').Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
            return;
        }

        AssetDatabase.CreateFolder(parent, folderName);
    }

    private static void EnsureGitKeep(string folderPath)
    {
        var gitKeepPath = $"{folderPath}/.gitkeep";
        var fullPath = Path.Combine(Application.dataPath, "..", gitKeepPath).Replace('\\', '/');
        fullPath = Path.GetFullPath(fullPath);
        if (!File.Exists(fullPath))
            File.WriteAllText(fullPath, string.Empty, Encoding.UTF8);
    }

    private static void WriteLinkXml()
    {
        var linkPath = "Assets/link.xml";
        var fullPath = Path.Combine(Application.dataPath, "..", linkPath);
        fullPath = Path.GetFullPath(fullPath);

        if (File.Exists(fullPath))
        {
            Debug.Log("[CreateProjectStructure] link.xml 已存在，跳过覆盖。");
            return;
        }

        File.WriteAllText(fullPath, LinkXmlContent, Encoding.UTF8);
    }

    private static void MigrateAndCleanupLegacyAssets()
    {
        const string oldBootstrap = "Assets/Scripts/GameBootstrap.cs";
        const string newBootstrap = "Assets/Scripts/App/GameBootstrap.cs";

        if (File.Exists(GetFullPath(oldBootstrap)) && !File.Exists(GetFullPath(newBootstrap)))
            AssetDatabase.MoveAsset(oldBootstrap, newBootstrap);

        DeleteAssetIfExists("Assets/TTTT.cs");
        DeleteAssetIfExists("Assets/Scenes/SampleScene.unity");
    }

    private static void DeleteAssetIfExists(string assetPath)
    {
        if (!File.Exists(GetFullPath(assetPath)) && !File.Exists(GetFullPath(assetPath + ".meta")))
            return;

        if (AssetDatabase.DeleteAsset(assetPath))
            Debug.Log($"[CreateProjectStructure] 已删除: {assetPath}");
    }

    private static void CreateAssemblyDefinitions()
    {
        WriteAsmDef("Assets/Scripts/Core/Core.asmdef", "DurianGame.Core", "DurianGame.Core");
        RefreshAssets();

        WriteAsmDef("Assets/Scripts/Model/Model.asmdef", "DurianGame.Model", "DurianGame.Model",
            "Assets/Scripts/Core/Core.asmdef");
        WriteAsmDef("Assets/Scripts/Platform/Platform.asmdef", "DurianGame.Platform", "DurianGame.Platform",
            "Assets/Scripts/Core/Core.asmdef");
        RefreshAssets();

        WriteAsmDef("Assets/Scripts/System/System.asmdef", "DurianGame.System", "DurianGame.System",
            "Assets/Scripts/Core/Core.asmdef",
            "Assets/Scripts/Model/Model.asmdef");
        WriteAsmDef("Assets/Scripts/Generated/Generated.asmdef", "DurianGame.Generated", "DurianGame.Generated",
            "Assets/Scripts/Model/Model.asmdef");
        RefreshAssets();

        WriteAsmDef("Assets/Scripts/Manager/Manager.asmdef", "DurianGame.Manager", "DurianGame.Manager",
            "Assets/Scripts/Core/Core.asmdef",
            "Assets/Scripts/Model/Model.asmdef",
            "Assets/Scripts/System/System.asmdef");
        RefreshAssets();

        WriteAsmDef("Assets/Scripts/View/View.asmdef", "DurianGame.View", "DurianGame.View",
            "Assets/Scripts/Core/Core.asmdef",
            "Assets/Scripts/Model/Model.asmdef",
            "Assets/Scripts/Manager/Manager.asmdef");
        WriteAsmDef("Assets/Scripts/App/App.asmdef", "DurianGame.App", "DurianGame.App",
            "Assets/Scripts/Core/Core.asmdef",
            "Assets/Scripts/Model/Model.asmdef",
            "Assets/Scripts/Manager/Manager.asmdef");
    }

    private static void RefreshAssets()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void WriteAsmDef(string assetPath, string assemblyName, string rootNamespace, params string[] referenceAsmDefPaths)
    {
        if (File.Exists(GetFullPath(assetPath)))
        {
            Debug.Log($"[CreateProjectStructure] 已存在，跳过: {assetPath}");
            return;
        }

        var references = new List<string>();
        foreach (var refPath in referenceAsmDefPaths)
        {
            var guid = AssetDatabase.AssetPathToGUID(refPath);
            if (!string.IsNullOrEmpty(guid))
                references.Add($"GUID:{guid}");
        }

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"    \"name\": \"{assemblyName}\",");
        sb.AppendLine($"    \"rootNamespace\": \"{rootNamespace}\",");
        sb.AppendLine("    \"references\": [");
        for (var i = 0; i < references.Count; i++)
        {
            var comma = i < references.Count - 1 ? "," : string.Empty;
            sb.AppendLine($"        \"{references[i]}\"{comma}");
        }
        sb.AppendLine("    ],");
        sb.AppendLine("    \"includePlatforms\": [],");
        sb.AppendLine("    \"excludePlatforms\": [],");
        sb.AppendLine("    \"allowUnsafeCode\": false,");
        sb.AppendLine("    \"overrideReferences\": false,");
        sb.AppendLine("    \"precompiledReferences\": [],");
        sb.AppendLine("    \"autoReferenced\": true,");
        sb.AppendLine("    \"defineConstraints\": [],");
        sb.AppendLine("    \"versionDefines\": [],");
        sb.AppendLine("    \"noEngineReferences\": false");
        sb.AppendLine("}");

        var fullPath = GetFullPath(assetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.ImportAsset(assetPath);
    }

    private static void CreateScenes()
    {
        CreateSceneIfMissing("Assets/Scenes/Launch.unity");
        CreateSceneIfMissing("Assets/Scenes/Main.unity");
    }

    private static void CreateSceneIfMissing(string scenePath)
    {
        if (File.Exists(GetFullPath(scenePath)))
        {
            Debug.Log($"[CreateProjectStructure] 场景已存在，跳过: {scenePath}");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[CreateProjectStructure] 已创建场景: {scenePath}");
    }

    private static void UpdateBuildSettings()
    {
        RefreshAssets();

        var launch = "Assets/Scenes/Launch.unity";
        var main = "Assets/Scenes/Main.unity";

        var scenes = new List<EditorBuildSettingsScene>();
        if (File.Exists(GetFullPath(launch)))
            scenes.Add(new EditorBuildSettingsScene(launch, true));
        if (File.Exists(GetFullPath(main)))
            scenes.Add(new EditorBuildSettingsScene(main, true));

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[CreateProjectStructure] Build Settings 已更新为 Launch → Main。");
    }

    private static string GetFullPath(string assetPath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
    }
}
