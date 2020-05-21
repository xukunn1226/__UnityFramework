using System.Collections;
using System.Collections.Generic;
using UnityEngine.U2D;
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

    [SerializeField]
    private string                  m_UIAtlasPath       = "res/ui/atlases";

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

        SpriteAtlasManager.atlasRequested += RequestAtlas;
    }

    private void OnDestroy()
    {
        SpriteAtlasManager.atlasRequested -= RequestAtlas;

        k_bDynamicLoad = false;
        Instance = null;

        AssetManager.Uninit();
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

    class PersistentSpriteAtlas
    {
        public string                           AtlasName;
        public int                              RefCount;
        public AssetLoaderAsync<SpriteAtlas>    Loader;
        public List<string>                     UsersList = new List<string>();
        public bool                             PendingRequest;

        protected PersistentSpriteAtlas() { }

        public PersistentSpriteAtlas(string atlasName)
        {
            AtlasName = atlasName;
            RefCount = 1;
            Loader = null;
        }
    }
    static private Dictionary<string, PersistentSpriteAtlas> s_PersistentAtlasDic = new Dictionary<string, PersistentSpriteAtlas>();
    
    class RuntimeSpriteAtlas
    {
        public string                           AtlasName;
        public int                              RefCount;
        public AssetLoader<SpriteAtlas>         Loader;
        public List<string>                     UsersList = new List<string>();
        public bool                             PendingRequest;

        protected RuntimeSpriteAtlas() { }

        public RuntimeSpriteAtlas(string atlasName)
        {
            AtlasName = atlasName;
            RefCount = 1;
            Loader = null;
        }
    }
    static private Dictionary<string, RuntimeSpriteAtlas> s_RuntimeAtlasDic = new Dictionary<string, RuntimeSpriteAtlas>();
    static private bool m_DoCallback;


    // Atlas没在内存中将触发request
    void RequestAtlas(string atlasName, Action<SpriteAtlas> callback)
    {
        Debug.LogError($"{string.Format($"ResourceManager        {Time.frameCount}   RequestAtlas： {atlasName}")}");

        m_DoCallback = false;

        // 图集加载请求必定来自于runtime或persistent
        DoLoadRuntimeSpriteAtlas(atlasName, callback);
        if(!m_DoCallback)
        {
            StartCoroutine(DoLoadPersistentSpriteAtlasAsync(atlasName, callback));
        }

        if (!m_DoCallback)
            throw new InvalidOperationException("PersistentAtlas and RuntimeAtlas have no PendingRequest, there are something wrong");
    }

    static private IEnumerator DoLoadPersistentSpriteAtlasAsync(string atlasName, Action<SpriteAtlas> callback)
    {
        PersistentSpriteAtlas atlasRef;
        if (!s_PersistentAtlasDic.TryGetValue(atlasName, out atlasRef))             // 查找不到属正常情况，待载入图集请求可能在s_RuntimeAtlasDic
            yield break;

        if (atlasRef.Loader == null)
            throw new System.ArgumentException("atlasRef.Loader != null", "atlasRef.Loader");

        if (atlasRef.PendingRequest)
        {
            m_DoCallback = true;
            yield return atlasRef.Loader;

            callback(atlasRef.Loader.asset);
        }
    }

    static private void DoLoadRuntimeSpriteAtlas(string atlasName, Action<SpriteAtlas> callback)
    {
        RuntimeSpriteAtlas atlasRef;
        if (!s_RuntimeAtlasDic.TryGetValue(atlasName, out atlasRef))                // 查找不到属正常情况，待载入图集请求可能在s_PersistentAtlasDic
            return;
            
        if(atlasRef.Loader == null)
            throw new System.ArgumentException("atlasRef.Loader == null", "atlasRef.Loader");

        if (atlasRef.Loader.asset == null)
            throw new System.ArgumentException("atlasRef.Loader.asset == null", "atlasRef.Loader.asset");

        if (atlasRef.PendingRequest)
        {
            // 运行时动态加载图集已由GetAtlas完成载入，这里仅执行callback
            callback(atlasRef.Loader.asset);
            m_DoCallback = true;
        }
    }

    /// <summary>
    /// register persistent atlas
    /// WARNING: 1、务必在Awake中调用，因为要先于RequestAtlas调用之前记录数据
    ///          2、不可在OnEnable中调用，原因见UnregisterPersistentAtlas
    /// </summary>
    /// <param name="atlasName"></param>
    static public void RegisterPersistentAtlas(string atlasName, string userTag)
    {
        //Debug.LogError($"RegisterPersistentAtlas:    {Time.frameCount}       {atlasName}     {userTag}");

        if (string.IsNullOrEmpty(userTag))
            throw new ArgumentNullException(userTag);

        PersistentSpriteAtlas atlasRef;
        if(s_PersistentAtlasDic.TryGetValue(atlasName, out atlasRef))
        { // 因为异步原因，atlas可能正在加载
            atlasRef.RefCount += 1;
            atlasRef.UsersList.Add(userTag);
            return;
        }

        atlasRef = new PersistentSpriteAtlas(atlasName);
        atlasRef.UsersList.Add(userTag);
        atlasRef.Loader = LoadAssetAsync<SpriteAtlas>(GetAtlasPath(atlasName));
        atlasRef.PendingRequest = !s_RuntimeAtlasDic.ContainsKey(atlasName);             // 已被RuntimeAtlas记录表示不会进入RequestAtlas
        s_PersistentAtlasDic.Add(atlasName, atlasRef);
    }

    /// <summary>
    /// unregister persistent atlas
    /// WARNING: 务必在OnDestroy中调用，不可在OnDisable中调用，因为仅deactive卸载不干净sprite，仍被Image持有，需要持有sprite的对象删除才能清除干净
    /// </summary>
    /// <param name="atlasName"></param>
    static public void UnregisterPersistentAtlas(string atlasName, string userTag)
    {
        PersistentSpriteAtlas atlasRef;
        if (!s_PersistentAtlasDic.TryGetValue(atlasName, out atlasRef))
            throw new System.InvalidOperationException($"{atlasName} has already unloaded, userTag: {userTag}");

        if (atlasRef.Loader == null)
            throw new System.ArgumentNullException("atlasRef.Loader");

        atlasRef.RefCount -= 1;
        atlasRef.UsersList.Remove(userTag);

        if(atlasRef.RefCount == 0)
        {
            UnloadAsset(atlasRef.Loader);
            s_PersistentAtlasDic.Remove(atlasName);
        }
    }

    static public SpriteAtlas GetAtlas(string atlasName, string userTag)
    {
        //Debug.LogError($"{string.Format($"-------------GetAtlas     {Time.frameCount}   ")}");

        if (string.IsNullOrEmpty(userTag))
            throw new ArgumentNullException(userTag);

        RuntimeSpriteAtlas runtimeAtlas;
        if(s_RuntimeAtlasDic.TryGetValue(atlasName, out runtimeAtlas))
        {
            if (runtimeAtlas.Loader == null)
                throw new ArgumentNullException("runtimeAtlas.Loader");

            runtimeAtlas.RefCount += 1;
            runtimeAtlas.UsersList.Add(userTag);
            return runtimeAtlas.Loader.asset;
        }

        runtimeAtlas = new RuntimeSpriteAtlas(atlasName);
        runtimeAtlas.Loader = LoadAsset<SpriteAtlas>(GetAtlasPath(atlasName));
        runtimeAtlas.UsersList.Add(userTag);
        runtimeAtlas.PendingRequest = !s_PersistentAtlasDic.ContainsKey(atlasName);             // 已被PersistentAtlas记录表示不会进入RequestAtlas
        s_RuntimeAtlasDic.Add(atlasName, runtimeAtlas);
        return runtimeAtlas.Loader.asset;
    }

    static public void ReleaseAtlas(string atlasName, string userTag)
    {
        RuntimeSpriteAtlas runtimeAtlas;
        if (!s_RuntimeAtlasDic.TryGetValue(atlasName, out runtimeAtlas))
            throw new Exception($"Can't find atlasName [{atlasName}] in s_RuntimeAtlasDic");

        if (runtimeAtlas.Loader == null)
            throw new System.InvalidOperationException($"{atlasName} can't unload, because it has not been loaded");

        runtimeAtlas.RefCount -= 1;
        runtimeAtlas.UsersList.Remove(userTag);

        if(runtimeAtlas.RefCount == 0)
        {
            UnloadAsset(runtimeAtlas.Loader);
            s_RuntimeAtlasDic.Remove(atlasName);
        }
    }

    static private string GetAtlasPath(string atlasName)
    {
        return string.Format($"{Instance.m_UIAtlasPath}/{atlasName}.spriteatlas");
    }
}
