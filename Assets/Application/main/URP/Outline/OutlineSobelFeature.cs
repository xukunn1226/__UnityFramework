using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineSobelFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        public Settings                 settings;
        private RenderTargetIdentifier  m_SourceRT;
        private RenderTargetIdentifier  m_TargetRT;
        private int                     m_TargetId = Shader.PropertyToID("_TempSobelRT");

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            blitTargetDescriptor.depthBufferBits = 0;

            m_SourceRT = renderingData.cameraData.renderer.cameraColorTarget;
            cmd.GetTemporaryRT(m_TargetId, blitTargetDescriptor, FilterMode.Point);
            m_TargetRT = new RenderTargetIdentifier(m_TargetId);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Outline_Sobel");
            Blit(cmd, m_SourceRT, m_TargetRT, settings.blitMaterial, 0);
            Blit(cmd, m_TargetRT, m_SourceRT);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (m_TargetId != -1)
                cmd.ReleaseTemporaryRT(m_TargetId);
        }
    }

    CustomRenderPass m_ScriptablePass;

    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

        public Material blitMaterial = null;
    }
    public Settings settings = new Settings();

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();        
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.blitMaterial == null)
        {
            Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }

        m_ScriptablePass.renderPassEvent = settings.renderPassEvent;
        m_ScriptablePass.settings = settings;
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


