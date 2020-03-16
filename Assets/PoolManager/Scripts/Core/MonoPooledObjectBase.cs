using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 基于Mono的可缓存对象基类
    /// </summary>
    public abstract class MonoPooledObjectBase : MonoBehaviour, IPooledObject
    {
        protected IPool m_Pool;

        /// <summary>
        /// 获取缓存池
        /// </summary>
        public virtual IPool Pool
        {
            get
            {
                if(m_Pool == null)
                { // create the default pool named "PrefabObjectPool", you can override it
                    m_Pool = PoolManager.GetOrCreatePool<PrefabObjectPool>(this);
                }
                return m_Pool;
            }
            set
            {
                m_Pool = value;
            }
        }

        public void OnInit()
        {
            throw new System.NotImplementedException("MonoPooledObjectBase:OnInit not implemente");
        }

        public virtual void OnGet()
        {
            gameObject.SetActive(true);
        }

        public virtual void OnRelease()
        {
            gameObject.SetActive(false);
        }

        public void ReturnToPool()
        {
            if(Pool != null)
            {
                Pool.Return(this);
            }
            else
            {
                Debug.LogWarning("Discard the PooledObject, because of Pool == null");
                PoolManager.Destroy(gameObject);
            }
        }
    }
}