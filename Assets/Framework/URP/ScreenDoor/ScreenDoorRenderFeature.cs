//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Rendering;
//using UnityEngine.Rendering.Universal;

//namespace Framework.Core
//{
//    public class ScreenDoorRenderFeature : ScriptableRendererFeature
//    {
//        [System.Serializable]
//        public class Settings
//        {
//            public RenderPassEvent  RenderPassEvent         = RenderPassEvent.AfterRenderingTransparents;
//            public LayerMask        layerMask               = -1;   // 筛选条件1
//            [Range(1, 32)]
//            public int              renderingLayerMask;             // 筛选条件2
//            public int              overrideMaterialPassIndex;
//            [Range(0, 1)]
//            public float            transparency = 1;               // 透明度
//        }

//        public Settings             m_Settings              = new Settings();
//        private ScreenDoorPass      m_RenderPass;
//        private Material            m_Material;
//        const string                k_ScreenDoorShader      = "Hidden/MyURP/ScreenDoor";

//        public override void Create()
//        {
//            m_Material = CoreUtils.CreateEngineMaterial(Shader.Find(k_ScreenDoorShader));
//            m_RenderPass = new ScreenDoorPass(m_Material, m_Settings);
//            m_RenderPass.renderPassEvent = m_Settings.RenderPassEvent;            
//        }

//        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//        {
//            if(!renderingData.cameraData.isSceneViewCamera)
//            {
//                m_RenderPass.SetUp(renderer.cameraColorTarget);
//                renderer.EnqueuePass(m_RenderPass);
//            }
//        }
//    }

//    public class ScreenDoorPass : ScriptableRenderPass
//    {
//        private Material                            m_Material;
//        private ScreenDoorRenderFeature.Settings    m_Settings;
//        private RenderTargetIdentifier              m_CameraColorTexture;

//        public ScreenDoorPass(Material material, ScreenDoorRenderFeature.Settings settings)
//        {
//            profilingSampler = new ProfilingSampler(nameof(ScreenDoorPass));
//            m_Settings = settings;
//            m_Material = material;
//        }

//        public void SetUp(RenderTargetIdentifier destination)
//        {
//            m_CameraColorTexture = destination;
//        }

//        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//        {
//            CommandBuffer cmd = CommandBufferPool.Get("ScreenDoor");

//            var drawingSettings = CreateDrawingSettings(new ShaderTagId("UniversalForward"), ref renderingData, SortingCriteria.CommonOpaque);
//            drawingSettings.overrideMaterial = m_Material;
//            drawingSettings.overrideMaterialPassIndex = m_Settings.overrideMaterialPassIndex;
//            FilteringSettings fs = new FilteringSettings(RenderQueueRange.opaque, m_Settings.layerMask, ((uint) 1 << m_Settings.renderingLayerMask));
//            context.DrawRenderers(renderingData.cullResults, 
//                                  ref drawingSettings,
//                                  ref fs);
//            context.ExecuteCommandBuffer(cmd);
//            CommandBufferPool.Release(cmd);
//        }
//    }
//}