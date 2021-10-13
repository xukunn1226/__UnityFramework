using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Tests
{
    public class TestUIManager
    {
        static private TestUILoader s_Pool;

        static public void Init()
        {
            if(s_Pool == null)
            {
                GameObject go = new GameObject("[LRU POOL] UI");
                s_Pool = go.AddComponent<TestUILoader>();
            }
        }

        static public void Uninit()
        {
            if(s_Pool != null)
            {
                s_Pool.Clear();
                UnityEngine.Object.Destroy(s_Pool.gameObject);
            }
        }

        static public TestUIView LoadUI(string assetPath)
        {
            return s_Pool.Load(assetPath);
        }

        static public void UnloadUI(string assetPath)
        {
            s_Pool.Unload(assetPath);
        }
    }
}