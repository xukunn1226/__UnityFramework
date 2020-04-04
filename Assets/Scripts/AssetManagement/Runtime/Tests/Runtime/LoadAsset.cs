using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetManagement.Runtime;

public class LoadAsset : MonoBehaviour
{
    public LoaderType type;

    public string   assetPath;

    AssetLoader<UnityEngine.Object> loader;
    string          info;

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
    }

    void StartTask()
    {
        loader = AssetManager.LoadAsset<UnityEngine.Object>(assetPath);

        info = loader.asset != null ? "sucess to load: " : "fail to load: ";
        info += assetPath;
    }

    void EndTask()
    {
        if(loader != null)
        {
            AssetManager.UnloadAsset(loader);
        }
        info = null;
    }
}
