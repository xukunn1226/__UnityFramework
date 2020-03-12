using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 对象缓存池
    /// prefab对象的缓存池
    /// </summary>
    public abstract class MonoPoolBase : MonoBehaviour, IPool
    {
        public MonoPooledObjectBase         PrefabAsset { get; set; }   // 缓存对象原型(proto type)

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

        public      abstract IPooledObject  Get();

        public      abstract void           Return(IPooledObject item);

        public      abstract void           Clear();

        /// <summary>
        /// prefab对象比较“重”，不建议warmup，但对于可预测的缓存对象可提前实例化
        /// </summary>
        protected   virtual void            Warmup() { }
    }
}