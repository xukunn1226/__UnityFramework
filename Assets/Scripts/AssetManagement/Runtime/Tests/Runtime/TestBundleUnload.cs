using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetManagement.Runtime;

/// <summary>
/// 测试ABload asset、unload asset前后内存变化
/// </summary>
public class TestBundleUnload : MonoBehaviour
{
    public LoaderType type;

    public string assetPathA;
    public string assetPathB;

    LinkedListNode<AssetLoader<UnityEngine.Object>> loaderA;
    LinkedListNode<AssetLoader<UnityEngine.Object>> loaderB;

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
        if (GUI.Button(new Rect(100, 100, 150, 80), "LoadA"))
        {
            LoadA();
        }

        if (GUI.Button(new Rect(100, 200, 150, 80), "LoadB"))
        {
            LoadB();
        }

        if (GUI.Button(new Rect(100, 300, 150, 80), "UnloadA"))
        {
            UnLoadA();
        }

        if (GUI.Button(new Rect(100, 400, 150, 80), "UnloadB"))
        {
            UnLoadB();
        }

        if (GUI.Button(new Rect(100, 500, 150, 80), "UnloadUnusedAssets"))
        {
            UnloadUnusedAssets();
        }

        if (!string.IsNullOrEmpty(info))
        {
            GUI.Label(new Rect(100, 600, 500, 100), info);
        }
    }

    void LoadA()
    {
        loaderA = AssetLoader<UnityEngine.Object>.Get(assetPathA);

        info = loaderA.Value.asset != null ? "sucess to load: " : "fail to load: ";
        info += assetPathA;
    }

    void UnLoadA()
    {
        AssetLoader<UnityEngine.Object>.Release(loaderA);
    }

    void LoadB()
    {
        loaderB = AssetLoader<UnityEngine.Object>.Get(assetPathB);

        info = loaderB.Value.asset != null ? "sucess to load: " : "fail to load: ";
        info += assetPathB;
    }

    void UnLoadB()
    {
        AssetLoader<UnityEngine.Object>.Release(loaderB);
    }

    void UnloadUnusedAssets()
    {        
        info = null;
        Resources.UnloadUnusedAssets();
    }
}
