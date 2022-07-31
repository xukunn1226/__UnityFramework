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
    /// 3、全局唯一
    
    /// <summary>
    /// 生成AssetManager有两种方式：
    /// 1、脚本动态生成：AssetManager.Init();
    /// 2、静态挂载
    /// 注意事项：
    /// 1、两种方式只能选其一，同时使用概不负责
    /// </summary>
    public sealed class AssetManager : SingletonMono<AssetManager>
    {
        static internal int             PreAllocateAssetBundlePoolSize          = 200;                      // 预分配缓存AssetBundleRef对象池大小
        static internal int             PreAllocateAssetBundleLoaderPoolSize    = 100;                      // 预分配缓存AssetBundleLoader对象池大小
        static internal int             PreAllocateAssetLoaderPoolSize          = 50;                       // 预分配缓存AssetLoader对象池大小
        static internal int             PreAllocateAssetLoaderAsyncPoolSize     = 50;                       // 预分配缓存AssetLoaderAsync对象池大小

#pragma warning disable CS0649
        [SerializeField]
        private LoaderType              m_LoaderType;                                                       // 资源加载方式（readonly）
#pragma warning restore CS0649
        static private bool             bOverrideLoaderType;
        static private LoaderType       overrideLoaderType;
        static private CustomManifest   m_CustomManifest;

        public LoaderType               loaderType
        {
            get
            {
#if UNITY_EDITOR
                LoaderType finalType = bOverrideLoaderType ? overrideLoaderType : m_LoaderType;
                return finalType;
#else
                LoaderType finalType = bOverrideLoaderType ? overrideLoaderType : m_LoaderType;
                return finalType == LoaderType.FromEditor ? LoaderType.FromStreamingAssets : finalType;
#endif
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            switch(loaderType)
            {
                case LoaderType.FromStreamingAssets:
                    LoadManifest(Application.streamingAssetsPath);
                    AssetBundleManager.Init(true);
                    break;
                case LoaderType.FromPersistent:
                    LoadManifest(Application.persistentDataPath);
                    AssetBundleManager.Init(false);
                    break;
            }
            // Debug.Log($"AssetManager.loaderType is {loaderType}");
        }

        protected override void OnDestroy()
        {
            if (loaderType == LoaderType.FromStreamingAssets || loaderType == LoaderType.FromPersistent)
            {
                // 确保应用层持有的AssetLoader(Async)释放后再调用
                AssetBundleManager.Uninit();
            }

            base.OnDestroy();
        }

        private CustomManifest LoadManifest(string rootPath)
        {
            string assetPath = string.Format("{0}/{1}/manifest", rootPath, Utility.GetPlatformName());
            AssetBundle manifest = AssetBundle.LoadFromFile(assetPath);
            if (manifest != null)
            {
                JsonAsset asset = manifest.LoadAsset<JsonAsset>("manifest");
                m_CustomManifest = asset.Require<CustomManifest>();
                manifest.Unload(false);
            }
            if (m_CustomManifest == null)
                Debug.LogError("LoadManifest failed becase of asset bundle manifest == null");
            return m_CustomManifest;
        }

        /// <summary>
        /// 资源管理器初始化
        /// </summary>
        /// <param name="type">AB加载模式或直接从project中加载资源</param>
        static public AssetManager Init(LoaderType type)
        {
            bOverrideLoaderType = true;
            overrideLoaderType = type;

            GameObject go = new GameObject("[AssetManager]", typeof(AssetManager));

            return go.GetComponent<AssetManager>();
        }

        static public void Uninit()
        {
            if (Instance != null)
            {
                Destroy(Instance);
            }
            bOverrideLoaderType = false;
        }

        static public CustomManifest.BundleDetail GetBundleDetail(string assetBundleName)
        {
            if (m_CustomManifest == null)
                throw new System.ArgumentNullException("CustomManifest");
            return m_CustomManifest.GetBundleDetail(assetBundleName);
        }

        static public CustomManifest.FileDetail GetFileDetail(string assetPath)
        {
            if (m_CustomManifest == null)
                throw new System.ArgumentNullException("CustomManifest");
            return m_CustomManifest.GetFileDetail(assetPath);
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
        
        static public void ReleaseInst(GameObjectLoaderAsync loader)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
                
            GameObjectLoaderAsync.Release(loader);
        }
        
        static public GameObject InstantiatePrefab(string assetPath)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            return PrefabLoader.Get(assetPath);
        }

        static public PrefabLoaderAsync InstantiatePrefabAsync(string assetPath)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            return PrefabLoaderAsync.Get(assetPath);
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
        static public AssetBundleLoader LoadAssetBundle(string assetPath)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");
            CustomManifest.FileDetail fd = AssetManager.GetFileDetail(assetPath);
            return AssetBundleLoader.Get(fd.bundleName);
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
        /// load scene that is in Build Settings
        /// 1、场景必须加入Build settings
        /// 2、不可热更
        /// 3、调用方式：
        ///    assetPath：完整路径名，必须带后缀名, 大小写不敏感
        /// </summary>
        /// <param name="assetPath"></param>
        static public SceneLoader LoadScene(string assetPath, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");

            return SceneLoader.Get(assetPath, mode);
        }

        /// <summary>
        /// 同上
        /// </summary>
        /// <param name="assetPath"></param>
        static public SceneLoaderAsync LoadSceneAsync(string assetPath, LoadSceneMode mode, bool allowSceneActivation = true)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");

            return SceneLoaderAsync.Get(assetPath, mode, allowSceneActivation);
        }

        /// <summary>
        /// load scene from bunlde
        /// 1、无需加入Build Settings
        /// 2、调用LoadScene之前必须把场景所在AB先载入
        /// 3、调用方式：LoadScene(sceneName) 不能带后缀名，且需小写
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        static public SceneLoader LoadSceneFromBundle(string assetPath, LoadSceneMode mode)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");

            return SceneLoader.GetFromBundle(assetPath, mode);
        }

        /// <summary>
        /// 同上
        /// </summary>
        static public SceneLoaderAsync LoadSceneFromBundleAsync(string assetPath, LoadSceneMode mode, bool allowSceneActivation = true)
        {
            if (Instance == null)
                throw new System.ArgumentNullException("Instance", "AssetManager not initialized.");

            return SceneLoaderAsync.GetFromBundle(assetPath, mode, allowSceneActivation);
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
    }
}