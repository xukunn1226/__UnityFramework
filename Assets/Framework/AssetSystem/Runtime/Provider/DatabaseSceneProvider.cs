using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.AssetManagement.Runtime
{
	internal sealed class DatabaseSceneProvider : ProviderBase
	{
		public readonly LoadSceneMode SceneMode;
		private readonly bool _activateOnLoad;
		private readonly int _priority;
		private AsyncOperation _asyncOp;

		public DatabaseSceneProvider(AssetSystem assetSystem, string providerGUID, AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad, int priority) : base(assetSystem, providerGUID, assetInfo)
		{
			SceneMode = sceneMode;
			_activateOnLoad = activateOnLoad;
			_priority = priority;
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
				loadSceneParameters.loadSceneMode = SceneMode;
				_asyncOp = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(assetInfo.assetPath, loadSceneParameters);
				if (_asyncOp != null)
				{
					_asyncOp.allowSceneActivation = true;
					_asyncOp.priority = _priority;
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
				progress = _asyncOp.progress;
				if (_asyncOp.isDone)
				{
					if (sceneObject.IsValid() && _activateOnLoad)
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