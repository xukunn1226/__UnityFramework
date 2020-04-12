using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cache;

namespace Tests
{
    public class UIManager
    {
        static private LRUQueue<string, UIPooledObject> m_Cached = new LRUQueue<string, UIPooledObject>(3);

        static public void Init()
        {
            m_Cached.OnDiscard += OnDiscard;
        }

        static public void Uninit()
        {
            m_Cached.OnDiscard -= OnDiscard;
            m_Cached.Clear();
        }

        static public UIPooledObject LoadUI(string assetPath)
        {
            UIPooledObject ui = m_Cached.Exist(assetPath);
            if (ui == null)
            {
                GameObject go = new GameObject();
                go.name = assetPath;
                ui = go.AddComponent<UIPooledObject>();
            }
            else
            {
                ui.OnGet();
            }
            return ui;
        }

        static public void UnloadUI(string assetPath, UIPooledObject ui)
        {
            ui.OnRelease();
            m_Cached.Return(assetPath, ui);
        }

        static private void OnDiscard(string assetPath, UIPooledObject ui)
        {

        }
    }
}