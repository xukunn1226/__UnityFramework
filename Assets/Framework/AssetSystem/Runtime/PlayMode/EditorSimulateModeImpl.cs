using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class EditorSimulateModeImpl : IBundleServices
    {
        public BundleInfo GetBundleInfo(AssetInfo assetInfo)
        {
            return null;
        }

        public BundleInfo[] GetAllDependBundleInfos(AssetInfo assetInfo)
        {
            return null;
        }

        public AssetDescriptor TryGetAssetDesc(string assetPath)
        {
            return null;
        }
    }
}