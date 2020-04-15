using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Framework.Core;

namespace Framework.AssetManagement.Runtime
{
    public class AssetManager : MonoBehaviour
    {
        static internal AssetManager    Instance { get; private set; }

        static public int               PreAllocateAssetBundlePoolSize        = 200;                              // 预分配缓存AssetBundleRef对象池大小
        static public int               PreAllocateAssetBundleLoaderPoolSize  = 100;                              // 预分配缓存AssetBundleLoader对象池大小
        static public int               PreAllocateAssetLoaderPoolSize        = 50;                               // 预分配缓存AssetLoader对象池大小
        static public int               PreAllocateAssetLoaderAsyncPoolSize   = 50;                               // 预分配缓存AssetLoaderAsync对象池大小
        
        static private LoaderType       m_LoaderType;
        static private string           m_RootPath;
        static private bool             m_bInit;

        static internal LoaderType      loaderType
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
                // 确保应用层持有的AssetLoader(Async)释放后再调用
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
                DontDestroyOnLoad(new GameObject("[AssetManager]", typeof(AssetManager)));
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
            return AssetLoaderAsync<T>.Get(assetBundleName, assetName);
        }

        static public void UnloadAsset<T>(AssetLoader<T> loader) where T : UnityEngine.Object
        {
            AssetLoader<T>.Release(loader);
        }

        static public void UnloadAsset<T>(AssetLoaderAsync<T> loader) where T : UnityEngine.Object
        {
            AssetLoaderAsync<T>.Release(loader);
        }

        /// <summary>
        /// ab加载接口
        /// </summary>
        static public AssetBundleLoader LoadAssetBundle(string assetBundleName)
        {
            return AssetBundleLoader.Get(assetBundleName);
        }

        /// <summary>
        /// ab卸载接口
        /// </summary>
        /// <param name="abLoader"></param>
        static public void UnloadAssetBundle(AssetBundleLoader abLoader)
        {
            AssetBundleLoader.Release(abLoader);
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