using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationInstancing : MonoBehaviour
    {
        public TextAsset                manifest;
        public Texture2D                animationTexture;
        public List<RendererCache>      rendererCacheList = new List<RendererCache>();
    }
}