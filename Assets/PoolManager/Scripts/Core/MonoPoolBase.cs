using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 对象缓存池
    /// </summary>
    public abstract class MonoPoolBase : MonoBehaviour, IPool
    {
        public  MonoPooledObjectBase        PrefabAsset;                // 缓存对象

        private Transform                   m_Group;                    // 缓存对象挂载处
        public  Transform                   Group
        {
            get
            {
                if(m_Group == null)
                {
                    m_Group = transform;
                }
                return m_Group;
            }
            set
            {
                m_Group = value;
            }
        }

        protected   abstract void           Warmup();

        public      abstract IPooledObject  Get();

        public      abstract void           Return(IPooledObject item);

        public      abstract void           TrimExcess();

        public      abstract void           Clear();
    }
}