using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Cache
{
    public abstract class LRUPoolBase : MonoBehaviour, IPool
    {
        [Range(1, 100)]
        public int          Capacity = 1;

        public bool         GroupByPoolManager = true;

        public int          countAll        { get { return Capacity; } }
        public abstract int countOfUsed     { get; }
        public int          countOfUnused   { get; }

        public abstract void Clear();

        public IPooledObject Get() { throw new System.NotImplementedException(); }

        public abstract IPooledObject Get(string assetPath);

        public abstract void Return(IPooledObject obj);

        public void Trim() { }

        protected abstract void InitLRU();

        protected abstract void UninitLRU();

        private void Awake()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            InitLRU();

#if UNITY_EDITOR
            gameObject.name = string.Format($"[LRUPool]{gameObject.name}");
#endif
        }

        private void Start()
        {
            if (GroupByPoolManager)
            {
                transform.parent = PoolManager.Instance.transform;
            }
        }

        private void OnDestroy()
        {
            UninitLRU();
        }
    }
}