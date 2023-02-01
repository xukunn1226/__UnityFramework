using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
	internal sealed class BundleSubAssetsProvider : BundleProvider
	{
		private AssetBundleRequest m_AssetBundleRequest;

		public BundleSubAssetsProvider(AssetSystem assetSystem, string providerGUID, AssetInfo assetInfo) : base(assetSystem, providerGUID, assetInfo)
		{
		}
		public override void Update()
		{
			DebugLoadingTime();

            isTriggerLoadingRequest = false;
            if (isDone)
				return;

			if (status == EProviderStatus.None)
			{
				status = EProviderStatus.CheckBundle;
			}

			// 1. 检测资源包
			if (status == EProviderStatus.CheckBundle)
			{
				if (requestAsyncComplete)
				{
					if(dependBundleLoader != null)
						dependBundleLoader.WaitForAsyncComplete();
					mainBundleLoader.WaitForAsyncComplete();
				}

				if (dependBundleLoader != null && !dependBundleLoader.IsDone())
					return;
				if (!mainBundleLoader.isDone)
					return;

				if (dependBundleLoader != null && !dependBundleLoader.IsSucceed())
				{
					status = EProviderStatus.Failed;
					lastError = dependBundleLoader.GetLastError();
					InvokeCompletion();
					return;
				}

				if (mainBundleLoader.status != EBundleLoadStatus.Succeed)
				{
					status = EProviderStatus.Failed;
					lastError = mainBundleLoader.lastError;
					InvokeCompletion();
					return;
				}

				status = EProviderStatus.Loading;
			}

			// 2. 加载资源对象
			if (status == EProviderStatus.Loading)
			{
				if (requestAsyncComplete)
				{
					if (assetInfo.assetType == null)
						allAssetObjects = mainBundleLoader.cachedBundle.LoadAssetWithSubAssets(assetInfo.addressableName);
					else
						allAssetObjects = mainBundleLoader.cachedBundle.LoadAssetWithSubAssets(assetInfo.addressableName, assetInfo.assetType);
				}
				else
				{
					if (assetInfo.assetType == null)
						m_AssetBundleRequest = mainBundleLoader.cachedBundle.LoadAssetWithSubAssetsAsync(assetInfo.addressableName);
					else
						m_AssetBundleRequest = mainBundleLoader.cachedBundle.LoadAssetWithSubAssetsAsync(assetInfo.addressableName, assetInfo.assetType);
				}
				status = EProviderStatus.Checking;
                isTriggerLoadingRequest = true;
            }

			// 3. 检测加载结果
			if (status == EProviderStatus.Checking)
			{
				progress = m_AssetBundleRequest?.progress ?? 1;
				if (m_AssetBundleRequest != null)
				{
					if (requestAsyncComplete)
					{
						// 强制挂起主线程（注意：该操作会很耗时）
						Debug.LogWarning("Suspend the main thread to load unity asset.");
						allAssetObjects = m_AssetBundleRequest.allAssets;
					}
					else
					{
						if (!m_AssetBundleRequest.isDone)
							return;
						allAssetObjects = m_AssetBundleRequest.allAssets;
					}
				}

				status = allAssetObjects == null ? EProviderStatus.Failed : EProviderStatus.Succeed;
				if (status == EProviderStatus.Failed)
				{
					if (assetInfo.assetType == null)
						lastError = $"Failed to load sub assets : {assetInfo.assetPath} AssetType : null AssetBundle : {mainBundleLoader.bundleInfo.descriptor.bundleName}";
					else
						lastError = $"Failed to load sub assets : {assetInfo.assetPath} AssetType : {assetInfo.assetType} AssetBundle : {mainBundleLoader.bundleInfo.descriptor.bundleName}";
					Debug.LogError(lastError);
				}
				InvokeCompletion();
			}
		}
	}
}