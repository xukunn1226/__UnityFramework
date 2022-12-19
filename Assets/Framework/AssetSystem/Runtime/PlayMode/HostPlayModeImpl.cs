using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class HostPlayModeImpl : IBundleServices
    {
        BundleInfo IBundleServices.GetBundleInfo(AssetInfo assetInfo)
        {
            return null;
        }

        BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
        {
            return null;
        }

        AssetDescriptor IBundleServices.TryGetAssetDesc(string assetPath)
        {
            return null;
        }
    }
}