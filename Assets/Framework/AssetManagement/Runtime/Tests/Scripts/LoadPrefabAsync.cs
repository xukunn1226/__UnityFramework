using System.Collections;
using UnityEngine;

namespace Framework.AssetManagement.Runtime.Tests
{
    public class LoadPrefabAsync : MonoBehaviour
    {
        public LoaderType type;

        public string assetPath;

        GameObject inst;
        // string info;

        GameObjectLoaderAsync m_LoaderAsync;

        private void Awake()
        {
            AssetManager.Init(type);
        }

        //IEnumerator Start()
        //{
        //    yield return StartCoroutine(StartTask());
        //}

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
            m_LoaderAsync = AssetManager.InstantiateAsync(assetPath);
            yield return m_LoaderAsync;

            // m_LoaderAsync.asset
        }

        void EndTask()
        {
            if(m_LoaderAsync != null)
            {
                AssetManager.ReleaseInst(m_LoaderAsync);
                m_LoaderAsync = null;
            }
        }
    }
}