using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
	internal class DatabaseRawFileProvider : ProviderBase
	{
		public DatabaseRawFileProvider(AssetSystem assetSystem, string providerGUID, AssetInfo assetInfo) : base(assetSystem, providerGUID, assetInfo)
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

				status = EProviderStatus.Checking;

				// 注意：模拟异步加载效果提前返回（延迟一帧）
				if (!m_RequestAsyncComplete)
					return;
			}

			if (status == EProviderStatus.Checking)
			{
				bundlePath = assetInfo.assetPath;
				status = EProviderStatus.Succeed;
				InvokeCompletion();
			}
#endif
		}
	}
}