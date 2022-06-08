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

            // 会有显示Scene视图的SceneCamera，点击Camera时显示Preview视图的PreviewCamera，以及场景中我们添加的Camera
            foreach (Camera camera in cameras)
            {
                //获取当前相机的剔除规则，进行剔除
                camera.TryGetCullingParameters(out var cullingParameters);
                cullingParameters.cullingOptions |= CullingOptions.None;
                //cullingParameters.cullingMask = 
                var cullingResults = context.Cull(ref cullingParameters);

                //根据当前Camera，更新内置Shader的变量
                context.SetupCameraProperties(camera);

                //生成DrawingSettings
                ShaderTagId shaderTagId = new ShaderTagId("UniversalForward");
                var sortingSettings = new SortingSettings(camera);
                DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
                drawingSettings.SetShaderPassName(1, new ShaderTagId("CustomLightModeTag"));

                //生成FilteringSettings
                FilteringSettings filteringSettings = FilteringSettings.defaultValue;

                context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

                if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
                {
                    //绘制天空盒
                    context.DrawSkybox(camera);
                }
                context.Submit();
            }
        }
    }
}