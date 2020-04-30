using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Cache
{
    public class LRUPool : MonoBehaviour, IPool
    {
        [Range(1, 100)]
        public int Capacity;

        int IPool.countAll { get; }

        int IPool.countOfUsed { get; }

        int IPool.countOfUnused { get; }

        public IPooledObject Get() { return null; }

        public void Return(IPooledObject item) { }

        public void Clear()
        {
        }

        void IPool.Trim() { }

        private void Awake()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private void Start()
        {
            // 
            if (PoolManager.Instance.gameObject != transform.root.gameObject)
            {
                transform.parent = PoolManager.Instance.transform;
            }
        }
        
        private void OnDestroy()
        {
            //if (!manualUnregisterPool)
            //    PoolManager.RemoveMonoPool(this);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LRUPool))]
    public class LRUPoolEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
#endif
}