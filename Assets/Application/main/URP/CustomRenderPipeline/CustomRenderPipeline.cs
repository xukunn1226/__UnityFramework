using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Application.Runtime
{
    public class CustomRenderPipeline : RenderPipeline
    {
        private CustomRenderPipelineAsset m_Asset;

        public CustomRenderPipeline(CustomRenderPipelineAsset asset)
        {
            m_Asset = asset;
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            var cmd = CommandBufferPool.Get();
            cmd.ClearRenderTarget(true, true, Color.red);
            context.ExecuteCommandBuffer(cmd);
            cmd.Release();

            // ������ʾScene��ͼ��SceneCamera�����Cameraʱ��ʾPreview��ͼ��PreviewCamera���Լ�������������ӵ�Camera
            foreach (Camera camera in cameras)
            {
                //��ȡ��ǰ������޳����򣬽����޳�
                camera.TryGetCullingParameters(out var cullingParameters);
                cullingParameters.cullingOptions |= CullingOptions.None;
                //cullingParameters.cullingMask = 
                var cullingResults = context.Cull(ref cullingParameters);

                //���ݵ�ǰCamera����������Shader�ı���
                context.SetupCameraProperties(camera);

                //����DrawingSettings
                ShaderTagId shaderTagId = new ShaderTagId("UniversalForward");
                var sortingSettings = new SortingSettings(camera);
                DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
                drawingSettings.SetShaderPassName(1, new ShaderTagId("CustomLightModeTag"));

                //����FilteringSettings
                FilteringSettings filteringSettings = FilteringSettings.defaultValue;

                context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

                if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
                {
                    //������պ�
                    context.DrawSkybox(camera);
                }
                context.Submit();
            }
        }
    }
}