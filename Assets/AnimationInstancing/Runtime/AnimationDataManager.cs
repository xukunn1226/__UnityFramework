using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationDataManager : SingletonMono<AnimationDataManager>
    {
        private Dictionary<AnimationData, AnimationData> m_AnimationDatas = new Dictionary<AnimationData, AnimationData>();

        public AnimationData Load(AnimationData prototype)
        {
            AnimationData inst;
            if(!m_AnimationDatas.TryGetValue(prototype, out inst))
            {
                inst = Instantiate<AnimationData>(prototype);
                m_AnimationDatas.Add(prototype, inst);
                inst.transform.parent = transform;
            }
            return inst;
        }
    }
}