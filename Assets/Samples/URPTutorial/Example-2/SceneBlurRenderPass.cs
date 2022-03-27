
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SceneBlurRenderPass : ScriptableRendererFeature
{
    [Serializable]
    public class Settings
    {
        public RenderPassEvent m_renderPassEvent;
        public Vector2 m_BlurAmount;
    }

    const string k_BlurShader = "Hidden/Blur";
    private Material m_BlurMaterial;

    private Vector2 currentBlurAmount;

    public Settings settings;

    private GrabPassImpl m_grabPass;

    public override void Create()
    {
        m_grabPass = new GrabPassImpl(m_BlurMaterial, currentBlurAmount);
        m_grabPass.renderPassEvent = settings.m_renderPassEvent;
        m_BlurMaterial = CoreUtils.CreateEngineMaterial(Shader.Find(k_BlurShader));
        currentBlurAmount = settings.m_BlurAmount;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!renderingData.cameraData.isSceneViewCamera)
        {
            m_grabPass.SetUp(renderer.cameraColorTarget);// 颜色RT _CameraColorTexture
            renderer.EnqueuePass(m_grabPass);
        }
    }

    void Update()
    {
        if(m_grabPass != null)
        {
            if(currentBlurAmount != settings.m_BlurAmount)
            {
                currentBlurAmount = settings.m_BlurAmount;
                m_grabPass.UpdateBlurAmount(currentBlurAmount);
            }
        } 
    }
}


public class GrabPassImpl : ScriptableRenderPass
{
    private Material m_BlurMaterial;
    private Vector2 m_BlurAmount;

    private RenderTextureDescriptor m_OpaqueDesc;
    private RenderTargetIdentifier m_CamerColorTexture;

    public GrabPassImpl(Material blurMaterial, Vector2 blurAmount)
    {
        base.profilingSampler = new ProfilingSampler(nameof(GrabPassImpl));
        m_BlurMaterial = blurMaterial;
        m_BlurAmount = blurAmount;
        
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

    public void UpdateBlurAmount(Vector2 newBlurAmount)
    {
        m_BlurAmount = newBlurAmount;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope (cmd, profilingSampler))
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
            cmd.SetGlobalVector("offsets", new Vector4(m_BlurAmount.x / Screen.width, 0, 0, 0));
            cmd.Blit(blurredID, blurredID2, m_BlurMaterial);
            cmd.SetGlobalVector("offsets", new Vector4(0, m_BlurAmount.y / Screen.height, 0, 0));
            cmd.Blit(blurredID2, blurredID, m_BlurMaterial);
            cmd.SetGlobalVector("offsets", new Vector4(m_BlurAmount.x * 2 / Screen.width, 0, 0, 0));
            cmd.Blit(blurredID, blurredID2, m_BlurMaterial);
            cmd.SetGlobalVector("offsets", new Vector4(0, m_BlurAmount.y * 2 / Screen.height, 0, 0));
            cmd.Blit(blurredID2, blurredID, m_BlurMaterial);
            //最后在把临时RT Blit回颜色RT
            cmd.Blit(blurredID, m_CamerColorTexture);
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}