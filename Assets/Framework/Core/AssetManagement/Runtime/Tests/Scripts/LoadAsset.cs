using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime.Tests
{
    public class LoadAsset : MonoBehaviour
    {
        public LoaderType type;

        public string bundleName;
        public string assetName;

        AssetLoader<UnityEngine.Texture2D> loader;
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
            if (GUI.Button(new Rect(100, 100, 120, 60), "Load"))
            {
                StartTask();
            }

            if (GUI.Button(new Rect(100, 200, 120, 60), "Unload"))
            {
                EndTask();
            }

            if (GUI.Button(new Rect(100, 300, 120, 60), "UnloadUnusedAssets"))
            {
                UnloadUnusedAssets();
            }

            if (!string.IsNullOrEmpty(info))
            {
                GUI.Label(new Rect(100, 600, 500, 100), info);
            }
        }

        void StartTask()
        {
            loader = AssetManager.LoadAsset<UnityEngine.Texture2D>(bundleName, assetName);

            info = loader == null ? "fail to load: " : "sucess to load: ";
            info += assetName;
        }

        void EndTask()
        {
            if (loader != null)
            {
                AssetManager.UnloadAsset(loader);
            }
            info = null;
        }

        void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
        }
    }
}