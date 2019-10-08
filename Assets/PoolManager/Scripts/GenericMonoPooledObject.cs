using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class GenericMonoPooledObject : MonoBehaviour, IPooledObject
    {
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

        public IPool Pool { get; set; }
    }
}