using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Tests
{
    public class LRUPool_UI : LRUPoolBase
    {
        static private LRUQueue<string, UILRUPooledObject> m_Pool;

        private Dictionary<string, UILRUPooledObject> m_UIPrefabs = new Dictionary<string, UILRUPooledObject>();            // 记录在LRU中的UI

        public override int countOfUsed { get { return m_Pool.Count; } }

        public override void Clear()
        {
            m_Pool?.Clear();
        }

        protected override void InitLRU()
        {
            if (m_Pool != null)
                throw new System.Exception("m_UIPrefabs != null");

            m_Pool = new LRUQueue<string, UILRUPooledObject>(Capacity);        // 自动注册到PoolManager
            m_Pool.OnDiscard += OnDiscard;
        }

        protected override void UninitLRU()
        {
            if (m_Pool == null)
                throw new System.ArgumentNullException("m_UIPrefabs");

            m_Pool.OnDiscard -= OnDiscard;
            PoolManager.RemoveObjectPool(typeof(UILRUPooledObject));
        }




        public override IPooledObject Get(string assetPath)
        {
            UILRUPooledObject ui = m_Pool.Exist(assetPath);
            if (ui == null)
            {
                ui = InstantiateUIPrefab(assetPath);
            }
            ui.OnTouch();
            m_Pool.Cache(assetPath, ui);

            if(m_UIPrefabs.ContainsKey(assetPath))
            {
                m_UIPrefabs.Add(assetPath, ui);
            }
            return ui;
        }

        public override IPooledObject Get(string bundleName, string assetName)
        {
            UILRUPooledObject ui = m_Pool.Exist(assetName);
            if (ui == null)
            {
                ui = InstantiateUIPrefab(bundleName, assetName);
            }
            ui.OnTouch();
            m_Pool.Cache(assetName, ui);

            if(m_UIPrefabs.ContainsKey(assetName))
            {
                m_UIPrefabs.Add(assetName, ui);
            }
            return ui;
        }

        public override void Return(IPooledObject obj)
        {
            if(!(obj is UILRUPooledObject))
            {
                throw new System.InvalidOperationException($"{obj} must be UILRUPooledObject");
            }
            obj.OnRelease();

            // 没在LRU中则立即销毁
            if(!m_UIPrefabs.ContainsValue((UILRUPooledObject)obj))
            {
                DestroyUIPrefab((UILRUPooledObject)obj);
            }
        }

        private void OnDiscard(string assetPath, UILRUPooledObject ui)
        {
            m_UIPrefabs.Remove(assetPath);

            // 关闭状态下被LRU移除立即销毁
            if(!ui.isActiveAndEnabled)                  // todo: 【重构】需要判定“关闭状态”的接口
                DestroyUIPrefab(ui);
        }

        private UILRUPooledObject InstantiateUIPrefab(string assetPath)
        {
            GameObject go = new GameObject();
            go.name = assetPath;
            go.transform.parent = gameObject.transform;
            return go.AddComponent<UILRUPooledObject>();
        }
        private UILRUPooledObject InstantiateUIPrefab(string bundleName, string assetName)
        {
            GameObject go = new GameObject();
            go.name = assetName;
            go.transform.parent = gameObject.transform;
            return go.AddComponent<UILRUPooledObject>();
        }

        private void DestroyUIPrefab(UILRUPooledObject ui)
        {
            Object.Destroy(ui.gameObject);
        }
    }
}