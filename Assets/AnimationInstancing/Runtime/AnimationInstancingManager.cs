using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationInstancingManager : SingletonMono<AnimationDataManager>
    {
        private Dictionary<int, VertexCache>    m_VertexCachePool;              // 

        public void AddInstance(AnimationInstancing inst)
        {

        }

        public void RemoveInstance(AnimationInstancing inst)
        {

        }
    }
}