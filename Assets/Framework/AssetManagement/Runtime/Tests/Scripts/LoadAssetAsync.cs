using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime.Tests
{
    public class LoadAssetAsync : MonoBehaviour
    {
        public LoaderType type;

        public string bundleName;
        public string assetName;

        AssetLoaderAsync<Material> m_Loader;
        // string info;

        private void Awake()
        {
            AssetManager.Init(type);
        }

        void OnDestroy()
        {
            AssetManager.Uninit();
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 200, 80), "Load"))
            {
                StartCoroutine(StartTask());
            }

            if (GUI.Button(new Rect(100, 280, 200, 80), "Unload"))
            {
                EndTask();
            }

            // if (!string.IsNullOrEmpty(info))
            // {
            //     GUI.Label(new Rect(100, 600, 500, 100), info);
            // }
        }

        IEnumerator StartTask()
        {
            //m_Loader = AssetManager.LoadAssetAsync<Material>(bundleName, assetName);
            //yield return m_Loader;
            yield break;

            // m_Loader.asset
        }

        void EndTask()
        {
            if (m_Loader != null)
            {
                AssetManager.UnloadAsset(m_Loader);
                m_Loader = null;
            }
        }
    }
}