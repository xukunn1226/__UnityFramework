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

			if (status == EProviderStatus.Checking)
			{
				rawFilePath = assetInfo.assetPath;
				status = EProviderStatus.Succeed;
				InvokeCompletion();
			}
#endif
		}
	}
}