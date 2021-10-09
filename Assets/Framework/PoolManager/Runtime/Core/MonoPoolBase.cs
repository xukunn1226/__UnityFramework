using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Cache
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
        public MonoPooledObject             PrefabAsset;

        [HideInInspector]
        internal bool                       ScriptDynamicAdded = false;     // MonoPooledObject是否是运行时添加的

        private Transform                   m_Group;

        public  Transform                   Group                           // 缓存对象回收时的挂载处（不仅限于回收时）
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

        public bool manualUnregisterPool { get; set; }                      // 是否手动从PoolManager释放，否则OnDestroy时自动释放
        public virtual void Init() {}

        public abstract int countAll { get; }

        public abstract int countOfUsed { get; }

        public abstract int countOfUnused { get; }

        public abstract IPooledObject  Get();

        public abstract void Return(IPooledObject item);

        public abstract void Clear();

        public abstract void Trim();

        /// <summary>
        /// prefab对象比较“重”，不建议warmup，但对于可预测的缓存对象可提前实例化
        /// </summary>
        // public virtual void Warmup() { }
    }
}