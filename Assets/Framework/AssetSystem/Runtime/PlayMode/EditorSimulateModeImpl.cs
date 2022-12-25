using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.AssetManagement.Runtime
{
    public class EditorSimulateModeImpl : IBundleServices
    {
        public InitializationOperation InitializeAsync(bool locationToLower)
        {
            var op = new EditorSimulateModeInitializationOperation(this);
            AsyncOperationSystem.StartOperation(op);
            return op;
        }

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