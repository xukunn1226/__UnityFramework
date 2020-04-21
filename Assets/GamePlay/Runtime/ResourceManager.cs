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




    /// <summary>
    /// 静态加载同步场景接口
    /// 1、场景必须加入Build settings
    /// 2、不可热更
    /// 3、sceneName不带后缀名，scenePath带后缀名
    /// 4、sceneName, scenePath大小写不敏感
    /// </summary>
    /// <param name="sceneName"></param>
    //static public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    //{
    //    AssetManager.LoadScene(sceneName, mode);
    //}

    //static public void LoadScene(int sceneBuildIndex, LoadSceneMode mode = LoadSceneMode.Single)
    //{
    //    AssetManager.LoadScene(sceneBuildIndex, mode);
    //}

    /// <summary>
    /// 静态场景异步加载接口
    /// 1、场景必须加入Build settings
    /// 2、不可热更
    /// 3、sceneName不带后缀名，scenePath带后缀名
    /// 4、sceneName, scenePath大小写不敏感
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    //static public AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    //{
    //    return AssetManager.LoadSceneAsync(sceneName, mode);
    //}

    //static public AsyncOperation LoadSceneAsync(int sceneBuildIndex, LoadSceneMode mode = LoadSceneMode.Single)
    //{
    //    return AssetManager.LoadSceneAsync(sceneBuildIndex, mode);
    //}






    //static public AsyncOperation UnloadSceneAsync(string sceneName)
    //{
    //    return AssetManager.UnloadSceneAsync(sceneName);
    //}

    //static public AsyncOperation UnloadSceneAsync(int sceneBuildIndex)
    //{
    //    return AssetManager.UnloadSceneAsync(sceneBuildIndex);
    //}
}
