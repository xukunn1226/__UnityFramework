
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MyBlurRenderPass : ScriptableRendererFeature
{
    [Serializable]
    public class Settings
    {
        public RenderPassEvent m_renderPassEvent;
    }

    const string k_BlurShader = "Hidden/Blur";
    private Material m_BlurMaterial;

    public Settings settings;

    private MyGrabPassImpl m_grabPass;

    public override void Create()
    {
        m_grabPass = new MyGrabPassImpl(m_BlurMaterial);
        m_grabPass.renderPassEvent = settings.m_renderPassEvent;
        m_BlurMaterial = CoreUtils.CreateEngineMaterial(Shader.Find(k_BlurShader));
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!renderingData.cameraData.isSceneViewCamera)
        {
            m_grabPass.SetUp(renderer.cameraColorTarget);// 颜色RT _CameraColorTexture
            renderer.EnqueuePass(m_grabPass);
        }
    }
}


public class MyGrabPassImpl : ScriptableRenderPass
{
    private Material m_BlurMaterial;
    private MyBlur m_MyBlur;
    private RenderTextureDescriptor m_OpaqueDesc;
    private RenderTargetIdentifier m_CamerColorTexture;

    public MyGrabPassImpl(Material blurMaterial)
    {
        base.profilingSampler = new ProfilingSampler(nameof(GrabPassImpl));
        m_BlurMaterial = blurMaterial;
        
    }

    public void SetUp(RenderTargetIdentifier destination)
    {
        m_CamerColorTexture = destination;
    }


    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        cameraTextureDescriptor.msaaSamples = 1;
        m_OpaqueDesc = cameraTextureDescriptor;
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var stack = VolumeManager.instance.stack;
        m_MyBlur = stack.GetComponent<MyBlur>();
        if (m_MyBlur != null && m_MyBlur.active)
        {
            var x = m_MyBlur.blurAmountX.value;
            var y = m_MyBlur.blurAmountY.value;
            CommandBuffer cmd = CommandBufferPool.Get();
            #region Blur 操作
            using (new ProfilingScope(cmd, profilingSampler))
            {
                //降低分辨率
                m_OpaqueDesc.width /= 4;
                m_OpaqueDesc.height /= 4;

                int blurredID = Shader.PropertyToID("_BlurRT1");
                int blurredID2 = Shader.PropertyToID("_BlurRT2");
                cmd.GetTemporaryRT(blurredID, m_OpaqueDesc, FilterMode.Bilinear);
                cmd.GetTemporaryRT(blurredID2, m_OpaqueDesc, FilterMode.Bilinear);
                //颜色RT Blit到临时RT中
                cmd.Blit(m_CamerColorTexture, blurredID);
                //横向纵向做Blur模糊
                cmd.SetGlobalVector("offsets", new Vector4(x / Screen.width, 0, 0, 0));
                cmd.Blit(blurredID, blurredID2, m_BlurMaterial);
                cmd.SetGlobalVector("offsets", new Vector4(0, y / Screen.height, 0, 0));
                cmd.Blit(blurredID2, blurredID, m_BlurMaterial);
                cmd.SetGlobalVector("offsets", new Vector4(x * 2 / Screen.width, 0, 0, 0));
                cmd.Blit(blurredID, blurredID2, m_BlurMaterial);
                cmd.SetGlobalVector("offsets", new Vector4(0, y * 2 / Screen.height, 0, 0));
                cmd.Blit(blurredID2, blurredID, m_BlurMaterial);
                //最后在把临时RT Blit回颜色RT
                cmd.Blit(blurredID, m_CamerColorTexture);
            }
            #endregion
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}