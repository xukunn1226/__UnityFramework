using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Tests
{
    public class TestUIManager
    {
        static private LRUQueue<string, TestUIView> m_UIPrefabs = new LRUQueue<string, TestUIView>(3);

        static public void Init()
        {
            m_UIPrefabs.OnDiscard += OnDiscard;
        }

        static public void Uninit()
        {
            m_UIPrefabs.Clear();
            m_UIPrefabs.OnDiscard -= OnDiscard;
        }

        static public TestUIView LoadUI(string assetPath)
        {
            TestUIView view = m_UIPrefabs.Exist(assetPath);
            if (view == null)
            {
                view = InternalLoadUI(assetPath);
            }
            view.OnGet();
            m_UIPrefabs.Cache(assetPath, view);
            return view;
        }

        static public void UnloadUI(string assetPath)
        {
            TestUIView view = m_UIPrefabs.Exist(assetPath);
            view?.OnRelease();
        }

        static private void OnDiscard(string assetPath, TestUIView ui)
        {
            InternalUnloadUI(assetPath, ui);
        }

        static private TestUIView InternalLoadUI(string assetPath)
        {
            GameObject go = new GameObject();
            go.name = assetPath;
            return go.AddComponent<TestUIView>();
        }

        static private void InternalUnloadUI(string assetPath, TestUIView ui)
        {
            Object.Destroy(ui.gameObject);
        }
    }
}