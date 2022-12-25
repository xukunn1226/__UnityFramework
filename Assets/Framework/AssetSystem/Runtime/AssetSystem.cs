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

        private readonly Dictionary<string, SceneOperationHandle> m_SceneHandlesDict = new Dictionary<string, SceneOperationHandle>(100);
        private long s_SceneCreateCount = 0;

        private EPlayMode           m_PlayMode;
        public IDecryptionServices  decryptionServices  { get; private set; }
        public IBundleServices      bundleServices      { get; private set; }

        public InitializationOperation InitializeAsync(InitializeParameters parameters)
        {
            InitializationOperation initializeOperation = null;
            if (parameters.PlayMode == EPlayMode.FromEditor)
            {
                var impl = new EditorSimulateModeImpl();
                parameters.BundleServices = impl;
                initializeOperation = impl.InitializeAsync(false);
            }
            else
            {
                var impl = new OfflinePlayModeImpl();
                parameters.BundleServices = impl;
                initializeOperation = impl.InitializeAsync(false);
            }
            Initialize(parameters.PlayMode, parameters.DecryptionServices, parameters.BundleServices);

            return initializeOperation;
        }

        private void Initialize(EPlayMode playMode, IDecryptionServices decryptionServices, IBundleServices bundleServices)
        {
            m_PlayMode = playMode;
            this.decryptionServices = decryptionServices;
            this.bundleServices = bundleServices;
        }

        public void Update()
        {
            for(int i = 0; i < m_BundleLoaderSet.Count; ++i)
            {
                m_BundleLoaderSet[i].Update();
            }

            for(int i = 0; i < m_ProviderSet.Count; ++i)
            {
                if (m_ProviderSet[i].IsSceneProvider())
                { // �������������ļ���
                    m_ProviderSet[i].Update();
                }
                else
                {
                    m_ProviderSet[i].Update();
                }
            }

            // for debug
            UnloadUnusedAssets();
        }

        public void Destroy()
        {
            for (int i = 0; i < m_ProviderSet.Count; ++i)
            {
                m_ProviderSet[i].Destroy();
            }
            m_ProviderSet.Clear();
            m_ProviderDict.Clear();

            for(int i = 0; i < m_BundleLoaderSet.Count; ++i)
            {
                m_BundleLoaderSet[i].Destroy(true);
            }
            m_BundleLoaderSet.Clear();
            m_BundleLoaderDict.Clear();

            ClearSceneHandle();

            decryptionServices = null;
            bundleServices = null;
        }

        /// <summary>
        /// ж�����ü���Ϊ0����Դ
        /// </summary>
        public void UnloadUnusedAssets()
        {
            // ������Դ�ṩ�ߣ����ü���Ϊ0������
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

            // �༭��ģ��ģʽ�������BundleLoader
            if (m_PlayMode != EPlayMode.FromEditor)
            {
                for(int i = m_BundleLoaderSet.Count - 1; i >= 0; --i)
                {
                    var loader = m_BundleLoaderSet[i];
                    if(loader.canDestroy)
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
            for (int i = m_ProviderSet.Count - 1; i >= 0; --i)
            {
                m_ProviderSet[i].Destroy();
            }
            m_ProviderSet.Clear();
            m_ProviderDict.Clear();

            for(int i = 0; i < m_BundleLoaderSet.Count; ++i)
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
                loader = new AssetBundleLoaderEx(this, bundleInfo);

            m_BundleLoaderDict.Add(loader.bundleInfo.descriptor.bundleName, loader);
            m_BundleLoaderSet.AddUnique(loader);
            return loader;
        }

        private BundleLoaderBase TryGetAssetBundleLoader(string bundleName)
        {
            m_BundleLoaderDict.TryGetValue(bundleName, out BundleLoaderBase loader);
            return loader;
        }

        private ProviderBase TryGetProvider(string providerGUID)
        {
            m_ProviderDict.TryGetValue(providerGUID, out ProviderBase provider);
            return provider;
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
                // ���·��ĩβ�Ƿ��пո�
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

        #region ԭ����Դ���ؽӿ�
        public RawFileOperationHandle LoadRawFile(string assetPath)
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, null);
            return LoadRawFileInternal(assetInfo, true);
        }

        public RawFileOperationHandle LoadRawFileAsync(string location)
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadRawFileInternal(assetInfo, false);
        }

        private RawFileOperationHandle LoadRawFileInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
        {
#if UNITY_EDITOR
            if (!assetInfo.isValid)
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
                m_ProviderDict.Add(providerGUID, provider);
                m_ProviderSet.AddUnique(provider);
            }
            return provider.CreateHandle<RawFileOperationHandle>();
        }
        #endregion      // ԭ����Դ���ؽӿ�

        #region ��Դ���ؽӿڣ�ͬ��&�첽��
        /// <summary>
		/// ͬ��������Դ����
		/// </summary>
		/// <typeparam name="TObject">��Դ����</typeparam>
		/// <param name="assetPath">��Դ�Ķ�λ��ַ</param>
		public AssetOperationHandle LoadAsset<TObject>(string assetPath) where TObject : UnityEngine.Object
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, typeof(TObject));
            return LoadAssetInternal(assetInfo, true);
        }

        /// <summary>
        /// ͬ��������Դ����
        /// </summary>
        /// <param name="assetPath">��Դ�Ķ�λ��ַ</param>
        /// <param name="type">��Դ����</param>
        public AssetOperationHandle LoadAsset(string assetPath, System.Type type)
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, type);
            return LoadAssetInternal(assetInfo, true);
        }

        /// <summary>
		/// �첽������Դ����
		/// </summary>
		/// <typeparam name="TObject">��Դ����</typeparam>
		/// <param name="assetPath">��Դ�Ķ�λ��ַ</param>
		public AssetOperationHandle LoadAssetAsync<TObject>(string assetPath) where TObject : UnityEngine.Object
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, typeof(TObject));
            return LoadAssetInternal(assetInfo, false);
        }

        /// <summary>
        /// �첽������Դ����
        /// </summary>
        /// <param name="assetPath">��Դ�Ķ�λ��ַ</param>
        /// <param name="type">��Դ����</param>
        public AssetOperationHandle LoadAssetAsync(string assetPath, System.Type type)
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, type);
            return LoadAssetInternal(assetInfo, false);
        }

        private AssetOperationHandle LoadAssetInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
        {
#if UNITY_EDITOR
            if (!assetInfo.isValid)
            {
                BundleInfo bundleInfo = bundleServices.GetBundleInfo(assetInfo);
                if (bundleInfo.descriptor.isRawFile)
                    throw new System.Exception($"Cannot load raw file using {nameof(LoadAssetAsync)} method !");
            }
#endif

            var handle = LoadAssetAsync(assetInfo);
            if (waitForAsyncComplete)
                handle.WaitForAsyncComplete();
            return handle;
        }

        private AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo)
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
                m_ProviderDict.Add(providerGUID, provider);
                m_ProviderSet.AddUnique(provider);
            }
            return provider.CreateHandle<AssetOperationHandle>();
        }
        #endregion  // ��Դ���ؽӿڣ�ͬ��&�첽��

        #region �������ؽӿ�
        /// <summary>
		/// �첽���س���
		/// </summary>
		/// <param name="location">�����Ķ�λ��ַ</param>
		/// <param name="sceneMode">��������ģʽ</param>
		/// <param name="activateOnLoad">�������ʱ�Ƿ���������</param>
		/// <param name="priority">���ȼ�</param>
		public SceneOperationHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
            var handle = LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
            return handle;
        }

        /// <summary>
        /// �첽���س���
        /// </summary>
        /// <param name="assetInfo">��������Դ��Ϣ</param>
        /// <param name="sceneMode">��������ģʽ</param>
        /// <param name="activateOnLoad">�������ʱ�Ƿ���������</param>
        /// <param name="priority">���ȼ�</param>        
		private SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad, int priority)
        {
            if (!assetInfo.isValid)
            {
                UnityEngine.Debug.LogError($"Failed to load scene ! {assetInfo.lastError}");
                CompletedProvider completedProvider = new CompletedProvider(assetInfo);
                completedProvider.SetCompleted(assetInfo.lastError);
                return completedProvider.CreateHandle<SceneOperationHandle>();
            }

            // ������ص�������������ж�����л���ĳ���
            if (sceneMode == LoadSceneMode.Single)
            {
                UnloadAllScene();
            }

            // ע�⣺ͬһ��������ProviderGUIDÿ�μ��ض���仯����Ϊͬһ������������Additive��ʽ��μ��أ�guid�����Ա�ʾΨһ
            string providerGUID = $"{assetInfo.guid}-{++s_SceneCreateCount}";
            ProviderBase provider;
            {
                if (m_PlayMode == EPlayMode.FromEditor)
                    provider = new DatabaseSceneProvider(this, providerGUID, assetInfo, sceneMode, activateOnLoad, priority);
                else
                    provider = new BundleSceneProvider(this, providerGUID, assetInfo, sceneMode, activateOnLoad, priority);
                m_ProviderDict.Add(providerGUID, provider);
                m_ProviderSet.AddUnique(provider);
            }

            var handle = provider.CreateHandle<SceneOperationHandle>();
            //handle.PackageName = BundleServices.GetPackageName();
            m_SceneHandlesDict.Add(providerGUID, handle);
            return handle;
        }

        internal void UnloadSubScene(ProviderBase provider)
        {
            string providerGUID = provider.providerGUID;
            if (m_SceneHandlesDict.ContainsKey(providerGUID) == false)
                throw new System.Exception("Should never get here !");

            // �ͷ��ӳ������
            m_SceneHandlesDict[providerGUID].Release();
            m_SceneHandlesDict.Remove(providerGUID);

            // ж��δ��ʹ�õ���Դ������������
            UnloadUnusedAssets();
        }

        private void UnloadAllScene()
        {
            // �ͷ����г������
            foreach (var valuePair in m_SceneHandlesDict)
            {
                valuePair.Value.Release();
            }
            m_SceneHandlesDict.Clear();

            // ж��δ��ʹ�õ���Դ������������
            UnloadUnusedAssets();
        }

        internal void ClearSceneHandle()
        {
            // �ͷ���Դ���µ����г���
            m_SceneHandlesDict.Clear();
        }

        #endregion  // �������ؽӿ�               
    }
}