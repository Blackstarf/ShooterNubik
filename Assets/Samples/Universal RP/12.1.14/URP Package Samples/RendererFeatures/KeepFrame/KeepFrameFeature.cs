using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class KeepFrameFeature : ScriptableRendererFeature
{
    class CopyFramePass : ScriptableRenderPass
    {
        private RTHandle m_Source;
        private RTHandle m_Destination;

        public void Setup(RTHandle source, RTHandle destination)
        {
            m_Source = source;
            m_Destination = destination;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(m_Destination);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.camera.cameraType != CameraType.Game)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("CopyFramePass");
            Blit(cmd, m_Source, m_Destination);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (m_Destination != null && !m_Destination.Equals(m_Source))
            {
                m_Destination.Release();
            }
        }
    }

    class DrawOldFramePass : ScriptableRenderPass
    {
        private Material m_DrawOldFrameMaterial;
        private RTHandle m_Handle;
        private string m_TextureName;

        public void Setup(Material drawOldFrameMaterial, RTHandle handle, string textureName)
        {
            m_DrawOldFrameMaterial = drawOldFrameMaterial;
            m_Handle = handle;
            m_TextureName = textureName;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;

            RenderingUtils.ReAllocateIfNeeded(ref m_Handle, descriptor, FilterMode.Bilinear,
                TextureWrapMode.Clamp, name: m_TextureName);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_DrawOldFrameMaterial == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("DrawOldFramePass");
            cmd.SetGlobalTexture(m_TextureName, m_Handle);
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_DrawOldFrameMaterial, 0, 0);
            cmd.SetViewProjectionMatrices(renderingData.cameraData.camera.worldToCameraMatrix, renderingData.cameraData.camera.projectionMatrix);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (m_Handle != null)
            {
                m_Handle.Release();
            }
        }
    }

    [Serializable]
    public class Settings
    {
        public Material displayMaterial;
        public string textureName = "_FrameCopyTex";
    }

    private CopyFramePass m_CopyFrame;
    private DrawOldFramePass m_DrawOldFrame;
    private RTHandle m_OldFrameHandle;
    public Settings settings = new Settings();

    public override void Create()
    {
        m_CopyFrame = new CopyFramePass();
        m_CopyFrame.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        m_DrawOldFrame = new DrawOldFramePass();
        m_DrawOldFrame.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;

        m_OldFrameHandle = RTHandles.Alloc(
            width: 1,
            height: 1,
            colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
            name: "_OldFrameRenderTarget");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.displayMaterial == null)
        {
            Debug.LogWarning("Display Material is not set in KeepFrameFeature");
            return;
        }

        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        desc.msaaSamples = 1;

        RenderingUtils.ReAllocateIfNeeded(ref m_OldFrameHandle, desc, FilterMode.Bilinear,
            TextureWrapMode.Clamp, name: "_OldFrameRenderTarget");

        m_CopyFrame.Setup(renderer.cameraColorTargetHandle, m_OldFrameHandle);
        renderer.EnqueuePass(m_CopyFrame);

        m_DrawOldFrame.Setup(
            settings.displayMaterial,
            m_OldFrameHandle,
            string.IsNullOrEmpty(settings.textureName) ? "_FrameCopyTex" : settings.textureName);
        renderer.EnqueuePass(m_DrawOldFrame);
    }

    protected override void Dispose(bool disposing)
    {
        m_OldFrameHandle?.Release();
        m_OldFrameHandle = null;
    }
}