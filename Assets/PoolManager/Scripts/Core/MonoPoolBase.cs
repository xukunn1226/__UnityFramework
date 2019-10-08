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
        public MonoPooledObjectBase     PrefabAsset;                // 缓存对象

        private Transform               m_Pivot;                    // 缓存对象挂载处
        public Transform                Pivot
        {
            get
            {
                if(m_Pivot == null)
                {
                    m_Pivot = transform;
                }
                return m_Pivot;
            }
            set
            {
                m_Pivot = value;
            }
        }

        public abstract IPooledObject Get();

        public abstract void Return(IPooledObject item);
    }
}