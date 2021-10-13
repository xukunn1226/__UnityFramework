using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Tests
{
    // 负责UI Prefab的加载、卸载
    public class TestUILoader : LRUMonoPool
    {
        private LRUQueue<string, TestUIView> m_LRUPool = new LRUQueue<string, TestUIView>(3);

        void Awake()
        {
            m_LRUPool.OnDiscard += OnDiscard;
        }

        private void OnDestroy()
        {
            m_LRUPool.OnDiscard -= OnDiscard;
        }

        private void OnDiscard(string assetPath, TestUIView item)
        {
            InternalUnload(assetPath, item);
        }

        public override void Return(IPooledObject item)
        {
            TestUIView view = item as TestUIView;
            if(view != null)
                Unload(view.assetPath);
        }

        public override void Clear()
        {
            m_LRUPool.Clear();
        }

        public TestUIView Load(string assetPath)
        {
            TestUIView view = m_LRUPool.Exist(assetPath);
            if (view == null)
            {
                view = InternalLoad(assetPath);
            }
            view.assetPath = assetPath;
            view.Pool = this;
            view.OnGet();
            m_LRUPool.Cache(assetPath, view);
            return view;
        }

        public void Unload(string assetPath)
        {
            TestUIView view = m_LRUPool.Exist(assetPath);
            if(view != null)
            {
                view.OnRelease();
                view.transform.parent = transform;
            }
        }

        private TestUIView InternalLoad(string assetPath)
        {
            GameObject go = new GameObject();
            go.name = assetPath;
            return go.AddComponent<TestUIView>();
        }

        private void InternalUnload(string assetPath, TestUIView ui)
        {
            Object.Destroy(ui.gameObject);
        }
    }
}