using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Framework.Core;
using UnityEngine.SceneManagement;

namespace Framework.AssetManagement.Runtime
{
    /// 设计目标：
    /// 1、即可动态生成又可静态挂载；
    /// 2、初始化后立即可用；
    /// 3、只允许一个AssetManager存在
    
    /// <summary>
    /// 生成AssetManager有两种方式：
    /// 1、脚本动态生成：AssetManager.Init();
    /// 2、静态挂载
    /// 注意事项：
    /// 1、两种方式只能选其一，同时使用概不负责
    /// </summary>
    public sealed class AssetManager : MonoBehaviour
    {
        static internal AssetManager    Instance { get; private set; }

        static internal int             PreAllocateAssetBundlePoolSize        = 200;                                // 预分配缓存AssetBundleRef对象池大小
        static internal int             PreAllocateAssetBundleLoaderPoolSize  = 100;                                // 预分配缓存AssetBundleLoader对象池大小
        static internal int             PreAllocateAssetLoaderPoolSize        = 50;                                 // 预分配缓存AssetLoader对象池大小
        static internal int             PreAllocateAssetLoaderAsyncPoolSize   = 50;                                 // 预分配缓存AssetLoaderAsync对象池大小

        [SerializeField]
        private LoaderType              m_LoaderType;

        [SerializeField]
        private string                  m_RootPath = "Deployment/AssetBundles";
        
        static private bool             k_bDynamicLoad;                                                             // true: dynamic loading AssetManager; false: static loading AssetManager

        internal LoaderType             loaderType
        {
            get
            {
#if UNITY_EDITOR
                if (Instance != null)
                {
                    return m_LoaderType;
                }
                return LoaderType.FromEditor;
#else
                return LoaderType.FromAB;       // 移动平台强制AB加载
#endif
            }
        }

        private void Awake()
        {
            // 已有AssetManager，则自毁
            if (FindObjectsOfType<AssetManager>().Length > 1)
            {
                DestroyImmediate(this);
                throw new Exception("AssetManager has already exist...");
            }

            gameObject.name = "[AssetManager]";

            Instance = this;

            DontDestroyOnLoad(gameObject);

            if(!k_bDynamicLoad)
                InternalInit(m_LoaderType, m_RootPath);

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private void OnDestroy()
        {
            if (loaderType == LoaderType.FromAB)
            {
                // 确保应用层持有的AssetLoader(Async)释放后再调用
                AssetBundleManager.Uninit();
            }
            k_bDynamicLoad = false;
            Instance = null;
        }

        /// <summary>
        /// 资源管理器初始化
        /// </summary>
        /// <param name="type">AB加载模式或直接从project中加载资源</param>
        /// <param name="bundleRootPath">bundle资源路径，仅限AB加载模式时有效</param>
        static public AssetManager Init(LoaderType type, string bundleRootPath = "Deployment/AssetBundles")
        {
            k_bDynamicLoad = true;

            GameObject go = new GameObject("[AssetManager]", typeof(AssetManager));

            Instance.InternalInit(type, bundleRootPath);

            return go.GetComponent<AssetManager>();
        }

        private void InternalInit(LoaderType type, string bundleRootPath)
        {
            m_LoaderType = type;

            // 根据不同应用环境初始化资源路径，e.g. 编辑器、开发环境、正式发布环境
#if UNITY_EDITOR
            m_RootPath = bundleRootPath;
#else
#if UNITY_ANDROID
            //rootPath = "jar:file://" + Application.dataPath + "!/assets";
            m_RootPath = Application.streamingAssetsPath;
#elif UNITY_IOS
            //rootPath = Application.dataPath + "/Raw";
            m_RootPath = Application.streamingAssetsPath;
#else
            //rootPath = Application.dataPath + "/StreamingAssets";
            m_RootPath = Application.streamingAssetsPath;
#endif
#endif
            if (m_LoaderType == LoaderType.FromAB)
            {
                AssetBundleManager.Init(m_RootPath, Utility.GetPlatformName());
            }
            Debug.Log($"AssetManager.loaderType is {m_LoaderType}");
        }

        static public void Uninit()
        {
            if (Instance != null)
            {
                Destroy(Instance);
            }
        }


        /// <summary>
        /// 加载prefab资源接口（同步、异步共4个）
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        static public GameObject InstantiatePrefab(string assetPath)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");

            GameObject go = null;

            AssetLoader<GameObject> loader = LoadAsset<GameObject>(assetPath);
            if (loader.asset != null)
            {
                go = UnityEngine.Object.Instantiate(loader.asset);

                GameObjectDestroyer destroyer = go.AddComponent<GameObjectDestroyer>();
                destroyer.loader = loader;
            }
            else
            {
                Debug.LogWarningFormat("InstantiatePrefab -- Failed to load asset[{0}]", assetPath);

                UnloadAsset(loader);        // 加载失败回收AssetLoader
            }

            return go;
        }

        static public GameObject InstantiatePrefab(string assetBundleName, string assetName)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");

            GameObject go = null;

            AssetLoader<GameObject> loader = LoadAsset<GameObject>(assetBundleName, assetName);
            if (loader.asset != null)
            {
                go = UnityEngine.Object.Instantiate(loader.asset);

                GameObjectDestroyer destroyer = go.AddComponent<GameObjectDestroyer>();
                destroyer.loader = loader;
            }
            else
            {
                Debug.LogWarningFormat("InstantiatePrefab -- Failed to load asset[{0}]", assetBundleName + "/" + assetName);

                UnloadAsset(loader);        // 加载失败回收AssetLoader
            }

            return go;
        }

        static public IEnumerator InstantiatePrefabAsync(string assetPath, Action<GameObject> handler = null)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            GameObject go = null;

            AssetLoaderAsync<GameObject> loaderAsync = LoadAssetAsync<GameObject>(assetPath);
            yield return loaderAsync;
            if (loaderAsync.asset != null)
            {
                go = UnityEngine.Object.Instantiate(loaderAsync.asset);

                GameObjectDestroyer destroyer = go.AddComponent<GameObjectDestroyer>();
                destroyer.loaderAsync = loaderAsync;

                handler?.Invoke(go);
            }
            else
            {
                Debug.LogWarningFormat("InstantiatePrefabAsync -- Failed to load asset[{0}]", assetPath);

                UnloadAsset(loaderAsync);        // 加载失败回收AssetLoaderAsync
            }            
        }

        static public IEnumerator InstantiatePrefabAsync(string assetBundleName, string assetName, Action<GameObject> handler = null)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            GameObject go = null;

            AssetLoaderAsync<GameObject> loaderAsync = LoadAssetAsync<GameObject>(assetBundleName, assetName);
            yield return loaderAsync;
            if (loaderAsync.asset != null)
            {
                go = UnityEngine.Object.Instantiate(loaderAsync.asset);

                GameObjectDestroyer destroyer = go.AddComponent<GameObjectDestroyer>();
                destroyer.loaderAsync = loaderAsync;

                handler?.Invoke(go);
            }
            else
            {
                Debug.LogWarningFormat("InstantiatePrefabAsync -- Failed to load asset[{0}]", assetBundleName + "/" + assetName);

                UnloadAsset(loaderAsync);        // 加载失败回收AssetLoaderAsync
            }            
        }

        /// <summary>
        /// 同步加载资源接口，如果加载失败则返回null，内部会自动回收loader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static public AssetLoader<T> LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            AssetLoader<T> loader = AssetLoader<T>.Get(assetPath);
            if(loader.asset == null)
            {
                UnloadAsset(loader);
                return null;
            }
            return loader;
        }

        /// <summary>
        /// 同上
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        static public AssetLoader<T> LoadAsset<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            AssetLoader<T> loader = AssetLoader<T>.Get(assetBundleName, assetName);
            if(loader.asset == null)
            {
                UnloadAsset(loader);
                return null;
            }
            return loader;
        }

        /// <summary>
        /// 异步加载资源接口，如果加载失败需应用层释放loader，因为异步内部无法判断是否加载成功
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static public AssetLoaderAsync<T> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            return AssetLoaderAsync<T>.Get(assetPath);
        }

        /// <summary>
        /// 同上
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        static public AssetLoaderAsync<T> LoadAssetAsync<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            return AssetLoaderAsync<T>.Get(assetBundleName, assetName);
        }

        static public void UnloadAsset<T>(AssetLoader<T> loader) where T : UnityEngine.Object
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            AssetLoader<T>.Release(loader);
        }

        static public void UnloadAsset<T>(AssetLoaderAsync<T> loader) where T : UnityEngine.Object
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            AssetLoaderAsync<T>.Release(loader);
        }

        /// <summary>
        /// ab加载接口
        /// </summary>
        static public AssetBundleLoader LoadAssetBundle(string assetBundleName)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            return AssetBundleLoader.Get(assetBundleName);
        }

        /// <summary>
        /// ab卸载接口
        /// </summary>
        /// <param name="abLoader"></param>
        static public void UnloadAssetBundle(AssetBundleLoader abLoader)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            AssetBundleLoader.Release(abLoader);
        }

        /// <summary>
        /// 静态加载同步场景接口
        /// 1、场景必须加入Build settings
        /// 2、不可热更
        /// 3、sceneName不带后缀名，scenePath带后缀名
        /// 4、sceneName, scenePath大小写不敏感
        /// </summary>
        /// <param name="sceneName"></param>
        static public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            SceneManager.LoadScene(sceneName, mode);
        }

        static public void LoadScene(int sceneBuildIndex, LoadSceneMode mode = LoadSceneMode.Single)
        {
            SceneManager.LoadScene(sceneBuildIndex, mode);
        }

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
        static public AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            return SceneManager.LoadSceneAsync(sceneName, mode);
        }

        static public AsyncOperation LoadSceneAsync(int sceneBuildIndex, LoadSceneMode mode = LoadSceneMode.Single)
        {
            return SceneManager.LoadSceneAsync(sceneBuildIndex, mode);
        }


        struct AssetName
        {
            public string assetBundleName;
            public string assetName;
        }
        static Dictionary<string, AssetName> s_dic = new Dictionary<string, AssetName>();               // 缓存，减少资源路径的解析；key: assetPath; value: AssetName

        /// <summary>
        /// 从资源路径解析出其所在的 AssetBundle Name 和 Asset Name
        /// </summary>
        /// <param name="assetPath">res/windows/test/cube.prefab</param>
        /// <param name="assetBundleName">res/windows/test.ab</param>
        /// <param name="assetName">cube.prefab</param>
        /// <returns></returns>
        static internal bool ParseAssetPath(string assetPath, out string assetBundleName, out string assetName)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                assetBundleName = null;
                assetName = null;
                return false;
            }

            AssetName an;
            if (s_dic.TryGetValue(assetPath, out an))
            {
                assetBundleName = an.assetBundleName;
                assetName = an.assetName;
                return true;
            }

            assetBundleName = null;
            assetName = null;

            int index = assetPath.LastIndexOf(@"/");
            if (index == -1)
                return false;

            assetName = assetPath.Substring(index + 1);
            assetBundleName = assetPath.Substring(0, index) + ".ab";

            an = new AssetName();
            an.assetBundleName = assetBundleName;
            an.assetName = assetName;
            s_dic.Add(assetPath, an);

            return true;
        }
    }
}