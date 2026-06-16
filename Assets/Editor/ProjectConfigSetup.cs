using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 一次性项目基础配置工具（URP 2D、Player、Quality、Time）。
/// </summary>
public static class ProjectConfigSetup
{
    private const string PipelinePath = "Assets/Settings/URP-Mobile-2D.asset";
    private const string RendererPath = "Assets/Settings/URP-Mobile-2D_Renderer.asset";

    [MenuItem("Tools/Apply DurianGame Project Config")]
    public static void Apply()
    {
        var pipeline = LoadOrCreatePipelineAsset();
        ApplyGraphicsSettings(pipeline);
        ApplyPlayerSettings();
        ApplyTimeSettings();
        ApplyQualitySettings(pipeline);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ProjectConfigSetup] 项目基础配置已应用。");
    }

    private static UniversalRenderPipelineAsset LoadOrCreatePipelineAsset()
    {
        var existing = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
        if (existing != null)
            return existing;

        return CreatePipelineAsset();
    }

    private static UniversalRenderPipelineAsset CreatePipelineAsset()
    {
        if (AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath) != null)
            AssetDatabase.DeleteAsset(PipelinePath);
        if (AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(RendererPath) != null)
            AssetDatabase.DeleteAsset(RendererPath);

        var createRenderer = typeof(UniversalRenderPipelineAsset).GetMethod(
            "CreateRendererAsset",
            BindingFlags.Static | BindingFlags.NonPublic);
        if (createRenderer == null)
            throw new System.InvalidOperationException("无法找到 CreateRendererAsset 方法。");

        var renderer2D = createRenderer.Invoke(
            null,
            new object[] { PipelinePath, RendererType._2DRenderer, true, "Renderer" }) as Renderer2DData;
        if (renderer2D == null)
            throw new System.InvalidOperationException("创建 2D Renderer 失败。");

        var pipeline = UniversalRenderPipelineAsset.Create(renderer2D);
        var pipelineSo = new SerializedObject(pipeline);
        pipelineSo.FindProperty("m_SupportsHDR").boolValue = false;
        pipelineSo.FindProperty("m_MSAA").intValue = 1;
        pipelineSo.FindProperty("m_MainLightShadowsSupported").boolValue = false;
        pipelineSo.FindProperty("m_AdditionalLightShadowsSupported").boolValue = false;
        pipelineSo.FindProperty("m_RequireDepthTexture").boolValue = false;
        pipelineSo.FindProperty("m_RequireOpaqueTexture").boolValue = false;
        pipelineSo.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(pipeline, PipelinePath);
        EditorUtility.SetDirty(pipeline);
        return pipeline;
    }

    private static void ApplyGraphicsSettings(UniversalRenderPipelineAsset pipeline)
    {
        GraphicsSettings.renderPipelineAsset = pipeline;

        var graphicsSo = new SerializedObject(Unsupported.GetSerializedAssetInterfaceSingleton("GraphicsSettings"));
        graphicsSo.FindProperty("m_InstancingStripping").intValue = 0;
        graphicsSo.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ApplyPlayerSettings()
    {
        PlayerSettings.companyName = "DurianGame";
        PlayerSettings.productName = "llopen";
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);
        PlayerSettings.SetMobileMTRendering(BuildTargetGroup.iOS, true);
        PlayerSettings.stripEngineCode = true;

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        const ApiCompatibilityLevel netStandard21 = (ApiCompatibilityLevel)6;
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, netStandard21);
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, netStandard21);

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "WECHAT_MINI_GAME");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "WECHAT_MINI_GAME");

        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.iOS.targetOSVersionString = "12.0";
        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
        PlayerSettings.Android.forceInternetPermission = true;

        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.High);
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, ManagedStrippingLevel.High);

        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.DurianGame.llkg");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.DurianGame.llkg");

        var playerSo = new SerializedObject(Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings"));
        var dpi = playerSo.FindProperty("targetPixelDensity");
        if (dpi != null)
            dpi.intValue = 320;
        playerSo.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ApplyTimeSettings()
    {
        Time.fixedDeltaTime = 0.01667f;
        Time.maximumDeltaTime = 0.1f;
    }

    private static void ApplyQualitySettings(UniversalRenderPipelineAsset pipeline)
    {
        var qualityObj = Unsupported.GetSerializedAssetInterfaceSingleton("QualitySettings");
        if (qualityObj == null)
        {
            Debug.LogWarning("[ProjectConfigSetup] 无法访问 QualitySettings。");
            return;
        }

        var qualitySo = new SerializedObject(qualityObj);
        var qualities = qualitySo.FindProperty("m_QualitySettings");
        if (qualities == null)
        {
            Debug.LogWarning("[ProjectConfigSetup] 无法访问 m_QualitySettings。");
            return;
        }

        var mobileIndex = -1;
        for (var i = 0; i < qualities.arraySize; i++)
        {
            if (qualities.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == "Mobile")
            {
                mobileIndex = i;
                break;
            }
        }

        if (mobileIndex < 0)
        {
            qualities.InsertArrayElementAtIndex(qualities.arraySize);
            mobileIndex = qualities.arraySize - 1;
        }

        var mobile = qualities.GetArrayElementAtIndex(mobileIndex);
        mobile.FindPropertyRelative("name").stringValue = "Mobile";
        mobile.FindPropertyRelative("pixelLightCount").intValue = 0;
        mobile.FindPropertyRelative("shadows").intValue = 0;
        mobile.FindPropertyRelative("shadowResolution").intValue = 0;
        mobile.FindPropertyRelative("antiAliasing").intValue = 0;
        mobile.FindPropertyRelative("vSyncCount").intValue = 0;
        mobile.FindPropertyRelative("lodBias").floatValue = 0.5f;
        mobile.FindPropertyRelative("particleRaycastBudget").intValue = 64;
        mobile.FindPropertyRelative("customRenderPipeline").objectReferenceValue = pipeline;

        var perPlatform = qualitySo.FindProperty("m_PerPlatformDefaultQuality");
        for (var i = 0; i < perPlatform.arraySize; i++)
        {
            var entry = perPlatform.GetArrayElementAtIndex(i);
            var platform = entry.FindPropertyRelative("m_BuildTarget").stringValue;
            if (platform == "Android" || platform == "iPhone")
                entry.FindPropertyRelative("m_QualityLevel").intValue = mobileIndex;
        }

        qualitySo.FindProperty("m_CurrentQuality").intValue = mobileIndex;
        qualitySo.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(qualityObj);
    }
}
