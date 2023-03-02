using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
	internal sealed class DatabaseAssetProvider : ProviderBase
	{
		private int m_DelayedFrameCount;

		public DatabaseAssetProvider(AssetSystem assetSystem, string providerGUID, AssetInfo assetInfo) : base(assetSystem, providerGUID, assetInfo)
		{
			m_DelayedFrameCount = Mathf.Max(1, AssetManagerSettings.DelayedFrameNumInEditorSimulateMode);
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

				if(requestAsyncComplete)
                { // 立即结束异步模拟，进入下个流程
					status = EProviderStatus.Loading;
                }
				else
                { // 模拟延迟几帧加载
					if (m_DelayedFrameCount <= 0)
					{
						status = EProviderStatus.Loading;
					}
					else
					{
						--m_DelayedFrameCount;
						return;
					}
                }
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

		protected override void ConditionalResetStatus()
        {
            base.ConditionalResetStatus();
            m_DelayedFrameCount = Mathf.Max(1, AssetManagerSettings.DelayedFrameNumInEditorSimulateMode);
        }
	}
}