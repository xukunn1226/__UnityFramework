using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Tests
{
    public class LRUPoolSample : MonoBehaviour
    {
        public LRUPool_UI LRUPrefab;
        private LRUPool_UI m_LRUInst;

        private UILRUPooledObject m_UI1;
        private UILRUPooledObject m_UI2;
        private UILRUPooledObject m_UI3;
        private UILRUPooledObject m_UI4;

        // Start is called before the first frame update
        void Start()
        {
            //if(LRUPrefab != null)
            //{
            //    m_LRUInst = Instantiate(LRUPrefab);
            //}

            m_LRUInst = FindObjectOfType<LRUPool_UI>();
        }

        private void OnDestroy()
        {
            if(m_LRUInst != null)
            {
                Destroy(m_LRUInst);
            }
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 200, 80), "Load 11"))
            {
                m_UI1 = Load("11");
            }

            if (GUI.Button(new Rect(320, 100, 200, 80), "Unload 11"))
            {
                if (m_UI1 != null)
                    Unload("11", m_UI1);
            }

            if (GUI.Button(new Rect(100, 200, 200, 80), "Load 22"))
            {
                m_UI2 = Load("22");
            }

            if (GUI.Button(new Rect(320, 200, 200, 80), "Unload 22"))
            {
                if (m_UI2 != null)
                    Unload("22", m_UI2);
            }

            if (GUI.Button(new Rect(100, 300, 200, 80), "Load 33"))
            {
                m_UI3 = Load("33");
            }

            if (GUI.Button(new Rect(320, 300, 200, 80), "Unload 33"))
            {
                if (m_UI3 != null)
                    Unload("33", m_UI3);
            }

            if (GUI.Button(new Rect(100, 400, 200, 80), "Load 44"))
            {
                m_UI4 = Load("44");
            }

            if (GUI.Button(new Rect(320, 400, 200, 80), "Unload 44"))
            {
                if (m_UI4 != null)
                    Unload("44", m_UI4);
            }
        }

        private UILRUPooledObject Load(string assetPath)
        {
            return m_LRUInst?.LoadUI(assetPath) ?? null;
        }

        private void Unload(string assetPath, UILRUPooledObject ui)
        {
            m_LRUInst?.UnloadUI(assetPath, ui);
        }
    }
}