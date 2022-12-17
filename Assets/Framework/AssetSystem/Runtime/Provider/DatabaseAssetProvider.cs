using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
	internal sealed class DatabaseAssetProvider : ProviderBase
	{
		public DatabaseAssetProvider(AssetSystem assetSystem, string providerGUID, AssetInfo assetInfo) : base(assetSystem, providerGUID, assetInfo)
		{
		}

		public override void Update()
		{
#if UNITY_EDITOR
			if (isDone)
				return;

			if (status == EProviderStatus.None)
			{
				// 检测资源文件是否存在
				string guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetInfo.assetPath);
				if (string.IsNullOrEmpty(guid))
				{
					status = EProviderStatus.Failed;
					lastError = $"Not found asset : {assetInfo.assetPath}";
					Debug.LogError(lastError);
					InvokeCompletion();
					return;
				}

				status = EProviderStatus.Loading;

				// 注意：模拟异步加载效果提前返回（延迟一帧）
				if (!m_RequestAsyncComplete)
					return;
			}

			// 1. 加载资源对象
			if (status == EProviderStatus.Loading)
			{
				if (assetInfo.assetType == null)
					assetObject = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetInfo.assetPath);
				else
					assetObject = UnityEditor.AssetDatabase.LoadAssetAtPath(assetInfo.assetPath, assetInfo.assetType);
				status = EProviderStatus.Checking;
			}

			// 2. 检测加载结果
			if (status == EProviderStatus.Checking)
			{
				status = assetObject == null ? EProviderStatus.Failed : EProviderStatus.Succeed;
				if (status == EProviderStatus.Failed)
				{
					if (assetInfo.assetType == null)
						lastError = $"Failed to load asset object : {assetInfo.assetPath} AssetType : null";
					else
						lastError = $"Failed to load asset object : {assetInfo.assetPath} AssetType : {assetInfo.assetType}";
					Debug.LogError(lastError);
				}
				InvokeCompletion();
			}
#endif
		}
	}
}