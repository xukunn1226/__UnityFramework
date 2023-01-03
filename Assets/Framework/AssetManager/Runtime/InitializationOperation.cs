using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class InitializationOperation : AsyncOperationBase
    {
        internal override void Start()
        { }

        internal override void Update()
        {
            status = EOperationStatus.Succeed;
        }
    }

    /// <summary>
    /// 编辑器模拟下的初始化流程
    /// </summary>
    internal sealed class EditorSimulateModeInitializationOperation : InitializationOperation
    {
        internal EditorSimulateModeInitializationOperation(EditorSimulateModeImpl impl)
        { }

        internal override void Start()
        { }

        internal override void Update()
        {
            status = EOperationStatus.Succeed;
        }
    }

    internal sealed class OfflinePlayModeInitializationOperation : InitializationOperation
    {
        private enum ESteps
        {
            None,
            LoadAppManifest,
			CheckAppManifest,
            Done,
        }

        private OfflinePlayModeImpl m_Impl;
        private ESteps				m_Steps;
		private AppManifestLoader	m_AppManifestLoader;

		internal OfflinePlayModeInitializationOperation(OfflinePlayModeImpl impl)
        {
            m_Impl = impl;
        }

        internal override void Start()
        {
			m_Steps = ESteps.LoadAppManifest;
		}

        internal override void Update()
        {
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			if(m_Steps == ESteps.LoadAppManifest)
            {
				m_AppManifestLoader = new AppManifestLoader();
				m_Steps = ESteps.CheckAppManifest;
            }

			if (m_Steps == ESteps.CheckAppManifest)
            {
				m_AppManifestLoader.Update();
				progress = m_AppManifestLoader.progress;
				if (!m_AppManifestLoader.isDone)
					return;

				var manifest = m_AppManifestLoader.manifest;
				if (manifest == null)
				{
					m_Steps = ESteps.Done;
					status = EOperationStatus.Failed;
					lastError = m_AppManifestLoader.error;
				}
				else
				{					
					m_Steps = ESteps.Done;
					status = EOperationStatus.Succeed;
					m_Impl.SetAppAssetManifest(manifest);
				}
			}
        }
    }

	internal class AppManifestLoader
	{
		private enum ESteps
		{
			LoadAppManifest,
			CheckAppManifest,
			Done,
		}

		private UnityWebDataRequester	m_Downloader;
		private ESteps					m_Steps = ESteps.LoadAppManifest;

		public AssetManifest	manifest	{ get; private set; }
		public string			error		{ get; private set; }
		public bool				isDone		{ get { return m_Steps == ESteps.Done; } }
		public float			progress	{ get { if (m_Downloader == null) return 0; return m_Downloader.Progress(); } }

		public AppManifestLoader()
		{
		}

		/// <summary>
		/// 更新流程
		/// </summary>
		public void Update()
		{
			if (isDone)
				return;

			if (m_Steps == ESteps.LoadAppManifest)
			{
				//string fileName = YooAssetSettingsData.GetPatchManifestBinaryFileName(_buildinPackageName, _buildinPackageVersion);
				string filePath = PathHelper.MakeStreamingLoadPath("AssetManifest.bytes");
				string url = PathHelper.ConvertToWWWPath(filePath);
				m_Downloader = new UnityWebDataRequester();
				m_Downloader.SendRequest(url);
				m_Steps = ESteps.CheckAppManifest;
			}

			if (m_Steps == ESteps.CheckAppManifest)
			{
				if (m_Downloader.IsDone() == false)
					return;

				if (m_Downloader.HasError())
				{
					error = m_Downloader.GetError();
				}
				else
				{
					// 解析APP里的补丁清单
					try
					{
						byte[] bytesData = m_Downloader.GetData();
						manifest = AssetManifest.DeserializeFromBinary(bytesData);
					}
					catch (System.Exception e)
					{
						error = e.Message;
					}
				}
				m_Steps = ESteps.Done;
				m_Downloader.Dispose();
			}
		}
	}
}