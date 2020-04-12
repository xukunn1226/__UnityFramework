using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cache;

namespace Tests
{
    public class UIManager
    {
        static private LRUQueue<string, UIPooledObject> m_UIPrefabs = new LRUQueue<string, UIPooledObject>(3);

        static public void Init()
        {
            m_UIPrefabs.OnDiscard += OnDiscard;
        }

        static public void Uninit()
        {
            m_UIPrefabs.OnDiscard -= OnDiscard;
            m_UIPrefabs.Clear();
        }

        static public UIPooledObject LoadUI(string assetPath)
        {
            UIPooledObject ui = m_UIPrefabs.Exist(assetPath);
            if (ui == null)
            {
                ui = InternalLoadUI(assetPath);
            }
            else
            {
                ui.OnGet();
            }
            m_UIPrefabs.Cache(assetPath, ui);
            return ui;
        }

        static public void UnloadUI(string assetPath, UIPooledObject ui)
        {
            ui.OnRelease();
        }

        static private void OnDiscard(string assetPath, UIPooledObject ui)
        {
            InternalUnloadUI(assetPath, ui);
        }

        static private UIPooledObject InternalLoadUI(string assetPath)
        {
            GameObject go = new GameObject();
            go.name = assetPath;
            return go.AddComponent<UIPooledObject>();
        }

        static private void InternalUnloadUI(string assetPath, UIPooledObject ui)
        {
            Object.Destroy(ui.gameObject);
        }
    }
}