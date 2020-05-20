using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using UnityEngine.U2D;

namespace Tests
{
    public class TestUIAtlas : MonoBehaviour
    {
        public LoaderType m_Type;
        string info;

        public SoftObject m_SoftObject;
        public SoftObject m_SoftObject2;

        private GameObject m_Go;

        private void Awake()
        {
            //ResourceManager.Init(m_Type);
        }

        private void OnDestroy()
        {
            //ResourceManager.Uninit();
        }

        // case 1: 通过实例化UI Prefab验证spriteAtlas加载、释放
        // 结论：正确释放，释放后再次触发回调RequestAtlas
        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 120, 60), "Load"))
            {
                m_Go = m_SoftObject?.Instantiate();
            }

            if (GUI.Button(new Rect(100, 300, 120, 60), "Unload Canvas"))
            {
                Destroy(m_Go);
            }
        }

        // case 2: 直接验证Atlas的加载、释放
        // 结论：正确释放，除首次外，释放后无法触发回调RequestAtlas
        //private void OnGUI()
        //{
        //    if (GUI.Button(new Rect(100, 100, 120, 60), "Load"))
        //    {
        //        LoadAtlas(1);
        //    }

        //    if (GUI.Button(new Rect(100, 300, 120, 60), "Unload Canvas"))
        //    {
        //        UnloadCanvas(1);
        //    }

        //    if (!string.IsNullOrEmpty(info))
        //    {
        //        GUI.Label(new Rect(100, 500, 120, 60), info);
        //    }
        //}

        // case 3： 上述两种方法的混合使用
        //private void OnGUI()
        //{
        //    if (GUI.Button(new Rect(100, 100, 120, 60), "LoadAtlas"))
        //    {
        //        LoadAtlas(1);
        //    }

        //    if (GUI.Button(new Rect(100, 200, 120, 60), "InstantiateCanvas"))
        //    {
        //        InstantiateCanvas(1);
        //    }

        //    if (GUI.Button(new Rect(100, 300, 120, 60), "Unload Canvas"))
        //    {
        //        UnloadCanvas(1);
        //    }

        //    if (!string.IsNullOrEmpty(info))
        //    {
        //        GUI.Label(new Rect(100, 600, 500, 100), info);
        //    }
        //}

        // case 4： 替换Canvas的sprite
        //private void OnGUI()
        //{
        //    if (GUI.Button(new Rect(100, 100, 120, 60), "InstantiateCanvas"))
        //    {
        //        InstantiateCanvas(1);
        //    }

        //    //if (GUI.Button(new Rect(100, 200, 120, 60), "InstantiateCanvas2"))
        //    //{
        //    //    InstantiateCanvas(2);
        //    //}

        //    if (GUI.Button(new Rect(100, 300, 120, 60), "Replace Atlas"))
        //    {
        //        ReplaceAtlas();
        //    }

        //    if (GUI.Button(new Rect(100, 400, 120, 60), "Unload Canvas"))
        //    {
        //        UnloadCanvas(1);
        //        UnloadCanvas(2);
        //    }


        //    if (!string.IsNullOrEmpty(info))
        //    {
        //        GUI.Label(new Rect(100, 600, 500, 100), info);
        //    }
        //}
    }
}