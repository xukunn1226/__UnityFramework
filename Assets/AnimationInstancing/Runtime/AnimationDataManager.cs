using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationDataManager : SingletonMono<AnimationDataManager>
    {
        class AnimationDataInst
        {
            public AnimationData    data;
            public int              refCount;
        }

        private Dictionary<AnimationData, AnimationDataInst> m_AnimationDatas = new Dictionary<AnimationData, AnimationDataInst>();

        public AnimationData Load(AnimationData prototype)
        {
            AnimationDataInst inst;
            if(!m_AnimationDatas.TryGetValue(prototype, out inst))
            {
                inst = new AnimationDataInst();
                inst.data = Instantiate<AnimationData>(prototype);
                inst.data.transform.parent = transform;

                m_AnimationDatas.Add(prototype, inst);
            }
            ++inst.refCount;
            return inst.data;
        }

        public void Unload(AnimationData prototype)
        {
            AnimationDataInst inst;
            if(!m_AnimationDatas.TryGetValue(prototype, out inst))
            {
                Debug.LogError($"AnimationDataManager.Unload: can't find the prototype {prototype}");
                return;
            }

            --inst.refCount;
            if(inst.refCount == 0)
            {
                m_AnimationDatas.Remove(prototype);
                Destroy(inst.data.gameObject);
            }
        }
    }
}