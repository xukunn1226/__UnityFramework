using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace Framework.AssetManagement.Runtime
{
    public class AssetSystem
    {
        private Dictionary<string, BundleLoaderBase>    m_BundleLoaderDict  = new Dictionary<string, BundleLoaderBase>();
        private Dictionary<string, ProviderBase>        m_ProviderDict      = new Dictionary<string, ProviderBase>();
        
        private EPlayMode           m_PlayMode;
        public IDecryptionServices  decryptionServices  { get; private set; }
        public IBundleServices      bundleServices      { get; private set; }

        public void Init(EPlayMode playMode, IDecryptionServices decryptionServices, IBundleServices bundleServices)
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
                loader = new AssetBundleLoader(this, bundleInfo);

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

        #region 资源加载接口

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



        #endregion      // 资源加载接口

        public void Update()
        {

        }
    }
}