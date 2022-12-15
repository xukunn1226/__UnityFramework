using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    internal class BundleProvider : ProviderBase
    {
        protected BundleLoaderBase          mainBundleLoader    { get; private set; }
        protected DependAssetBundleLoader   dependBundleLoader  { get; private set; }

        protected BundleProvider() { }
        public BundleProvider(AssetSystem assetSystem, string providerGUID, AssetInfo assetInfo) : base(assetSystem, providerGUID, assetInfo)
        {
            mainBundleLoader = assetSystem.CreateMainAssetBundleLoader(assetInfo);
            mainBundleLoader.IncreaseRefs();

            var dependBundles = assetSystem.CreateDependAssetBundleLoaders(assetInfo);
            if(dependBundles.Count > 0)
            {
                dependBundleLoader = new DependAssetBundleLoader(dependBundles);
                dependBundleLoader.IncreaseRefs();
            }
        }

        public override void Destroy()
        {
            base.Destroy();

            if(mainBundleLoader != null)
            {
                mainBundleLoader.DecreaseRefs();
                mainBundleLoader = null;
            }
            if(dependBundleLoader != null)
            {
                dependBundleLoader.DecreaseRefs();
                dependBundleLoader = null;
            }
        }

        public override void Update()
        { }
    }
}