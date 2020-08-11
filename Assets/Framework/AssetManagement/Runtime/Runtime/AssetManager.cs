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
        public delegate bool AssetPathToBundleAndAsset_EventHandler(string assetPath, out string assetBundleName, out string assetName);
        static public event AssetPathToBundleAndAsset_EventHandler CustomizedParser_AssetPathToBundleAndAsset;

        public delegate bool BundleAndAssetToAssetPath_EventHandler(string assetBundleName, string assetName, out string assetPath);
        static public event BundleAndAssetToAssetPath_EventHandler CustomizedParser_BundleAndAssetToAssetPath;

        static public AssetManager      Instance { get; private set; }

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
        static public AssetManager Init(LoaderType type, string bundleRootPath = "Assets/Deployment/AssetBundles")
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
                AssetBundleManager.Init(m_RootPath);
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

        static public void RegisterCustomizedParser(AssetPathToBundleAndAsset_EventHandler parser1, BundleAndAssetToAssetPath_EventHandler parser2)
        {
            if (parser1 == null)
                throw new ArgumentNullException("parser1");

            if (parser2 == null)
                throw new ArgumentNullException("parser2");

            if (Instance == null)
                throw new ArgumentNullException("Instance", "AssetManager not init");

            CustomizedParser_AssetPathToBundleAndAsset += parser1;
            CustomizedParser_BundleAndAssetToAssetPath += parser2;
        }


        /// <summary>
        /// 加载prefab资源接口（同步、异步共4个）
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static public GameObjectLoader Instantiate(string assetPath)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            return GameObjectLoader.Get(assetPath);
        }
        static public GameObjectLoader Instantiate(string bundleName, string assetName)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            return GameObjectLoader.Get(bundleName, assetName);
        }

        static public void ReleaseInst(GameObjectLoader loader)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
                
            GameObjectLoader.Release(loader);
        }

        static public GameObjectLoaderAsync InstantiateAsync(string assetPath)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            return GameObjectLoaderAsync.Get(assetPath);
        }
        static public GameObjectLoaderAsync InstantiateAsync(string bundleName, string assetName)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            return GameObjectLoaderAsync.Get(bundleName, assetName);
        }

        static public void ReleaseInst(GameObjectLoaderAsync loader)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
                
            GameObjectLoaderAsync.Release(loader);
        }

        /// <summary>
        /// 同步加载资源接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static public AssetLoader<T> LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            return AssetLoader<T>.Get(assetPath);
        }
        static public AssetLoader<T> LoadAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            return AssetLoader<T>.Get(bundleName, assetName);
        }

        /// <summary>
        /// 异步加载资源接口
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
        static public AssetLoaderAsync<T> LoadAssetAsync<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            
            return AssetLoaderAsync<T>.Get(bundleName, assetName);
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
        /// 3、sceneName不能带后缀名，scenePath必须带后缀名
        /// 4、sceneName, scenePath大小写不敏感
        /// </summary>
        /// <param name="sceneName"></param>
        static public SceneLoader LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");

            return SceneLoader.Get(sceneName, mode);
        }

        static public SceneLoaderAsync LoadSceneAsync(string sceneName, LoadSceneMode mode, bool allowSceneActivation = true)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");

            return SceneLoaderAsync.Get(sceneName, mode, allowSceneActivation);
        }

        /// <summary>
        /// “动态场景”同步加载接口
        /// 1、无需加入Build Settings
        /// 2、调用LoadScene之前必须把场景所在AB及依赖AB先载入
        /// 3、调用方式：LoadScene(sceneName) 不能带后缀名 OR  LoadScene(scenePath) 可带可不带后缀名
        /// 4、sceneName or scenePath大小写敏感
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        static public SceneLoader LoadScene(string bundlePath, string sceneName, LoadSceneMode mode)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");

            return SceneLoader.Get(bundlePath, sceneName, mode);
        }

        static public SceneLoaderAsync LoadSceneAsync(string bundlePath, string sceneName, LoadSceneMode mode, bool allowSceneActivation = true)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");

            return SceneLoaderAsync.Get(bundlePath, sceneName, mode, allowSceneActivation);
        }

        static public AsyncOperation UnloadSceneAsync(SceneLoader loader)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");

            return SceneLoader.Release(loader);
        }

        static public AsyncOperation UnloadSceneAsync(SceneLoaderAsync loader)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");

            return SceneLoaderAsync.Release(loader);
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
        static public bool ParseAssetPath(string assetPath, out string assetBundleName, out string assetName)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new System.ArgumentNullException(assetPath);
            }            

            AssetName an;
            if (s_dic.TryGetValue(assetPath, out an))
            {
                assetBundleName = an.assetBundleName;
                assetName = an.assetName;
                return true;
            }

            bool bSucess;
            if (CustomizedParser_AssetPathToBundleAndAsset != null)
            {
                bSucess = CustomizedParser_AssetPathToBundleAndAsset(assetPath, out assetBundleName, out assetName);
            }
            else
            {
                bSucess = DefaultParser_AssetPathToBundleAndAsset(assetPath, out assetBundleName, out assetName);
            }
            if (!bSucess)
                return false;

            an = new AssetName();
            an.assetBundleName = assetBundleName;
            an.assetName = assetName;
            s_dic.Add(assetPath, an);

            return true;
        }

        static public void ParseBundleAndAssetName(string assetBundleName, string assetName, out string assetPath)
        {
            if(CustomizedParser_BundleAndAssetToAssetPath != null)
            {
                CustomizedParser_BundleAndAssetToAssetPath(assetBundleName, assetName, out assetPath);
            }
            else
            {
                DefaultParser_BundleAndAssetToAssetPath(assetBundleName, assetName, out assetPath);
            }
        }

        /// <summary>
        /// 默认解析器——从assetPath解析出bundleName和assetName
        /// </summary>
        /// <param name="assetPath">"res/windows/test/cube.prefab"</param>
        /// <param name="assetBundleName">"res/windows/test.ab"</param>
        /// <param name="assetName">"cube.prefab"</param>
        /// <returns></returns>
        static private bool DefaultParser_AssetPathToBundleAndAsset(string assetPath, out string assetBundleName, out string assetName)
        {
            assetBundleName = null;
            assetName = null;

            int index = assetPath.LastIndexOf(@"/");
            if (index == -1)
                return false;

            assetName = assetPath.Substring(index + 1);
            assetBundleName = assetPath.Substring(0, index) + ".ab";

            return true;
        }

        static private void DefaultParser_BundleAndAssetToAssetPath(string assetBundleName, string assetName, out string assetPath)
        {
            if (assetBundleName.EndsWith(".ab", System.StringComparison.OrdinalIgnoreCase))
            {
                assetBundleName = assetBundleName.Substring(0, assetBundleName.Length - 3);
            }
            assetPath = assetBundleName + "/" + assetName;
        }
    }
}