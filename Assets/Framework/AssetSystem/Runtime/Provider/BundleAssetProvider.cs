using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    internal sealed class BundleAssetProvider : BundleProvider
    {
        private AssetBundleRequest m_AssetBundleRequest;

#pragma warning disable CS0628 // 在密封类型中声明了新的保护成员
        protected BundleAssetProvider() { }
#pragma warning restore CS0628 // 在密封类型中声明了新的保护成员
        public BundleAssetProvider(AssetSystem assetSystem, string providerGUID, AssetInfo assetInfo) : base(assetSystem, providerGUID, assetInfo)
        {
        }

        public override void Update()
        {
            if (isDone)
                return;

            // 开始
            if (status == EProviderStatus.None)
                status = EProviderStatus.CheckBundle;

            // 检测资源包的完成状态
            if(status == EProviderStatus.CheckBundle)
            {
                if(requestAsyncComplete)
                {
                    mainBundleLoader.WaitForAsyncComplete();
                    if (dependBundleLoader != null)
                        dependBundleLoader.WaitForAsyncComplete();
                }

                // 等待资源包加载完成
                if (!mainBundleLoader.isDone)
                    return;
                if (dependBundleLoader != null && !dependBundleLoader.IsDone())
                    return;

                if(dependBundleLoader != null && !dependBundleLoader.IsSucceed())
                {
                    status = EProviderStatus.Failed;
                    lastError = dependBundleLoader.GetLastError();
                    InvokeCompletion();
                    return;
                }

                if(mainBundleLoader.status != EBundleLoadStatus.Succeed)
                {
                    status = EProviderStatus.Failed;
                    lastError = mainBundleLoader.lastError;
                    InvokeCompletion();
                    return;
                }

                status = EProviderStatus.Loading;
            }

            // 加载资源
            if(status == EProviderStatus.Loading)
            {
                if(requestAsyncComplete)
                { // 同步
                    if(assetInfo.assetType == null)
                    {
                        assetObject = mainBundleLoader.cachedBundle.LoadAsset(assetInfo.addressableName);
                    }
                    else
                    {
                        assetObject = mainBundleLoader.cachedBundle.LoadAsset(assetInfo.addressableName, assetInfo.assetType);
                    }
                }
                else
                { // 异步
                    if(assetInfo.assetType == null)
                    {
                        m_AssetBundleRequest = mainBundleLoader.cachedBundle.LoadAssetAsync(assetInfo.addressableName);
                    }
                    else
                    {
                        m_AssetBundleRequest = mainBundleLoader.cachedBundle.LoadAssetAsync(assetInfo.addressableName, assetInfo.assetType);
                    }
                }
                status = EProviderStatus.Checking;
            }

            // 检测加载结果
            if(status == EProviderStatus.Checking)
            {
                progress = m_AssetBundleRequest?.progress ?? 1;

                if (m_AssetBundleRequest != null)
                {
                    if (requestAsyncComplete)
                    {
                        // 异步转同步将强制挂起主线程
                        Debug.LogWarning($"Suspend main thread to load unity asset");
                        assetObject = m_AssetBundleRequest.asset;
                    }
                    else
                    {
                        if (!m_AssetBundleRequest.isDone)
                            return;
                        assetObject = m_AssetBundleRequest.asset;
                    }
                }

                status = assetObject != null ? EProviderStatus.Succeed : EProviderStatus.Failed;
                if(status == EProviderStatus.Failed)
                {
                    lastError = $"Failed to load asset: {assetInfo.assetPath} AssetType : {assetInfo.assetType} from AssetBundle: {mainBundleLoader.bundlePath}";
                    Debug.LogError(lastError);
                }
                InvokeCompletion();
            }
        }
    }
}