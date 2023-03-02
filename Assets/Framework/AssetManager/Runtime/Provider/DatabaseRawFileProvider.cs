using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
	internal class DatabaseRawFileProvider : ProviderBase
	{
		private int m_DelayedFrameCount;

		public DatabaseRawFileProvider(AssetSystem assetSystem, string providerGUID, AssetInfo assetInfo) : base(assetSystem, providerGUID, assetInfo)
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

				if (requestAsyncComplete)
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

			if (status == EProviderStatus.Checking)
			{
				rawFilePath = assetInfo.assetPath;
				status = EProviderStatus.Succeed;
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