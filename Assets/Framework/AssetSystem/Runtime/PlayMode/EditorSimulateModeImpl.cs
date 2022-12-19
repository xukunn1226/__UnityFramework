using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.AssetManagement.Runtime
{
    public class EditorSimulateModeImpl : IBundleServices
    {
        BundleInfo IBundleServices.GetBundleInfo(AssetInfo assetInfo)
        {
            throw new NotImplementedException();
        }

        BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
        {
            throw new NotImplementedException();
        }

        AssetDescriptor IBundleServices.TryGetAssetDesc(string assetPath)
        {
            AssetDescriptor desc = new AssetDescriptor() { assetPath = assetPath };

            return desc;
        }
    }
}