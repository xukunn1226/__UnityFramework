using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class LRUSample : MonoBehaviour
    {
        private TestUIView m_UI1;
        private TestUIView m_UI2;
        private TestUIView m_UI3;
        private TestUIView m_UI4;

        // Start is called before the first frame update
        void Start()
        {
            TestUIManager.Init();
        }

        private void OnDestroy()
        {
            TestUIManager.Uninit();
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
                    Unload("11");
            }

            if (GUI.Button(new Rect(100, 200, 200, 80), "Load 22"))
            {
                m_UI2 = Load("22");
            }

            if (GUI.Button(new Rect(320, 200, 200, 80), "Unload 22"))
            {
                if (m_UI2 != null)
                    Unload("22");
            }

            if (GUI.Button(new Rect(100, 300, 200, 80), "Load 33"))
            {
                m_UI3 = Load("33");
            }

            if (GUI.Button(new Rect(320, 300, 200, 80), "Unload 33"))
            {
                if (m_UI3 != null)
                    Unload("33");
            }

            if (GUI.Button(new Rect(100, 400, 200, 80), "Load 44"))
            {
                m_UI4 = Load("44");
            }

            if (GUI.Button(new Rect(320, 400, 200, 80), "Unload 44"))
            {
                if (m_UI4 != null)
                    Unload("44");
            }
        }

        private TestUIView Load(string assetPath)
        {
            return TestUIManager.LoadUI(assetPath);
        }

        private void Unload(string assetPath)
        {
            TestUIManager.UnloadUI(assetPath);
        }
    }
}