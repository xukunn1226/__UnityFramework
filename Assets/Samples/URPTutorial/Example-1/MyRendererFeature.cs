using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static MyRendererFeature;

public class MyRendererFeature : ScriptableRendererFeature
{

    public Settings settings = new Settings();
    private MyRendererPass m_MyRenderObjectsPass;

    public override void Create()
    {
        m_MyRenderObjectsPass = new MyRendererPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_MyRenderObjectsPass);
    }

    [System.Serializable]
    public class Settings
    {
        public Mesh mesh ;
        public Material material;
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;
    }
}


public class MyRendererPass : ScriptableRenderPass
{
    private Settings m_Setting;
    public MyRendererPass(Settings settings)
    {
        base.profilingSampler = new ProfilingSampler(nameof(MyRendererPass));
        m_Setting = settings;
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, profilingSampler))
        {
            //cmd.draw......调用绘制接口
            cmd.DrawMesh(m_Setting.mesh, Matrix4x4.identity, m_Setting.material);

            /*
            cmd.DrawMeshInstanced()
            cmd.DrawMeshInstancedIndirect();
            cmd.DrawMeshInstancedProcedural();
            cmd.DrawOcclusionMesh();
            cmd.DrawProceduralIndirect();
            cmd.DrawRenderer();
            */
            //注意
            //context.DrawRenderers()
        }
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }
}