using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cache
{
    /// <summary>
    /// prefab对象缓存池
    /// 相比IPool多了PrefabAsset、Group、Warmup
    /// </summary>
    public abstract class MonoPoolBase : MonoBehaviour, IPool
    {
        /// <summary>
        /// 缓存对象原型(proto type)
        /// 注意：PrefabAsset是从AB加载出来的asset，尚未实例化
        /// </summary>
        public MonoPooledObjectBase         PrefabAsset;

        public bool                         ScriptDynamicAdded = true;      // MonoPooledObjectBase是否是运行时Add

        private Transform                   m_Group;

        public  Transform                   Group                       // 缓存对象回收时的挂载处（不仅限于回收时）
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

        public abstract int countAll { get; }

        public abstract int countActive { get; }

        public abstract int countInactive { get; }

        public abstract IPooledObject  Get();

        public abstract void Return(IPooledObject item);

        public abstract void Clear();

        public abstract void Trim();

        /// <summary>
        /// prefab对象比较“重”，不建议warmup，但对于可预测的缓存对象可提前实例化
        /// </summary>
        public virtual void Warmup() { }
    }
}