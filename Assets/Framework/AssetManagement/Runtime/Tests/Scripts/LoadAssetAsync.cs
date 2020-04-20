using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime.Tests
{
    public class LoadAssetAsync : MonoBehaviour
    {
        public LoaderType type;

        public string assetPath;

        AssetLoaderAsync<Material> loader;
        string info;

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

            if (!string.IsNullOrEmpty(info))
            {
                GUI.Label(new Rect(100, 600, 500, 100), info);
            }
        }

        IEnumerator StartTask()
        {
            loader = AssetManager.LoadAssetAsync<Material>(assetPath);
            yield return loader;

            info = loader.asset != null ? "sucess to load: " : "fail to load: ";
            info += assetPath;

            if (loader.asset == null)
            {
                AssetManager.UnloadAsset(loader);
                loader = null;
            }
        }

        void EndTask()
        {
            if (loader != null)
            {
                AssetManager.UnloadAsset(loader);
                loader = null;
            }
            info = null;
        }
    }
}