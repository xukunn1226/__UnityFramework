﻿using UnityEngine;

namespace Framework.AssetManagement.Runtime.Tests
{
    public class LoadPrefab : MonoBehaviour
    {
        public LoaderType type;

        public string bundleName;
        public string assetName;

        GameObject inst;
        string info;

        GameObjectLoader loader;

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
                StartTask();
            }

            if (GUI.Button(new Rect(100, 280, 200, 80), "Unload"))
            {
                EndTask();
            }

            if (!string.IsNullOrEmpty(info))
            {
                GUI.Label(new Rect(100, 600, 500, 100), info);
            }

            if(GUI.Button(new Rect(100, 400, 200, 80), "masterTextureLimit = 0"))
            {
                QualitySettings.masterTextureLimit = 0;
            }

            if(GUI.Button(new Rect(100, 500, 200, 80), "masterTextureLimit = 1"))
            {
                QualitySettings.masterTextureLimit = 1;
            }

            if(GUI.Button(new Rect(100, 600, 200, 80), "masterTextureLimit = 2"))
            {
                QualitySettings.masterTextureLimit = 2;
            }
        }

        void StartTask()
        {
            loader = AssetManager.Instantiate(bundleName, assetName);

            info = inst != null ? "sucess to load: " : "fail to load: ";
            info += assetName;
        }

        void EndTask()
        {
            if(loader != null)
            {
                AssetManager.ReleaseInst(loader);
                loader = null;
            }
            
            if (inst != null)
            {
                Destroy(inst);
                inst = null;
            }
            info = null;
        }
    }
}