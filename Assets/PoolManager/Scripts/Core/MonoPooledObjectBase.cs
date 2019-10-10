using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class MonoPooledObjectBase : MonoBehaviour, IPooledObject
    {
        private IPool m_Pool;

        public IPool Pool
        {
            get
            {
                if(m_Pool == null)
                {
                    m_Pool = PoolManager.GetOrCreatePool(this);
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
            throw new System.NotImplementedException("GenericMonoPooledObject:OnInit not implemente");
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
                Destroy(this);
            }
        }
    }
}