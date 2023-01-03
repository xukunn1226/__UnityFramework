using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public interface IBundleServices
    {
        BundleInfo GetBundleInfo(AssetInfo assetInfo);

        BundleInfo[] GetAllDependBundleInfos(AssetInfo assetInfo);

        AssetDescriptor TryGetAssetDesc(string assetPath);
    }
}