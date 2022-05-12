using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Universal
{
    static public class RenderingLayerMask
    {
        static private string[] renderingLayerMaskNames => UniversalRenderPipelineGlobalSettings.instance.renderingLayerMaskNames;

        static public uint GetRenderingLayerMask(params string[] layerNames)
        {
            uint ret = 0;
            foreach (var layerName in layerNames)
            {
                int index = NameToRenderingLayer(layerName);
                if (index != -1)
                    ret += (uint)(1 << index);
            }
            return ret;
        }

        static public string RenderingLayerToName(int layer)
        {
            if (layer < 0 || layer >= renderingLayerMaskNames.Length)
                return null;
            return renderingLayerMaskNames[layer];
        }

        // return the layer index, return -1 if not found
        static public int NameToRenderingLayer(string layerName)
        {
            for (int i = 0; i < renderingLayerMaskNames.Length; ++i)
            {
                string name = renderingLayerMaskNames[i];
                if (!string.IsNullOrEmpty(name) && name == layerName)
                    return i;
            }
            return -1;
        }
    }
}