using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
	internal sealed class DatabaseSubAssetsProvider : ProviderBase
	{
		private int m_DelayedFrameCount;

		public DatabaseSubAssetsProvider(AssetSystem assetSystem, string providerGUID, AssetInfo assetInfo) : base(assetSystem, providerGUID, assetInfo)
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
								
				if (requestAsyncComplete)
				{ // ���������첽ģ�⣬�����¸�����
					status = EProviderStatus.Loading;
				}
				else
				{ // ģ���ӳټ�֡����
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

			// 1. ������Դ����
			if (status == EProviderStatus.Loading)
			{
				if (assetInfo.assetType == null)
				{
					allAssetObjects = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(assetInfo.assetPath);
				}
				else
				{
					UnityEngine.Object[] findAssets = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(assetInfo.assetPath);
					List<UnityEngine.Object> result = new List<Object>(findAssets.Length);
					foreach (var findAsset in findAssets)
					{
						if (assetInfo.assetType.IsAssignableFrom(findAsset.GetType()))
							result.Add(findAsset);
					}
					allAssetObjects = result.ToArray();
				}
				status = EProviderStatus.Checking;
			}

			// 2. �����ؽ��
			if (status == EProviderStatus.Checking)
			{
				status = allAssetObjects == null ? EProviderStatus.Failed : EProviderStatus.Succeed;
				if (status == EProviderStatus.Failed)
				{
					if (assetInfo.assetType == null)
						lastError = $"Failed to load sub assets : {assetInfo.assetPath} AssetType : null";
					else
						lastError = $"Failed to load sub assets : {assetInfo.assetPath} AssetType : {assetInfo.assetType}";
					Debug.LogError(lastError);
				}
				InvokeCompletion();
			}
#endif
		}
	}
}