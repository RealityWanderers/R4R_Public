using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlatKit;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class ObjectOutlineEditorUtils {
    private static readonly GUIStyle RichHelpBoxStyle = new(EditorStyles.helpBox) { richText = true };

    public static void SetActive(Material material, bool active) {
        var renderer = GetRenderer(Camera.current ?? Camera.main);
        if (renderer == null) {
            const string m = "<b>ScriptableRendererData</b> is required to enable per-object outlines.\n" +
                             "Please assign a <b>URP Asset</b> in the Graphics settings.";
            EditorGUILayout.LabelField(m, RichHelpBoxStyle);
            return;
        }

        var features = GetRendererFeatures(renderer);
        if (features == null) {
            const string m = "<b>ScriptableRendererFeature</b> is required to enable per-object outlines.\n" +
                             "Please assign a <b>URP Asset</b> in the Graphics settings.";
            EditorGUILayout.LabelField(m, RichHelpBoxStyle);
            return;
        }

        var feature = features.FirstOrDefault(f => f.GetType() == typeof(ObjectOutlineRendererFeature));
        if (feature == null) {
            if (active) {
                // Add the feature.
                var rendererData = GetRendererData();
                if (rendererData == null) return;

                feature = ScriptableObject.CreateInstance<ObjectOutlineRendererFeature>();
                feature.name = "Flat Kit Per Object Outline";
                AddRendererFeature(rendererData, feature);

                var m = $"<color=grey>[Flat Kit]</color> <b>Added</b> <color=green>{feature.name}</color> Renderer " +
                        $"Feature to <b>{rendererData.name}</b>.";
                Debug.Log(m, rendererData);
            } else {
                // Feature not added and outline is disabled.
                return;
            }
        }

        // Register the material. This handles the feature activation.
        var outlineFeature = feature as ObjectOutlineRendererFeature;
        if (outlineFeature == null) {
            Debug.LogError("ObjectOutlineRendererFeature not found");
            return;
        }

        var featureIsUsed = outlineFeature.RegisterMaterial(material, active);

        // Remove the feature if no materials are using it.
        if (!featureIsUsed) {
            var rendererData = GetRendererData();
            if (rendererData == null) return;
            RemoveRendererFeature(rendererData, feature);

            var m = $"<color=grey>[Flat Kit]</color> <b>Removed</b> <color=green>{feature.name}</color> Renderer " +
                    $"Feature from <b>{rendererData.name}</b> because no materials are using it.";
            Debug.Log(m, rendererData);
        }
    }

    private static void AddRendererFeature(ScriptableRendererData rendererData, ScriptableRendererFeature feature) {
        // Save the asset as a sub-asset.
        AssetDatabase.AddObjectToAsset(feature, rendererData);
        rendererData.rendererFeatures.Add(feature);
        rendererData.SetDirty();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void RemoveRendererFeature(ScriptableRendererData rendererData, ScriptableRendererFeature feature) {
        rendererData.rendererFeatures.Remove(feature);
        rendererData.SetDirty();

        // Remove the asset.
        AssetDatabase.RemoveObjectFromAsset(feature);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [CanBeNull]
    private static ScriptableRenderer GetRenderer(Camera camera) {
        if (!camera) {
            return null;
        }

        var additionalCameraData = camera.GetComponent<UniversalAdditionalCameraData>();
        if (!additionalCameraData) {
            return null;
        }

        var renderer = additionalCameraData.scriptableRenderer;
        return renderer;
    }

    private static List<ScriptableRendererFeature> GetRendererFeatures(ScriptableRenderer renderer) {
        var property =
            typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);
        if (property == null) return null;
        var features = property.GetValue(renderer) as List<ScriptableRendererFeature>;
        return features;
    }

    internal static ScriptableRendererData GetRendererData() {
#if UNITY_6000_0_OR_NEWER
        var srpAsset = GraphicsSettings.defaultRenderPipeline;
#else
        var srpAsset = GraphicsSettings.renderPipelineAsset;
#endif
        if (srpAsset == null) {
            const string m = "<b>Flat Kit</b> No SRP asset found. Please assign a URP Asset in the Graphics settings " +
                             "to enable per-object outlines.";
            Debug.LogError(m);
            return null;
        }

        var field = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var rendererDataList = (ScriptableRendererData[])field!.GetValue(srpAsset);

        var rendererData = rendererDataList.FirstOrDefault();
        if (rendererData == null) {
            Debug.LogError("No ScriptableRendererData found");
            return null;
        }

        return rendererData;
    }
}