using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetManagement.Runtime
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class AssetManager : MonoBehaviour
    {
        static public AssetManager  Instance                                    { get; private set; }

        public int                  PreAllocateAssetBundlePoolSize              = 200;                              // 预分配缓存AssetBundleRef对象池大小
        public int                  PreAllocateAssetBundleLoaderPoolSize        = 100;                              // 预分配缓存AssetBundleLoader对象池大小
        public int                  PreAllocateAssetLoaderPoolSize              = 50;                               // 预分配缓存AssetLoader对象池大小
        public int                  PreAllocateAssetLoaderAsyncPoolSize         = 50;                               // 预分配缓存AssetLoaderAsync对象池大小


        static private LoaderType   m_LoaderType;
        static private string       m_RootPath;
        static private bool         m_bInit;

        private void Start()
        {
            // 已有AssetManager，则自毁
            if (FindObjectsOfType<AssetManager>().Length > 1)
            {
                Debug.LogError($"AssetManager has already exist, destroy self");
                DestroyImmediate(this);
                return;
            }

            if (!m_bInit)
            {
                Debug.LogError($"AssetManager.m_bInit == false, plz call AssetManager.Init() first");
                return;
            }

            Instance = this;

            if (loaderType == LoaderType.FromAB)
            {
                AssetBundleManager.Init(m_RootPath, Utility.GetPlatformName());
            }
            Debug.Log($"AssetManager.loaderType is {loaderType}");
        }

        private void OnDestroy()
        {
            if (loaderType == LoaderType.FromAB)
            {
                AssetBundleManager.Uninit();
            }
            Instance = null;
        }

        /// <summary>
        /// 资源管理器初始化
        /// WARNING： 务必在AssetManager.Start()之前调用
        /// </summary>
        /// <param name="type">AB加载模式或直接从project中加载资源</param>
        /// <param name="bundleRootPath">bundle资源路径，仅限AB加载模式时有效</param>
        static public void Init(LoaderType type, string bundleRootPath = "Deployment/AssetBundles")
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

            m_bInit = true;

            if (GameObject.FindObjectOfType<AssetManager>() == null)
            {
                DontDestroyOnLoad(new GameObject("AssetManager", typeof(AssetManager)));
            }
        }

        static public void Uninit()
        {
            if(Instance != null)
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
            GameObject go = null;

            LinkedListNode<AssetLoader<GameObject>> loader = LoadAsset<GameObject>(assetPath);
            if (loader.Value.asset != null)
            {
                go = UnityEngine.Object.Instantiate(loader.Value.asset);

                GameObjectDestroyer destroyer = go.AddComponent<GameObjectDestroyer>();
                destroyer.loader = loader;
            }
            else
            {
                Debug.LogWarningFormat("InstantiatePrefab -- Failed to load asset[{0}]", assetPath);
            }

            return go;
        }

        static public GameObject InstantiatePrefab(string assetBundleName, string assetName)
        {
            GameObject go = null;

            LinkedListNode<AssetLoader<GameObject>> loader = LoadAsset<GameObject>(assetBundleName, assetName);
            if (loader.Value.asset != null)
            {
                go = UnityEngine.Object.Instantiate(loader.Value.asset);

                GameObjectDestroyer destroyer = go.AddComponent<GameObjectDestroyer>();
                destroyer.loader = loader;
            }
            else
            {
                Debug.LogWarningFormat("InstantiatePrefab -- Failed to load asset[{0}]", assetBundleName + "/" + assetName);
            }

            return go;
        }

        static public IEnumerator InstantiatePrefabAsync(string assetPath, Action<GameObject> handler = null)
        {
            GameObject go = null;

            LinkedListNode<AssetLoaderAsync<GameObject>> loaderAsync = LoadAssetAsync<GameObject>(assetPath);
            yield return loaderAsync.Value;
            if (loaderAsync.Value.asset != null)
            {
                go = UnityEngine.Object.Instantiate(loaderAsync.Value.asset);

                GameObjectDestroyer destroyer = go.AddComponent<GameObjectDestroyer>();
                destroyer.loaderAsync = loaderAsync;
            }
            else
            {
                Debug.LogWarningFormat("InstantiatePrefabAsync -- Failed to load asset[{0}]", assetPath);
            }

            handler?.Invoke(go);
        }

        static public IEnumerator InstantiatePrefabAsync(string assetBundleName, string assetName, Action<GameObject> handler = null)
        {
            GameObject go = null;

            LinkedListNode<AssetLoaderAsync<GameObject>> loaderAsync = LoadAssetAsync<GameObject>(assetBundleName, assetName);
            yield return loaderAsync.Value;
            if (loaderAsync.Value.asset != null)
            {
                go = UnityEngine.Object.Instantiate(loaderAsync.Value.asset);

                GameObjectDestroyer destroyer = go.AddComponent<GameObjectDestroyer>();
                destroyer.loaderAsync = loaderAsync;
            }
            else
            {
                Debug.LogWarningFormat("InstantiatePrefabAsync -- Failed to load asset[{0}]", assetBundleName + "/" + assetName);
            }

            handler?.Invoke(go);
        }

        /// <summary>
        /// 加载非Prefab资源接口（同步、异步共4个）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static public LinkedListNode<AssetLoader<T>> LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            return AssetLoader<T>.Get(assetPath);
        }

        static public LinkedListNode<AssetLoader<T>> LoadAsset<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
        {
            return AssetLoader<T>.Get(assetBundleName, assetName);
        }

        static public LinkedListNode<AssetLoaderAsync<T>> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object
        {
            return AssetLoaderAsync<T>.Get(assetPath);
        }

        static public LinkedListNode<AssetLoaderAsync<T>> LoadAssetAsync<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
        {
            return AssetLoaderAsync<T>.Get(assetBundleName, assetName);
        }

        /// <summary>
        /// ab加载接口
        /// </summary>
        static public LinkedListNode<AssetBundleLoader> LoadAssetBundle(string assetBundleName)
        {
            return AssetBundleLoader.Get(assetBundleName);
        }

        /// <summary>
        /// ab卸载接口
        /// </summary>
        /// <param name="abLoader"></param>
        static public void UnloadAssetBundle(LinkedListNode<AssetBundleLoader> abLoader)
        {
            AssetBundleLoader.Release(abLoader);
        }


        static public LoaderType loaderType
        {
            get
            {
#if UNITY_EDITOR
                if(Instance != null)
                {
                    return m_LoaderType;
                }
                return LoaderType.FromEditor;
#else
                return LoaderType.FromAB;       // 非编辑器模式强制AB加载
#endif
            }
        }

        static public int preAllocateAssetBundlePoolSize
        {
            get
            {
                return Instance?.PreAllocateAssetBundlePoolSize ?? 200;
            }
        }

        static public int preAllocateAssetBundleLoaderPoolSize
        {
            get
            {
                return Instance?.PreAllocateAssetBundleLoaderPoolSize ?? 100;
            }
        }

        static public int preAllocateAssetLoaderPoolSize
        {
            get
            {
                return Instance?.PreAllocateAssetLoaderPoolSize ?? 50;
            }
        }

        static public int preAllocateAssetLoaderAsyncPoolSize
        {
            get
            {
                return Instance?.PreAllocateAssetLoaderAsyncPoolSize ?? 50;
            }
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


