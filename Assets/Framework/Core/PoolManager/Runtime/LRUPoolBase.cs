using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Cache
{
    public abstract class LRUPoolBase : MonoBehaviour
    {
        [Range(1, 100)]
        public int Capacity = 1;

        public bool GroupByPoolManager = true;

        public int countAll { get { return Capacity; } }
        public abstract int countOfUsed { get; }

        public abstract void Clear();

        protected abstract void InitLRU();

        protected abstract void UninitLRU();

        private void Awake()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            InitLRU();

            if(GroupByPoolManager)
            {
                transform.parent = PoolManager.Instance.transform;
            }

#if UNITY_EDITOR
            gameObject.name = string.Format($"[LRUPool]{gameObject.name}");
#endif
        }

        private void OnDestroy()
        {
            UninitLRU();
        }
    }
}