using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using System;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using RenderGraphUtils = UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils;
#endif

public class ScreenRenderPass : ScriptableRenderPass {
    private Material _passMaterial;
    private bool _requiresColor;
    private bool _isBeforeTransparents;
    private PassData _passData;
    private ProfilingSampler _profilingSampler;
    private RTHandle _copiedColor;

#if UNITY_6000_0_OR_NEWER
    private RenderTextureDescriptor _texDescriptor = new(Screen.width, Screen.height,
        RenderTextureFormat.Default, 0);
#endif

    private const string TexName = "_BlitTexture";
    private static readonly int BlitTextureShaderID = Shader.PropertyToID(TexName);

    public void Setup(Material mat, bool requiresColor, bool isBeforeTransparents, string featureName,
        in RenderingData renderingData) {
        _passMaterial = mat;
        _requiresColor = requiresColor;
        _isBeforeTransparents = isBeforeTransparents;
        _profilingSampler ??= new ProfilingSampler(featureName);

        var colorCopyDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        colorCopyDescriptor.depthBufferBits = (int)DepthBits.None;

#if UNITY_6000_0_OR_NEWER
        RenderingUtils.ReAllocateHandleIfNeeded(ref _copiedColor, colorCopyDescriptor, FilterMode.Point,
            TextureWrapMode.Clamp, name: "_FullscreenPassColorCopy");
#elif UNITY_2022_3_OR_NEWER
        RenderingUtils.ReAllocateIfNeeded(ref _copiedColor, colorCopyDescriptor, name: "_FullscreenPassColorCopy");
#endif

        _passData ??= new PassData();
    }

    public void Dispose() {
        _copiedColor?.Release();
    }

#if UNITY_6000_0_OR_NEWER
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // The following line ensures that the render pass doesn't blit from the back buffer.
        if (resourceData.isActiveTargetBackBuffer) {
            return;
        }

        {
            _texDescriptor.width = cameraData.cameraTargetDescriptor.width;
            _texDescriptor.height = cameraData.cameraTargetDescriptor.height;
            _texDescriptor.depthBufferBits = 0;
        }

        TextureHandle srcCamColor = resourceData.activeColorTexture;
        TextureHandle dst = UniversalRenderer.CreateRenderGraphTexture(renderGraph, _texDescriptor, TexName, false);

        // This check is to avoid an error from the material preview in the scene.
        if (!srcCamColor.IsValid() || !dst.IsValid()) {
            return;
        }

        RenderGraphUtils.BlitMaterialParameters blit1 = new(srcCamColor, dst, _passMaterial, shaderPass: 0);
        renderGraph.AddBlitPass(blit1, $"{_profilingSampler.name} (Effect Pass)");
        renderGraph.AddCopyPass(dst, srcCamColor);
    }

    [Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled).", false)]
#endif
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        _passData.effectMaterial = _passMaterial;
        _passData.requiresColor = _requiresColor;
        _passData.isBeforeTransparents = _isBeforeTransparents;
        _passData.profilingSampler = _profilingSampler;
        _passData.copiedColor = _copiedColor;

        ExecutePass(_passData, ref renderingData, ref context);
    }

#if UNITY_6000_0_OR_NEWER
    [Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled).", false)]
#endif
    private static void ExecutePass(PassData passData, ref RenderingData renderingData,
        ref ScriptableRenderContext context) {
        var passMaterial = passData.effectMaterial;
        var requiresColor = passData.requiresColor;
        var copiedColor = passData.copiedColor;
        var profilingSampler = passData.profilingSampler;

        if (passMaterial == null) {
            return;
        }

        if (renderingData.cameraData.isPreviewCamera) {
            return;
        }

#if UNITY_2022_3_OR_NEWER
        CommandBuffer cmd = renderingData.commandBuffer;
#else
        CommandBuffer cmd = CommandBufferPool.Get();
#endif
        var cameraData = renderingData.cameraData;

        using (new ProfilingScope(cmd, profilingSampler)) {
            if (requiresColor) {
#if UNITY_2022_3_OR_NEWER
                var source = passData.isBeforeTransparents
                    ? cameraData.renderer.GetCameraColorBackBuffer(cmd)
                    : cameraData.renderer.cameraColorTargetHandle;
                Blitter.BlitCameraTexture(cmd, source, copiedColor);
#else
                var source = cameraData.renderer.cameraColorTarget;
                cmd.Blit(source, copiedColor);
#endif

                passMaterial.SetTexture(BlitTextureShaderID, copiedColor);
            }

#if UNITY_2022_3_OR_NEWER
            CoreUtils.SetRenderTarget(cmd, cameraData.renderer.GetCameraColorBackBuffer(cmd));
#else
            CoreUtils.SetRenderTarget(cmd, cameraData.renderer.cameraColorTarget);
#endif
            CoreUtils.DrawFullScreen(cmd, passMaterial);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
    }

    private class PassData {
        internal Material effectMaterial;
        internal bool requiresColor;
        internal bool isBeforeTransparents;
        public ProfilingSampler profilingSampler;
        public RTHandle copiedColor;
    }
}