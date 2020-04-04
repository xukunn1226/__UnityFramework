using System.Collections;
using UnityEngine;
using AssetManagement.Runtime;

public class LoadPrefabAsync : MonoBehaviour
{
    public LoaderType type;

    public string assetPath;

    GameObject inst;
    string info;

    private void Awake()
    {
        AssetManagerEx.Init(type);
    }

    void OnDestroy()
    {
        AssetManagerEx.Uninit();
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
        yield return AssetManagerEx.InstantiatePrefabAsync(assetPath, (go) =>
        {
            inst = go;
        });

        info = inst != null ? "sucess to load: " : "fail to load: ";
        info += assetPath;
    }

    void EndTask()
    {
        if (inst != null)
        {
            Destroy(inst);
            inst = null;
        }
        info = null;
    }
}
