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
        /// <summary>
        /// 不提供默认对象池，防止资源错误的绑定此脚本会导致回收时创建一个新的Pool
        /// </summary>
        public virtual IPool Pool
        {
            get;
            set;
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