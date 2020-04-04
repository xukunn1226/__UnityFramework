using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AssetManagement.Runtime
{
    public class AssetManagerEx : MonoBehaviour
    {
        static public AssetManagerEx Instance { get; private set; }

        static public int           PreAllocateAssetBundlePoolSize        = 200;                              // 预分配缓存AssetBundleRef对象池大小
        static public int           PreAllocateAssetBundleLoaderPoolSize  = 100;                              // 预分配缓存AssetBundleLoader对象池大小
        static public int           PreAllocateAssetLoaderPoolSize        = 50;                               // 预分配缓存AssetLoader对象池大小
        static public int           PreAllocateAssetLoaderAsyncPoolSize   = 50;                               // 预分配缓存AssetLoaderAsync对象池大小
        
        static private LoaderType   m_LoaderType;
        static private string       m_RootPath;
        static private bool         m_bInit;

        private void Start()
        {
            // 已有AssetManager，则自毁
            if (FindObjectsOfType<AssetManagerEx>().Length > 1)
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
                AssetBundleManagerEx.Init(m_RootPath, Utility.GetPlatformName());
            }
            Debug.Log($"AssetManager.loaderType is {loaderType}");
        }

        private void OnDestroy()
        {
            if (loaderType == LoaderType.FromAB)
            {
                AssetBundleManagerEx.Uninit();
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

            if (GameObject.FindObjectOfType<AssetManagerEx>() == null)
            {
                DontDestroyOnLoad(new GameObject("[AssetManager]", typeof(AssetManagerEx)));
            }
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
            GameObject go = null;

            AssetLoaderEx<GameObject> loader = LoadAsset<GameObject>(assetPath);
            if (loader.asset != null)
            {
                go = UnityEngine.Object.Instantiate(loader.asset);

                GameObjectDestroyerEx destroyer = go.AddComponent<GameObjectDestroyerEx>();
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

            AssetLoaderEx<GameObject> loader = LoadAsset<GameObject>(assetBundleName, assetName);
            if (loader.asset != null)
            {
                go = UnityEngine.Object.Instantiate(loader.asset);

                GameObjectDestroyerEx destroyer = go.AddComponent<GameObjectDestroyerEx>();
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

            AssetLoaderAsyncEx<GameObject> loaderAsync = LoadAssetAsync<GameObject>(assetPath);
            yield return loaderAsync;
            if (loaderAsync.asset != null)
            {
                go = UnityEngine.Object.Instantiate(loaderAsync.asset);

                GameObjectDestroyerEx destroyer = go.AddComponent<GameObjectDestroyerEx>();
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

            AssetLoaderAsyncEx<GameObject> loaderAsync = LoadAssetAsync<GameObject>(assetBundleName, assetName);
            yield return loaderAsync;
            if (loaderAsync.asset != null)
            {
                go = UnityEngine.Object.Instantiate(loaderAsync.asset);

                GameObjectDestroyerEx destroyer = go.AddComponent<GameObjectDestroyerEx>();
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
        static public AssetLoaderEx<T> LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            return AssetLoaderEx<T>.Get(assetPath);
        }

        static public AssetLoaderEx<T> LoadAsset<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
        {
            return AssetLoaderEx<T>.Get(assetBundleName, assetName);
        }

        static public AssetLoaderAsyncEx<T> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object
        {
            return AssetLoaderAsyncEx<T>.Get(assetPath);
        }

        static public AssetLoaderAsyncEx<T> LoadAssetAsync<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
        {
            return AssetLoaderAsyncEx<T>.Get(assetBundleName, assetName);
        }

        static public void UnloadAsset<T>(AssetLoaderEx<T> loader) where T : UnityEngine.Object
        {
            AssetLoaderEx<T>.Release(loader);
        }

        static public void UnloadAsset<T>(AssetLoaderAsyncEx<T> loader) where T : UnityEngine.Object
        {
            AssetLoaderAsyncEx<T>.Release(loader);
        }

        /// <summary>
        /// ab加载接口
        /// </summary>
        static public AssetBundleLoaderEx LoadAssetBundle(string assetBundleName)
        {
            return AssetBundleLoaderEx.Get(assetBundleName);
        }

        /// <summary>
        /// ab卸载接口
        /// </summary>
        /// <param name="abLoader"></param>
        static public void UnloadAssetBundle(AssetBundleLoaderEx abLoader)
        {
            AssetBundleLoaderEx.Release(abLoader);
        }


        static public LoaderType loaderType
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
                return LoaderType.FromAB;       // 非编辑器模式强制AB加载
#endif
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