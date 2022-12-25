using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class OfflinePlayModeImpl : IBundleServices
    {
        private AssetManifest m_Manifest;
        
        public InitializationOperation InitializeAsync(bool locationToLower)
        {
            var operation = new OfflinePlayModeInitializationOperation(this);
            AsyncOperationSystem.StartOperation(operation);
            return operation;
        }

        internal void SetAppPatchManifest(AssetManifest patchManifest)
        {
            m_Manifest = patchManifest;
        }

        private BundleInfo CreateBundleInfo(BundleDescriptor patchBundle)
        {
            if (patchBundle == null)
                throw new System.Exception("Should never get here !");

            // ²éÑ¯APP×ÊÔ´
            {
                BundleInfo bundleInfo = new BundleInfo(patchBundle, ELoadMethod.LoadFromStreaming);
                return bundleInfo;
            }
        }

        BundleInfo IBundleServices.GetBundleInfo(AssetInfo assetInfo)
        {
            if (!assetInfo.isValid)
                throw new System.Exception($"assetInfo.isValid: {assetInfo.assetPath}");

            BundleDescriptor desc = m_Manifest.GetMainBundleDesc(assetInfo.assetPath);
            return CreateBundleInfo(desc);
        }

        BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
        {
            if (!assetInfo.isValid)
                throw new System.Exception("Should never get here !");

            var depends = m_Manifest.GetAllDependencies(assetInfo.assetPath);
            List<BundleInfo> result = new List<BundleInfo>(depends.Length);
            foreach (var patchBundle in depends)
            {
                BundleInfo bundleInfo = CreateBundleInfo(patchBundle);
                result.Add(bundleInfo);
            }
            return result.ToArray();
        }

        AssetDescriptor IBundleServices.TryGetAssetDesc(string assetPath)
        {
            if (m_Manifest.TryGetAssetDesc(assetPath, out var desc))
                return desc;
            else
                return null;
        }
    }
}