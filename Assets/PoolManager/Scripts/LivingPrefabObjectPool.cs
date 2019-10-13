using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 具有生命周期Prefab的缓存池
    /// </summary>
    public class LivingPrefabObjectPool : MonoPoolBase
    {
        private LinkedList<LivingMonoPooledObject> m_DeactiveObjects = new LinkedList<LivingMonoPooledObject>();

        public int countAll { get; private set; }

        public int countActive { get { return countAll - countInactive; } }

        public int countInactive { get { return m_DeactiveObjects.Count; } }

        public override IPooledObject Get()
        {
            return null;
        }

        public override void Return(IPooledObject item)
        {

        }

        public override void Clear()
        { }
    }
}