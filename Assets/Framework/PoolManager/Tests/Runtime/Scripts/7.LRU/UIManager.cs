using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Tests
{
    public class UIManager
    {
        static private LRUQueue<string, UIView> m_UIPrefabs = new LRUQueue<string, UIView>(3);

        static public void Init()
        {
            m_UIPrefabs.OnDiscard += OnDiscard;
        }

        static public void Uninit()
        {
            m_UIPrefabs.OnDiscard -= OnDiscard;
            m_UIPrefabs.Clear();
        }

        static public UIView LoadUI(string assetPath)
        {
            UIView ui = m_UIPrefabs.Exist(assetPath);
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

        static public void UnloadUI(string assetPath, UIView ui)
        {
            ui.OnRelease();
        }

        static private void OnDiscard(string assetPath, UIView ui)
        {
            InternalUnloadUI(assetPath, ui);
        }

        static private UIView InternalLoadUI(string assetPath)
        {
            GameObject go = new GameObject();
            go.name = assetPath;
            return go.AddComponent<UIView>();
        }

        static private void InternalUnloadUI(string assetPath, UIView ui)
        {
            Object.Destroy(ui.gameObject);
        }
    }
}