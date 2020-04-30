using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Tests
{
    public class UILRUManager : LRUPool
    {
        static private LRUQueue<string, UILRUPooledObject> m_UIPrefabs;

        public int countOfUsed { get { return m_UIPrefabs.Count; } }

        protected override void InitLRU()
        {
            if (m_UIPrefabs != null)
                throw new System.Exception("m_UIPrefabs != null");

            m_UIPrefabs = new LRUQueue<string, UILRUPooledObject>(Capacity);        // 自动注册到PoolManager
            m_UIPrefabs.OnDiscard += OnDiscard;
        }

        protected override void UninitLRU()
        {
            if (m_UIPrefabs == null)
                throw new System.ArgumentNullException("m_UIPrefabs");

            m_UIPrefabs.OnDiscard -= OnDiscard;
            PoolManager.RemoveObjectPool(typeof(UILRUPooledObject));
        }

        public UILRUPooledObject LoadUI(string assetPath)
        {
            UILRUPooledObject ui = m_UIPrefabs.Exist(assetPath);
            if (ui == null)
            {
                ui = InstantiateUIPrefab(assetPath);
            }
            else
            {
                ui.OnGet();
            }
            m_UIPrefabs.Cache(assetPath, ui);
            return ui;
        }

        public void UnloadUI(string assetPath, UILRUPooledObject ui)
        {
            ui.OnRelease();
        }

        private void OnDiscard(string assetPath, UILRUPooledObject ui)
        {
            DestroyUIPrefab(assetPath, ui);
        }

        private UILRUPooledObject InstantiateUIPrefab(string assetPath)
        {
            GameObject go = new GameObject();
            go.name = assetPath;
            go.transform.parent = gameObject.transform;
            return go.AddComponent<UILRUPooledObject>();
        }

        private void DestroyUIPrefab(string assetPath, UILRUPooledObject ui)
        {
            Object.Destroy(ui.gameObject);
        }
    }
}