using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetManagement.Runtime;

public class TestSyncAndAsync : MonoBehaviour
{
    public LoaderType type;

    public string assetPath;

    public Material Mat;

    GameObject inst;
    string info;
    AssetBundleLoader loader;

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
        if (GUI.Button(new Rect(100, 100, 200, 80), "Load -- TestSyncAndAsync"))
        {
            StartCoroutine(StartTaskEx());
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
    
    IEnumerator StartTaskEx()
    {
        int f1 = Time.frameCount;

        AssetBundleLoader loader = AssetManager.LoadAssetBundle("prefab.ab");
        AssetBundleRequest abRequest = loader.LoadAssetAsync<GameObject>("cube.prefab");
        //if (abRequest.asset == null) { }      // 这行代码因为访问了asset，将异步改为同步
        yield return abRequest;

        if(abRequest.asset != null)
        {
            inst = Instantiate<GameObject>(abRequest.asset as GameObject);
        }

        info = inst != null ? "sucess to load: " : "fail to load: ";
        info += assetPath;
        info += Time.frameCount - f1;
    }

    IEnumerator StartTask()
    {
        int f1 = Time.frameCount;

        // 异步加载
        IEnumerator e = AssetManager.InstantiatePrefabAsync(assetPath, (go) =>
        {
            inst = go;
        });

        // 马上同步加载，将立即结束之前的异步流程
        loader = AssetManager.LoadAssetBundle("prefab.ab");
        GameObject asset = loader.LoadAsset<GameObject>("cube.prefab");

        yield return e;

        info = inst != null ? "sucess to load: " : "fail to load: ";
        info += assetPath;
        info += Time.frameCount - f1;
    }

    void EndTask()
    {
        if (inst != null)
        {
            Destroy(inst);
            inst = null;
        }
        info = null;

        if(loader != null)
        {
            AssetManager.UnloadAssetBundle(loader);
        }
        loader = null;
    }
}
