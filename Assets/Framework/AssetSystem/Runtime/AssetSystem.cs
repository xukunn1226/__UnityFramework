using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.SceneManagement;

namespace Framework.AssetManagement.Runtime
{
    public class AssetSystem
    {

        private Dictionary<string, BundleLoaderBase>    m_BundleLoaderDict  = new Dictionary<string, BundleLoaderBase>();
        private Dictionary<string, ProviderBase>        m_ProviderDict      = new Dictionary<string, ProviderBase>();
        private List<string>                            m_PendingDestroy    = new List<string>(10);
        private readonly static Dictionary<string, SceneOperationHandle> _sceneHandles = new Dictionary<string, SceneOperationHandle>(100);
        private static long _sceneCreateCount = 0;

        private EPlayMode           m_PlayMode;
        public IDecryptionServices  decryptionServices  { get; private set; }
        public IBundleServices      bundleServices      { get; private set; }

        public InitializationOperation InitializeAsync(InitializeParameters parameters)
        {
            InitializationOperation initializeOperation = new InitializationOperation();
            if (parameters.PlayMode == EPlayMode.FromEditor)
                parameters.BundleServices = new EditorSimulateModeImpl();
            else
                parameters.BundleServices = new OfflinePlayModeImpl();
            Initialize(parameters.PlayMode, parameters.DecryptionServices, parameters.BundleServices);
            return initializeOperation;
        }

        private void Initialize(EPlayMode playMode, IDecryptionServices decryptionServices, IBundleServices bundleServices)
        {
            m_PlayMode = playMode;
            this.decryptionServices = decryptionServices;
            this.bundleServices = bundleServices;
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
            //DebugCheckInitialize();
            AssetInfo assetInfo = ConvertLocationToAssetInfo(assetPath, null);
            return LoadRawFileInternal(assetInfo, true);
        }

        public RawFileOperationHandle LoadRawFileAsync(string location)
        {
            //DebugCheckInitialize();
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
            //DebugCheckInitialize();
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
            //DebugCheckInitialize();
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
            //DebugCheckInitialize();
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
            //DebugCheckInitialize();
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
            //DebugCheckInitialize();
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

            // ע�⣺ͬһ��������ProviderGUIDÿ�μ��ض���仯
            string providerGUID = $"{assetInfo.guid}-{++_sceneCreateCount}";
            ProviderBase provider;
            {
                if (m_PlayMode == EPlayMode.FromEditor)
                    provider = new DatabaseSceneProvider(this, providerGUID, assetInfo, sceneMode, activateOnLoad, priority);
                else
                    provider = new BundleSceneProvider(this, providerGUID, assetInfo, sceneMode, activateOnLoad, priority);
                m_ProviderDict.Add(providerGUID, provider);
            }

            var handle = provider.CreateHandle<SceneOperationHandle>();
            //handle.PackageName = BundleServices.GetPackageName();
            _sceneHandles.Add(providerGUID, handle);
            return handle;
        }

        private void UnloadSubScene(ProviderBase provider)
        {
            string providerGUID = provider.providerGUID;
            if (_sceneHandles.ContainsKey(providerGUID) == false)
                throw new System.Exception("Should never get here !");

            // �ͷ��ӳ������
            _sceneHandles[providerGUID].Release();
            _sceneHandles.Remove(providerGUID);

            // ж��δ��ʹ�õ���Դ������������
            //UnloadUnusedAssets();
        }

        private void UnloadAllScene()
        {
            // �ͷ����г������
            foreach (var valuePair in _sceneHandles)
            {
                valuePair.Value.Release();
            }
            _sceneHandles.Clear();

            // ж��δ��ʹ�õ���Դ������������
            //UnloadUnusedAssets();
        }

        internal void ClearSceneHandle()
        {
            // �ͷ���Դ���µ����г���
            //if (BundleServices.IsServicesValid())
            //{
            //    string packageName = BundleServices.GetPackageName();
            //    List<string> removeList = new List<string>();
            //    foreach (var valuePair in _sceneHandles)
            //    {
            //        if (valuePair.Value.PackageName == packageName)
            //        {
            //            removeList.Add(valuePair.Key);
            //        }
            //    }
            //    foreach (var key in removeList)
            //    {
            //        _sceneHandles.Remove(key);
            //    }
            //}
        }

        #endregion  // �������ؽӿ�

        public void Update()
        {
            foreach(var loader in m_BundleLoaderDict.Values)
            {
                loader.Update();
            }

            foreach(var provider in m_ProviderDict.Values)
            {
                provider.Update();
            }

            // for debug
            UnloadUnusedAssets();
        }

        public void Destroy()
        {
            foreach(var provider in m_ProviderDict.Values)
            {
                provider.Destroy();
            }
            m_ProviderDict.Clear();

            foreach(var loader in m_BundleLoaderDict.Values)
            {
                loader.Destroy(true);
            }
            m_BundleLoaderDict.Clear();
        }

        /// <summary>
        /// ж�����ü���Ϊ0����Դ
        /// </summary>
        public void UnloadUnusedAssets()
        {
            m_PendingDestroy.Clear();
            foreach (var provider in m_ProviderDict)
            {
                if (provider.Value.canDestroy)
                {
                    provider.Value.Destroy();
                    m_PendingDestroy.Add(provider.Key);
                }
            }
            foreach (var key in m_PendingDestroy)
            {
                m_ProviderDict.Remove(key);
            }

            if (m_PlayMode != EPlayMode.FromEditor)
            { // �༭��ģ��ģʽ�������BundleLoader
                m_PendingDestroy.Clear();
                foreach(var loader in m_BundleLoaderDict)
                {
                    if(loader.Value.canDestroy)
                    {
                        loader.Value.Destroy(false);
                        m_PendingDestroy.Add(loader.Key);
                    }
                }
                foreach(var key in m_PendingDestroy)
                {
                    m_BundleLoaderDict.Remove(key);
                }
            }
        }

        public void ForceUnloadAllAssets()
        {
            foreach(var provider in m_ProviderDict.Values)
            {
                provider.Destroy();
            }
            foreach(var loader in m_BundleLoaderDict.Values)
            {
                loader.Destroy(true);
            }
            m_ProviderDict.Clear();
            m_BundleLoaderDict.Clear();

            Resources.UnloadUnusedAssets();
        }
    }
}