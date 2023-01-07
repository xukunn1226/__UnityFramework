using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.AssetManagement.Runtime
{
	internal sealed class DatabaseSceneProvider : ProviderBase
	{
		public LoadSceneMode    sceneMode       { get; private set; }
        public bool             activateOnLoad  { get; private set; }
        public int              priority        { get; private set; }
		private AsyncOperation	m_AsyncOp;

		public DatabaseSceneProvider(AssetSystem assetSystem, string providerGUID, AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad, int priority) : base(assetSystem, providerGUID, assetInfo)
		{
			this.sceneMode = sceneMode;
			this.activateOnLoad = activateOnLoad;
			this.priority = priority;
		}
		
		public override void Update()
		{
#if UNITY_EDITOR
			if (isDone)
				return;

			if (status == EProviderStatus.None)
			{
				status = EProviderStatus.Loading;
			}

			// 1. 加载资源对象
			if (status == EProviderStatus.Loading)
			{
				LoadSceneParameters loadSceneParameters = new LoadSceneParameters();
				loadSceneParameters.loadSceneMode = sceneMode;
				m_AsyncOp = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(assetInfo.assetPath, loadSceneParameters);
				if (m_AsyncOp != null)
				{
					m_AsyncOp.allowSceneActivation = true;
					m_AsyncOp.priority = priority;
					sceneObject = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
					status = EProviderStatus.Checking;
				}
				else
				{
					status = EProviderStatus.Failed;
					lastError = $"Failed to load scene : {assetInfo.assetPath}";
					Debug.LogError(lastError);
					InvokeCompletion();
				}
			}

			// 2. 检测加载结果
			if (status == EProviderStatus.Checking)
			{
				progress = m_AsyncOp.progress;
				if (m_AsyncOp.isDone)
				{
					if (sceneObject.IsValid() && activateOnLoad)
						SceneManager.SetActiveScene(sceneObject);

					status = sceneObject.IsValid() ? EProviderStatus.Succeed : EProviderStatus.Failed;
					if (status == EProviderStatus.Failed)
					{
						lastError = $"The loaded scene is invalid : {assetInfo.assetPath}";
						Debug.LogError(lastError);
					}
					InvokeCompletion();
				}
			}
#endif
		}
	}
}