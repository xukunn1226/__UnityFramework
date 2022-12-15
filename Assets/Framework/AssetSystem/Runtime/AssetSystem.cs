using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class AssetSystem
    {
        private Dictionary<string, BundleLoaderBase> m_BundleLoaderDict = new Dictionary<string, BundleLoaderBase>();

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
            BundleLoaderBase loader = null;
            m_BundleLoaderDict.TryGetValue(bundleName, out loader);
            return loader;
        }


        public void Update()
        {

        }
    }
}