using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Application.Runtime
{
    [CreateAssetMenu(menuName = "Rendering/CustomRenderPipelineAsset")]
    public class CustomRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new CustomRenderPipeline(this);
        }
    }
}