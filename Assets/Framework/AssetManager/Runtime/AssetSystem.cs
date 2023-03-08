using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.SceneManagement;

namespace Framework.AssetManagement.Runtime
{
    internal class AssetSystem
    {
        private IndexedSet<BundleLoaderBase>            m_BundleLoaderSet   = new IndexedSet<BundleLoaderBase>();
        private Dictionary<string, BundleLoaderBase>    m_BundleLoaderDict  = new Dictionary<string, BundleLoaderBase>(1000);
        private IndexedSet<ProviderBase>                m_ProviderSet       = new IndexedSet<ProviderBase>();
        private Dictionary<string, ProviderBase>        m_ProviderDict      = new Dictionary<string, ProviderBase>(1000);
        private IndexedSet<ProviderBase>                m_ProviderSetHigh   = new IndexedSet<ProviderBase>();
        private Dictionary<string, ProviderBase>        m_ProviderDictHigh  = new Dictionary<string, ProviderBase>(100);
        private IndexedSet<ProviderBase>                m_ProviderSetLow    = new IndexedSet<ProviderBase>();
        private Dictionary<string, ProviderBase>        m_ProviderDictLow   = new Dictionary<string, ProviderBase>(100);

        private readonly Dictionary<string, SceneOperationHandle> m_SceneHandlesDict = new Dictionary<string, SceneOperationHandle>(100);
        private long s_SceneCreateCount = 0;

        private EPlayMode           m_PlayMode;
        public IDecryptionServices  decryptionServices  { get; private set; }
        public IBundleServices      bundleServices      { get; private set; }
        private int                 m_AssetLoadingMaxNumber;

        public InitializationOperation InitializeAsync(InitializeParameters parameters)
        {
            InitializationOperation initializeOperation = null;
            if (parameters.PlayMode == EPlayMode.FromEditor)
            {
                var impl = new EditorSimulateModeImpl();
                parameters.BundleServices = impl;
                initializeOperation = impl.InitializeAsync(parameters.LocationToLower);
            }
            else
            {
                var impl = new OfflinePlayModeImpl();
                parameters.BundleServices = impl;
                initializeOperation = impl.InitializeAsync(parameters.LocationToLower);
            }

            m_PlayMode              = parameters.PlayMode;
            decryptionServices      = parameters.DecryptionServices;
            bundleServices          = parameters.BundleServices;
            m_AssetLoadingMaxNumber = parameters.AssetLoadingMaxNumber;

            return initializeOperation;
        }

        public void Update()
        {
            for(int i = 0; i < m_BundleLoaderSet.Count; ++i)
            {
                m_BundleLoaderSet[i].Update();
            }

            PollingProviders();

            UnloadUnusedAssets();
        }

        private void PollingProviders()
        {
            // polling high priority queue
            int loadingCount = 0;
            for (int i = 0; i < m_ProviderSetHigh.Count; ++i)
            {
                if (loadingCount < m_AssetLoadingMaxNumber)
                    m_ProviderSetHigh[i].Update();

                if (!m_ProviderSetHigh[i].isDone)
                    ++loadingCount;
            }

            // polling normal priority queue
            for (int i = 0; i < m_ProviderSet.Count; ++i)
            {
                if (m_ProviderSet[i].IsSceneProvider())
                { // 不能阻塞场景的加载
                    m_ProviderSet[i].Update();
                }
                else
                {
                    if (loadingCount < m_AssetLoadingMaxNumber)
                        m_ProviderSet[i].Update();

                    if (!m_ProviderSet[i].isDone)
                        ++loadingCount;
                }
            }

            // polling low priority queue
            for (int i = 0; i < m_ProviderSetLow.Count; ++i)
            {
                if (loadingCount < m_AssetLoadingMaxNumber)
                    m_ProviderSetLow[i].Update();

                if (!m_ProviderSetLow[i].isDone)
                    ++loadingCount;
            }
        }

        public void Destroy()
        {
            ClearAllProviders();

            for (int i = 0; i < m_BundleLoaderSet.Count; ++i)
            {
                m_BundleLoaderSet[i].Destroy(true);
            }
            m_BundleLoaderSet.Clear();
            m_BundleLoaderDict.Clear();

            ClearSceneHandle();

            decryptionServices = null;
            bundleServices = null;
        }

        private void ClearAllProviders()
        {
            for (int i = 0; i < m_ProviderSet.Count; ++i)
            {
                m_ProviderSet[i].Destroy();
            }
            m_ProviderSet.Clear();
            m_ProviderDict.Clear();

            for (int i = 0; i < m_ProviderSetHigh.Count; ++i)
            {
                m_ProviderSetHigh[i].Destroy();
            }
            m_ProviderSetHigh.Clear();
            m_ProviderDictHigh.Clear();

            for (int i = 0; i < m_ProviderSetLow.Count; ++i)
            {
                m_ProviderSetLow[i].Destroy();
            }
            m_ProviderSetLow.Clear();
            m_ProviderDictLow.Clear();
        }

        /// <summary>
        /// 卸载引用计数为0的资源
        /// </summary>
        public void UnloadUnusedAssets()
        {
            // 遍历资源提供者，引用计数为0的销毁
            for(int i = m_ProviderSet.Count - 1; i >= 0; --i)
            {
                ProviderBase provider = m_ProviderSet[i];
                if (provider.canDestroy)
                {
                    provider.Destroy();

                    m_ProviderDict.Remove(provider.providerGUID);
                    m_ProviderSet.RemoveAt(i);
                }
            }
            for (int i = m_ProviderSetHigh.Count - 1; i >= 0; --i)
            {
                ProviderBase provider = m_ProviderSetHigh[i];
                if (provider.canDestroy)
                {
                    provider.Destroy();

                    m_ProviderDictHigh.Remove(provider.providerGUID);
                    m_ProviderSetHigh.RemoveAt(i);
                }
            }
            for (int i = m_ProviderSetLow.Count - 1; i >= 0; --i)
            {
                ProviderBase provider = m_ProviderSetLow[i];
                if (provider.canDestroy)
                {
                    provider.Destroy();

                    m_ProviderDictLow.Remove(provider.providerGUID);
                    m_ProviderSetLow.RemoveAt(i);
                }
            }

            // 编辑器模拟模式不会产生BundleLoader
            if (m_PlayMode != EPlayMode.FromEditor)
            {
                for (int i = m_BundleLoaderSet.Count - 1; i >= 0; --i)
                {
                    var loader = m_BundleLoaderSet[i];
                    if (loader.canDestroy())
                    {
                        loader.Destroy(false);

                        m_BundleLoaderDict.Remove(loader.bundleInfo.descriptor.bundleName);
                        m_BundleLoaderSet.RemoveAt(i);
                    }
                }
            }
        }

        public void ForceUnloadAllAssets()
        {
            ClearAllProviders();

            for (int i = 0; i < m_BundleLoaderSet.Count; ++i)
            {
                m_BundleLoaderSet[i].Destroy(true);
            }
            m_BundleLoaderSet.Clear();
            m_BundleLoaderDict.Clear();
            ClearSceneHandle();

            Resources.UnloadUnusedAssets();
        }

        internal BundleLoaderBase CreateMainAssetBundleLoader(AssetInfo assetInfo)
        {
            BundleInfo bundleInfo = bundleServices.GetBundleInfo(assetInfo);
            return CreateAssetBundleLoaderInternal(bundleInfo);
        }

        internal List<BundleLoaderBase> CreateDependAssetBundleLoaders(AssetInfo assetInfo)
        {
            BundleInfo[] depends = bundleServices.GetAllDependBundleInfos(assetInfo);
            List<BundleLoaderBase> result = new List<BundleLoaderBase>(depends.Length);
            foreach (var bundleInfo in depends)
            {
                BundleLoaderBase dependLoader = CreateAssetBundleLoaderInternal(bundleInfo);
                result.Add(dependLoader);
            }
            return result;
        }

        private BundleLoaderBase CreateAssetBundleLoaderInternal(BundleInfo bundleInfo)
        {
            BundleLoaderBase loader = TryGetAssetBundleLoader(bundleInfo.descriptor.bundleName);
            if (loader != null)
                return loader;

            if (bundleInfo.descriptor.isRawFile)
                loader = new RawBundleLoader(this, bundleInfo);
            else
                loader = new AssetBundleLoader(this, bundleInfo);

            m_BundleLoaderDict.Add(loader.bundleInfo.descriptor.bundleName, loader);
            m_BundleLoaderSet.AddUnique(loader);
            return loader;
        }

        /// <summary>
        /// 检查资源包的销毁状态
        /// </summary>
        /// <param name="bundleID"></param>
        /// <returns></returns>
        internal bool CheckBundleDestroyed(int bundleID)
		{
			string bundleName = bundleServices.GetBundleName(bundleID);
			BundleLoaderBase loader = TryGetAssetBundleLoader(bundleName);
			if (loader == null)
				return true;
			return loader.isDestroyed;
		}

        private BundleLoaderBase TryGetAssetBundleLoader(string bundleName)
        {
            m_BundleLoaderDict.TryGetValue(bundleName, out BundleLoaderBase loader);
            return loader;
        }

        private ProviderBase TryGetProvider(string providerGUID)
        {
            if(m_ProviderDict.TryGetValue(providerGUID, out ProviderBase provider))
                return provider;
            if(m_ProviderDictHigh.TryGetValue(providerGUID, out provider))
                return provider;
            if(m_ProviderDictLow.TryGetValue(providerGUID, out provider))
                return provider;
            return null;
        }

        private void AddProvider(string providerGUID, ProviderBase provider, ELoadingPriority priority = ELoadingPriority.Normal)
        {
            if (priority == ELoadingPriority.Normal)
            {
                m_ProviderDict.Add(providerGUID, provider);
                m_ProviderSet.AddUnique(provider);
            }
            else if(priority == ELoadingPriority.High)
            {
                m_ProviderDictHigh.Add(providerGUID, provider);
                m_ProviderSetHigh.AddUnique(provider);
            }
            else if(priority == ELoadingPriority.Low)
            {
                m_ProviderDictLow.Add(providerGUID, provider);
                m_ProviderSetLow.AddUnique(provider);
            }
            else
            {
                throw new System.Exception($"should never get here");
            }
        }

        private AssetInfo ConvertLocationToAssetInfo(string assetPath, System.Type assetType)
        {
            DebugCheckLocation(assetPath);
            AssetDescriptor patchAsset = bundleServices.TryGetAssetDesc(assetPath);
            if (patchAsset != null)
            {
                AssetInfo assetInfo = new AssetInfo(patchAsset, assetType);
                return assetInfo;
            }
            else
            {
                string error;
                if (string.IsNullOrEmpty(assetPath))
                    error = $"The location is null or empty !";
                else
                    error = $"The location is invalid : {assetPath}";
                AssetInfo assetInfo = new AssetInfo(error);
                return assetInfo;
            }
        }

        [Conditional("DEBUG")]
        private void DebugCheckLocation(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) == false)
            {
                // 检查路径末尾是否有空格
                int index = assetPath.LastIndexOf(" ");
                if (index != -1)
                {
                    if (assetPath.Length == index + 1)
                        UnityEngine.Debug.LogWarning($"Found blank character in location : \"{assetPath}\"");
                }

                if (assetPath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
                    UnityEngine.Debug.LogWarning($"Found illegal character in location : \"{assetPath}\"");
            }
        }

        #region 原生资源加载接口
        public RawFileOperationHandle LoadRawFile(string assetPath)
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, null);
            return LoadRawFileInternal(assetInfo, true);
        }

        public RawFileOperationHandle LoadRawFileAsync(string assetPath)
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, null);
            return LoadRawFileInternal(assetInfo, false);
        }

        private RawFileOperationHandle LoadRawFileInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
        {
#if UNITY_EDITOR
            if (assetInfo.isValid && m_PlayMode != EPlayMode.FromEditor)
            {
                BundleInfo bundleInfo = bundleServices.GetBundleInfo(assetInfo);
                if (bundleInfo.descriptor.isRawFile == false)
                    throw new System.Exception($"Cannot load asset bundle file using {nameof(LoadRawFileAsync)} method !");
            }
#endif

            var handle = LoadRawFileAsync(assetInfo);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }

        private RawFileOperationHandle LoadRawFileAsync(AssetInfo assetInfo)
        {
            if (!assetInfo.isValid)
            {
                UnityEngine.Debug.LogError($"Failed to load raw file ! {assetInfo.lastError}");
                CompletedProvider completedProvider = new CompletedProvider(assetInfo);
                completedProvider.SetCompleted(assetInfo.lastError);
                return completedProvider.CreateHandle<RawFileOperationHandle>();
            }

            string providerGUID = assetInfo.guid;
            ProviderBase provider = TryGetProvider(providerGUID);
            if (provider == null)
            {
                if (m_PlayMode == EPlayMode.FromEditor)
                    provider = new DatabaseRawFileProvider(this, providerGUID, assetInfo);
                else
                    provider = new BundleRawFileProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                AddProvider(providerGUID, provider);
            }
            return provider.CreateHandle<RawFileOperationHandle>();
        }
        #endregion      // 原生资源加载接口

        #region 资源加载接口（同步&异步）
        /// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="assetPath">资源的定位地址</param>
		public AssetOperationHandle LoadAsset<TObject>(string assetPath) where TObject : UnityEngine.Object
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, typeof(TObject));
            return LoadAssetInternal(assetInfo, true);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="assetPath">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public AssetOperationHandle LoadAsset(string assetPath, System.Type type)
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, type);
            return LoadAssetInternal(assetInfo, true);
        }

        /// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="assetPath">资源的定位地址</param>
		public AssetOperationHandle LoadAssetAsync<TObject>(string assetPath, ELoadingPriority priority = ELoadingPriority.Normal) where TObject : UnityEngine.Object
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, typeof(TObject));
            return LoadAssetInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="assetPath">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public AssetOperationHandle LoadAssetAsync(string assetPath, System.Type type, ELoadingPriority priority = ELoadingPriority.Normal)
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, type);
            return LoadAssetInternal(assetInfo, false, priority);
        }

        private AssetOperationHandle LoadAssetInternal(AssetInfo assetInfo, bool waitForAsyncComplete, ELoadingPriority priority = ELoadingPriority.Normal)
        {
#if UNITY_EDITOR
            if (assetInfo.isValid && m_PlayMode != EPlayMode.FromEditor)
            {
                BundleInfo bundleInfo = bundleServices.GetBundleInfo(assetInfo);
                if (bundleInfo.descriptor.isRawFile)
                    throw new System.Exception($"Cannot load raw file using {nameof(LoadAssetAsync)} method !");
            }
#endif

            var handle = LoadAssetAsync(assetInfo, priority);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }

        private AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo, ELoadingPriority priority = ELoadingPriority.Normal)
        {
            if (!assetInfo.isValid)
            {
                UnityEngine.Debug.LogError($"Failed to load asset ! {assetInfo.lastError}");
                CompletedProvider completedProvider = new CompletedProvider(assetInfo);
                completedProvider.SetCompleted(assetInfo.lastError);
                return completedProvider.CreateHandle<AssetOperationHandle>();
            }

            string providerGUID = assetInfo.guid;
            ProviderBase provider = TryGetProvider(providerGUID);
            if (provider == null)
            {
                if (m_PlayMode == EPlayMode.FromEditor)
                    provider = new DatabaseAssetProvider(this, providerGUID, assetInfo);
                else
                    provider = new BundleAssetProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                AddProvider(providerGUID, provider, priority);
            }
            return provider.CreateHandle<AssetOperationHandle>();
        }

        /// <summary>
        /// 同步加载Prefab资源对象
        /// </summary>
        /// <param name="assetPath">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public PrefabOperationHandle LoadPrefab(string assetPath, Transform parent, Vector3 pos, Quaternion rot)
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, typeof(GameObject));
            return LoadPrefabInternal(assetInfo, parent, pos, rot, true);
        }

        /// <summary>
        /// 异步加载Prefab资源对象
        /// </summary>
        /// <param name="assetPath">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public PrefabOperationHandle LoadPrefabAsync(string assetPath, Transform parent, Vector3 pos, Quaternion rot, ELoadingPriority priority = ELoadingPriority.Normal)
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, typeof(GameObject));
            return LoadPrefabInternal(assetInfo, parent, pos, rot, false, priority);
        }

        private PrefabOperationHandle LoadPrefabInternal(AssetInfo assetInfo, Transform parent, Vector3 pos, Quaternion rot, bool waitForAsyncComplete, ELoadingPriority priority = ELoadingPriority.Normal)
        {
#if UNITY_EDITOR
            if (assetInfo.isValid && m_PlayMode != EPlayMode.FromEditor)
            {
                BundleInfo bundleInfo = bundleServices.GetBundleInfo(assetInfo);
                if (bundleInfo.descriptor.isRawFile)
                    throw new System.Exception($"Cannot load raw file using {nameof(LoadPrefabAsync)} method !");
            }
#endif

            var handle = LoadPrefabAsync(assetInfo, parent, pos, rot, priority);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }

        private PrefabOperationHandle LoadPrefabAsync(AssetInfo assetInfo, Transform parent, Vector3 pos, Quaternion rot, ELoadingPriority priority = ELoadingPriority.Normal)
        {
            if (!assetInfo.isValid)
            {
                UnityEngine.Debug.LogError($"Failed to load prefab ! {assetInfo.lastError}");
                CompletedProvider completedProvider = new CompletedProvider(assetInfo);
                completedProvider.SetCompleted(assetInfo.lastError);
                return completedProvider.CreateHandle(null, Vector3.zero, Quaternion.identity);
            }

            string providerGUID = assetInfo.guid;
            ProviderBase provider = TryGetProvider(providerGUID);
            if (provider == null)
            {
                if (m_PlayMode == EPlayMode.FromEditor)
                    provider = new DatabasePrefabProvider(this, providerGUID, assetInfo);
                else
                    provider = new BundlePrefabProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                AddProvider(providerGUID, provider, priority);
            }
            return provider.CreateHandle(parent, pos, rot);
        }

        public SubAssetsOperationHandle LoadSubAssets<TObject>(string assetPath) where TObject : UnityEngine.Object
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, typeof(TObject));
            return LoadSubAssetsInternal(assetInfo, true);
        }

        public SubAssetsOperationHandle LoadSubAssets(string assetPath, System.Type type)
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, type);
            return LoadSubAssetsInternal(assetInfo, true);
        }

        public SubAssetsOperationHandle LoadSubAssetsAsync<TObject>(string assetPath, ELoadingPriority priority = ELoadingPriority.Normal) where TObject : UnityEngine.Object
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, typeof(TObject));
            return LoadSubAssetsInternal(assetInfo, false, priority);
        }

        public SubAssetsOperationHandle LoadSubAssetsAsync(string assetPath, System.Type type, ELoadingPriority priority = ELoadingPriority.Normal)
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, type);
            return LoadSubAssetsInternal(assetInfo, false, priority);
        }

        private SubAssetsOperationHandle LoadSubAssetsInternal(AssetInfo assetInfo, bool waitForAsyncComplete, ELoadingPriority priority = ELoadingPriority.Normal)
        {
#if UNITY_EDITOR
            if (assetInfo.isValid && m_PlayMode != EPlayMode.FromEditor)
            {
                BundleInfo bundleInfo = bundleServices.GetBundleInfo(assetInfo);
                if (bundleInfo.descriptor.isRawFile)
                    throw new System.Exception($"Cannot load raw file using {nameof(LoadSubAssetsAsync)} method !");
            }
#endif

            var handle = LoadSubAssetsAsync(assetInfo, priority);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }

        private SubAssetsOperationHandle LoadSubAssetsAsync(AssetInfo assetInfo, ELoadingPriority priority = ELoadingPriority.Normal)
        {
            if (!assetInfo.isValid)
            {
                UnityEngine.Debug.LogError($"Failed to load sub assets ! {assetInfo.lastError}");
                CompletedProvider completedProvider = new CompletedProvider(assetInfo);
                completedProvider.SetCompleted(assetInfo.lastError);
                return completedProvider.CreateHandle<SubAssetsOperationHandle>();
            }

            string providerGUID = assetInfo.guid;
            ProviderBase provider = TryGetProvider(providerGUID);
            if (provider == null)
            {
                if (m_PlayMode == EPlayMode.FromEditor)
                    provider = new DatabaseSubAssetsProvider(this, providerGUID, assetInfo);
                else
                    provider = new BundleSubAssetsProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                AddProvider(providerGUID, provider, priority);
            }
            return provider.CreateHandle<SubAssetsOperationHandle>();
        }
        #endregion  // 资源加载接口（同步&异步）

        #region 场景加载接口
        /// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="location">场景的定位地址</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public SceneOperationHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            var handle = LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
            return handle;
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetInfo">场景的资源信息</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="activateOnLoad">加载完毕时是否主动激活</param>
        /// <param name="priority">优先级</param>        
		private SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad, int priority)
        {
            if (!assetInfo.isValid)
            {
                UnityEngine.Debug.LogError($"Failed to load scene ! {assetInfo.lastError}");
                CompletedProvider completedProvider = new CompletedProvider(assetInfo);
                completedProvider.SetCompleted(assetInfo.lastError);
                return completedProvider.CreateHandle<SceneOperationHandle>();
            }

            // 如果加载的是主场景，则卸载所有缓存的场景
            if (sceneMode == LoadSceneMode.Single)
            {
                UnloadAllScene();
            }

            // 注意：同一个场景的ProviderGUID每次加载都会变化，因为同一个场景可以以Additive方式多次加载，guid不足以表示唯一
            string providerGUID = $"{assetInfo.guid}-{++s_SceneCreateCount}";
            ProviderBase provider;
            {
                if (m_PlayMode == EPlayMode.FromEditor)
                    provider = new DatabaseSceneProvider(this, providerGUID, assetInfo, sceneMode, activateOnLoad, priority);
                else
                    provider = new BundleSceneProvider(this, providerGUID, assetInfo, sceneMode, activateOnLoad, priority);
                provider.InitSpawnDebugInfo();
                AddProvider(providerGUID, provider);
            }

            var handle = provider.CreateHandle<SceneOperationHandle>();
            m_SceneHandlesDict.Add(providerGUID, handle);
            return handle;
        }

        internal void UnloadSubScene(ProviderBase provider)
        {
            string providerGUID = provider.providerGUID;
            if (m_SceneHandlesDict.ContainsKey(providerGUID) == false)
                throw new System.Exception("Should never get here !");

            // 释放子场景句柄
            m_SceneHandlesDict[providerGUID].ReleaseInternal();
            m_SceneHandlesDict.Remove(providerGUID);

            // 卸载未被使用的资源（包括场景）
            UnloadUnusedAssets();
        }

        private void UnloadAllScene()
        {
            // 释放所有场景句柄
            foreach (var valuePair in m_SceneHandlesDict)
            {
                valuePair.Value.ReleaseInternal();
            }
            m_SceneHandlesDict.Clear();

            // 卸载未被使用的资源（包括场景）
            UnloadUnusedAssets();
        }

        internal void ClearSceneHandle()
        {
            // 释放资源包下的所有场景
            m_SceneHandlesDict.Clear();
        }

        #endregion  // 场景加载接口

        /////////////////////////////////// 调式信息
        public List<DebugProviderInfo> GetDebugProviderInfos()
        {
            List<DebugProviderInfo> infos = new List<DebugProviderInfo>(m_ProviderSet.Count);
            for(int i = 0; i < m_ProviderSet.Count; ++i)
            {
                DebugProviderInfo info = new DebugProviderInfo();
                m_ProviderSet[i].GetProviderDebugInfo(ref info);
                infos.Add(info);
            }
            return infos;
        }
    }
}