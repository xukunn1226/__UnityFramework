using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetManagement.Runtime;

public class LoadAssetAsync : MonoBehaviour
{
    public LoaderType type;
    
    public string assetPath;

    LinkedListNode<AssetLoaderAsync<Material>> loader;
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
        loader = AssetLoaderAsync<Material>.Get(assetPath);
        yield return loader.Value;

        info = loader.Value.asset != null ? "sucess to load: " : "fail to load: ";
        info += assetPath;
    }

    void EndTask()
    {
        if(loader != null)
        {
            AssetLoaderAsync<Material>.Release(loader);
            loader = null;
        }
        info = null;
    }
}
