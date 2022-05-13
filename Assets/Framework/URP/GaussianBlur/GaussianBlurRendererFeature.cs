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
            [Range(0.2f, 8.0f)][Tooltip("值越大，模糊程度越高，不影响性能")]
            public float            BlurSize        = 1;        // 值越大，模糊程度越高，不影响性能
            [Range(1, 8)][Tooltip("值越大，性能越好，但容易图像虚化")]
            public int              DownSample      = 2;        // 值越大，性能越好，但容易图像虚化
            [Range(1, 4)][Tooltip("值越大，效果越好，同时会严重影响性能")]
            public int              Iteration       = 1;
        }
        public Settings             m_Settings      = new Settings();
        private GaussianBlurPass    m_RenderPass;
        private Material            m_BlurMaterial;
        const string                k_BlurShader    = "Hidden/MyURP/GaussianBlur";

        public override void Create()
        {
            m_BlurMaterial = CoreUtils.CreateEngineMaterial(Shader.Find(k_BlurShader));

            m_RenderPass = new GaussianBlurPass(m_BlurMaterial, m_Settings);
            m_RenderPass.renderPassEvent = m_Settings.RenderPassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(!renderingData.cameraData.isSceneViewCamera)
            {
                renderer.EnqueuePass(m_RenderPass);
            }
        }
    }

    public class GaussianBlurPass : ScriptableRenderPass
    {
        private Material                                m_BlurMaterial;
        private GaussianBlurRendererFeature.Settings    m_Settings;
        private RenderTextureDescriptor                 m_OpaqueDesc;
        private RenderTargetIdentifier                  m_CameraColorTexture;       // 某阶段的color texture，例如_CameraColorAttachment

        public GaussianBlurPass(Material blurMaterial, GaussianBlurRendererFeature.Settings settings)
        {
            profilingSampler = new ProfilingSampler(nameof(GaussianBlurPass));
            m_BlurMaterial = blurMaterial;
            m_Settings = settings;
        }

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // _CameraColorAttachmentA
            m_CameraColorTexture = renderingData.cameraData.renderer.cameraColorTarget;
        }

        /// <summary>
        /// This method is called by the renderer before executing the render pass.
        /// Override this method if you need to to configure render targets and their clear state, and to create temporary render target textures.
        /// If a render pass doesn't override this method, this render pass renders to the active Camera's render target.
        /// You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        /// </summary>
        /// <param name="cmd">CommandBuffer to enqueue rendering commands. This will be executed by the pipeline.</param>
        /// <param name="cameraTextureDescriptor">Render texture descriptor of the camera render target.</param>
        /// <seealso cref="ConfigureTarget"/>
        /// <seealso cref="ConfigureClear"/>
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cameraTextureDescriptor.msaaSamples = 1;
            m_OpaqueDesc = cameraTextureDescriptor;
            m_OpaqueDesc.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None;       // 优化带宽，不需要depth buffer
        }

        /// <summary>
        /// Called upon finish rendering a camera. You can use this callback to release any resources created
        /// by this render
        /// pass that need to be cleanup once camera has finished rendering.
        /// This method be called for all cameras in a camera stack.
        /// </summary>
        /// <param name="cmd">Use this CommandBuffer to cleanup any generated data</param>
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        /// <summary>
        /// Called upon finish rendering a camera stack. You can use this callback to release any resources created
        /// by this render pass that need to be cleanup once all cameras in the stack have finished rendering.
        /// This method will be called once after rendering the last camera in the camera stack.
        /// Cameras that don't have an explicit camera stack are also considered stacked rendering.
        /// In that case the Base camera is the first and last camera in the stack.
        /// </summary>
        /// <param name="cmd">Use this CommandBuffer to cleanup any generated data</param>
        public override void OnFinishCameraStackRendering(CommandBuffer cmd)
        { }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope (cmd, profilingSampler))
            {
                // down sample
                m_OpaqueDesc.width /= m_Settings.DownSample;
                m_OpaqueDesc.height /= m_Settings.DownSample;

                int blurredRT0 = Shader.PropertyToID("_BlurredRT0");
                int blurredRT1 = Shader.PropertyToID("_BlurredRT1");
                cmd.GetTemporaryRT(blurredRT0, m_OpaqueDesc, FilterMode.Bilinear);
                cmd.GetTemporaryRT(blurredRT1, m_OpaqueDesc, FilterMode.Bilinear);

                // color texture blit to rt0
                cmd.Blit(m_CameraColorTexture, blurredRT0);

                for(int i = 0; i < m_Settings.Iteration; ++i)
                {
                    cmd.SetGlobalFloat("_BlurSize", (1.0f + i) * m_Settings.BlurSize);

                    // render the vertical pass
                    cmd.Blit(blurredRT0, blurredRT1, m_BlurMaterial, 0);

                    // render the horizontal pass
                    cmd.Blit(blurredRT1, blurredRT0, m_BlurMaterial, 1);
                }

                // finally blit to color texture
                cmd.Blit(blurredRT0, m_CameraColorTexture);
                cmd.ReleaseTemporaryRT(blurredRT0);
                cmd.ReleaseTemporaryRT(blurredRT1);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}