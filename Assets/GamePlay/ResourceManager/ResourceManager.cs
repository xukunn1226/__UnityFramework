using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using UnityEngine.SceneManagement;
using System;

public class ResourceManager : MonoBehaviour
{
    static private ResourceManager  Instance            { get; set; }

    static private AssetManager     m_AssetManager;

    [SerializeField]
    private LoaderType              m_LoaderType        = LoaderType.FromEditor;

    [SerializeField]
    private string                  m_RootPath          = "Deployment/AssetBundles";

    static private bool             k_bDynamicLoad;                                                             // true: dynamic loading AssetManager; false: static loading AssetManager

    private void Awake()
    {
        // 已有ResourceManager，则自毁
        if (FindObjectsOfType<ResourceManager>().Length > 1)
        {
            DestroyImmediate(this);
            throw new Exception("ResourceManager has already exist...");
        }

        gameObject.name = "[ResourceManager]";

        Instance = this;

        gameObject.transform.parent = null;
        DontDestroyOnLoad(gameObject);

        if(!k_bDynamicLoad)
        {
            m_AssetManager = AssetManager.Init(m_LoaderType, m_RootPath);
            m_AssetManager.transform.parent = transform;
        }

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    private void OnDestroy()
    {
        AssetManager.Uninit();
        k_bDynamicLoad = false;
        Instance = null;        
    }

    static public void Init(LoaderType type, string bundleRootPath = "Deployment/AssetBundles")
    {
        m_AssetManager = AssetManager.Init(type, bundleRootPath);

        k_bDynamicLoad = true;
        GameObject go = new GameObject();
        go.AddComponent<ResourceManager>();

        m_AssetManager.transform.parent = go.transform;

        Instance.m_LoaderType = type;
        Instance.m_RootPath = bundleRootPath;
    }

    static public void Uninit()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
    }

    static public void RegisterCustomizedAssetPathParser(AssetManager.AssetPathToBundleAndAsset_EventHandler parser1, AssetManager.BundleAndAssetToAssetPath_EventHandler parser2)
    {
        if (parser1 == null)
            throw new System.ArgumentNullException("parser1");
        if (parser2 == null)
            throw new System.ArgumentNullException("parser2");

        AssetManager.RegisterCustomizedParser(parser1, parser2);
    }

    static public GameObject InstantiatePrefab(string assetPath)
    {
        return AssetManager.InstantiatePrefab(assetPath);
    }

    static public GameObject InstantiatePrefab(string assetBundleName, string assetName)
    {
        return AssetManager.InstantiatePrefab(assetBundleName, assetName);
    }

    static public IEnumerator InstantiatePrefabAsync(string assetPath, Action<GameObject> handler = null)
    {
        return AssetManager.InstantiatePrefabAsync(assetPath, handler);
    }

    static public IEnumerator InstantiatePrefabAsync(string assetBundleName, string assetName, Action<GameObject> handler = null)
    {
        return AssetManager.InstantiatePrefabAsync(assetBundleName, assetName, handler);
    }

    static public AssetLoader<T> LoadAsset<T>(string assetPath) where T : UnityEngine.Object
    {
        return AssetManager.LoadAsset<T>(assetPath);
    }

    static public AssetLoader<T> LoadAsset<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
    {
        return AssetManager.LoadAsset<T>(assetBundleName, assetName);
    }

    static public AssetLoaderAsync<T> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object
    {
        return AssetManager.LoadAssetAsync<T>(assetPath);
    }

    static public AssetLoaderAsync<T> LoadAssetAsync<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
    {
        return AssetManager.LoadAssetAsync<T>(assetBundleName, assetName);
    }

    static public void UnloadAsset<T>(AssetLoader<T> loader) where T : UnityEngine.Object
    {
        AssetManager.UnloadAsset<T>(loader);
    }

    static public void UnloadAsset<T>(AssetLoaderAsync<T> loader) where T : UnityEngine.Object
    {
        AssetManager.UnloadAsset<T>(loader);
    }

    static public AssetBundleLoader LoadAssetBundle(string assetBundleName)
    {
        return AssetManager.LoadAssetBundle(assetBundleName);
    }

    static public void UnloadAssetBundle(AssetBundleLoader abLoader)
    {
        AssetManager.UnloadAssetBundle(abLoader);
    }


    static public SceneLoader LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        return AssetManager.LoadScene(sceneName, mode);
    }

    static public SceneLoaderAsync LoadSceneAsync(string sceneName, LoadSceneMode mode, bool allowSceneActivation = true)
    {
        return AssetManager.LoadSceneAsync(sceneName, mode, allowSceneActivation);
    }

    static public SceneLoader LoadScene(string bundlePath, string sceneName, LoadSceneMode mode)
    {
        return AssetManager.LoadScene(bundlePath, sceneName, mode);
    }

    static public SceneLoaderAsync LoadSceneAsync(string bundlePath, string sceneName, LoadSceneMode mode, bool allowSceneActivation = true)
    {
        return AssetManager.LoadSceneAsync(bundlePath, sceneName, mode, allowSceneActivation);
    }

    static public AsyncOperation UnloadSceneAsync(SceneLoader loader)
    {
        return AssetManager.UnloadSceneAsync(loader);
    }

    static public AsyncOperation UnloadSceneAsync(SceneLoaderAsync loader)
    {
        return AssetManager.UnloadSceneAsync(loader);
    }
}
