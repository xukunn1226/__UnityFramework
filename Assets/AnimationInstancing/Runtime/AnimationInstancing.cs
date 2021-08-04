using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationInstancing : MonoBehaviour
    {
        public AnimationData            prototype;
        public List<RendererCache>      rendererCacheList = new List<RendererCache>();
    }
}