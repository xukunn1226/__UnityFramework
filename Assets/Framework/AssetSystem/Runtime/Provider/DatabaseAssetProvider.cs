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
				// �����Դ�ļ��Ƿ����
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

				// ע�⣺ģ���첽����Ч����ǰ���أ��ӳ�һ֡��
				if (!m_RequestAsyncComplete)
					return;
			}

			// 1. ������Դ����
			if (status == EProviderStatus.Loading)
			{
				if (assetInfo.assetType == null)
					assetObject = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetInfo.assetPath);
				else
					assetObject = UnityEditor.AssetDatabase.LoadAssetAtPath(assetInfo.assetPath, assetInfo.assetType);
				status = EProviderStatus.Checking;
			}

			// 2. �����ؽ��
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