using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Framework.Core
{
    public class GaussianBlurRendererFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent  RenderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            [Range(0.2f, 3.0f)]
            public float            BlurSize        = 1;
            [Range(1, 8)]
            public int              DownSample      = 2;
        }
        public Settings             m_Settings      = new Settings();
        private GaussianBlurPass    m_RenderPass;
        private Material            m_BlurMaterial;
        private int                 m_DownSample;
        private float               m_BlurSize;
        const string                k_BlurShader    = "Hidden/CRP/GaussianBlur";

        public override void Create()
        {
            m_BlurMaterial = CoreUtils.CreateEngineMaterial(Shader.Find(k_BlurShader));
            m_DownSample = m_Settings.DownSample;
            m_BlurSize = m_Settings.BlurSize;

            m_RenderPass = new GaussianBlurPass(m_BlurMaterial, m_DownSample, m_BlurSize);
            m_RenderPass.renderPassEvent = m_Settings.RenderPassEvent;
        }


        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(!renderingData.cameraData.isSceneViewCamera)
            {
                m_RenderPass.SetUp(renderer.cameraColorTarget);         // _CameraColorTexture
                renderer.EnqueuePass(m_RenderPass);
            }
        }

        void Update()
        {
            if(m_RenderPass != null)
            {
                if(m_DownSample != m_Settings.DownSample || m_BlurSize != m_Settings.BlurSize)
                {
                    m_DownSample = m_Settings.DownSample;
                    m_BlurSize = m_Settings.BlurSize;
                    m_RenderPass.UpdateBlurParams(m_DownSample, m_BlurSize);
                }
            }
        }
    }

    public class GaussianBlurPass : ScriptableRenderPass
    {
        private Material                m_BlurMaterial;
        private int                     m_DownSample;
        private float                   m_BlurSize;
        private RenderTextureDescriptor m_OpaqueDesc;
        private RenderTargetIdentifier  m_CameraColorTexture;

        public GaussianBlurPass(Material blurMaterial, int downSample, float blurSize)
        {
            profilingSampler = new ProfilingSampler(nameof(GaussianBlurPass));
            m_BlurMaterial = blurMaterial;
            m_DownSample = downSample;
            m_BlurSize = blurSize;
        }

        public void SetUp(RenderTargetIdentifier destination)
        {
            m_CameraColorTexture = destination;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cameraTextureDescriptor.msaaSamples = 1;
            m_OpaqueDesc = cameraTextureDescriptor;
        }

        public void UpdateBlurParams(int downSample, float blurSize)
        {
            m_DownSample = downSample;
            m_BlurSize = blurSize;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope (cmd, profilingSampler))
            {
                // down sample
                m_OpaqueDesc.width /= m_DownSample;
                m_OpaqueDesc.height /= m_DownSample;

                int blurredRT0 = Shader.PropertyToID("_BlurredRT0");
                int blurredRT1 = Shader.PropertyToID("_BlurredRT1");
                cmd.GetTemporaryRT(blurredRT0, m_OpaqueDesc, FilterMode.Bilinear);
                cmd.GetTemporaryRT(blurredRT1, m_OpaqueDesc, FilterMode.Bilinear);

                // color texture blit to rt0
                cmd.Blit(m_CameraColorTexture, blurredRT0);

                // render the vertical pass
                m_BlurMaterial.SetFloat("_BlurSize", m_BlurSize);
                cmd.Blit(blurredRT0, blurredRT1, m_BlurMaterial, 0);

                // render the horizontal pass
                cmd.Blit(blurredRT1, blurredRT0, m_BlurMaterial, 1);

                // finally blit to color texture
                cmd.Blit(blurredRT0, m_CameraColorTexture);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}