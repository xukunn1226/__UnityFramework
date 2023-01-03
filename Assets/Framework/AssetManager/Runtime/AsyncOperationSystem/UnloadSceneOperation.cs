using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.AssetManagement.Runtime
{
	/// <summary>
	/// 场景卸载异步操作类
	/// </summary>
	public sealed class UnloadSceneOperation : AsyncOperationBase
	{
		private enum EFlag
		{
			Normal,
			Error,
		}
		private enum ESteps
		{
			None,
			UnLoad,
			Checking,
			Done,
		}

		private readonly EFlag	m_Flag;
		private ESteps			m_Steps			= ESteps.None;
		private Scene			m_SceneObject;
		private AsyncOperation	m_AsyncOp;

		internal UnloadSceneOperation(string error)
		{
			m_Flag = EFlag.Error;
			lastError = error;
		}

		internal UnloadSceneOperation(Scene scene)
		{
			m_Flag = EFlag.Normal;
			m_SceneObject = scene;
		}

		internal override void Start()
		{
			if (m_Flag == EFlag.Normal)
			{
				m_Steps = ESteps.UnLoad;
			}
			else if (m_Flag == EFlag.Error)
			{
				m_Steps = ESteps.Done;
				status = EOperationStatus.Failed;
			}
			else
			{
				throw new System.NotImplementedException(m_Flag.ToString());
			}
		}
		
		internal override void Update()
		{
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			if (m_Steps == ESteps.UnLoad)
			{
				if (m_SceneObject.IsValid() && m_SceneObject.isLoaded)
				{
					m_AsyncOp = SceneManager.UnloadSceneAsync(m_SceneObject);
					m_Steps = ESteps.Checking;
				}
				else
				{
					lastError = "Scene is invalid or is not loaded.";
					m_Steps = ESteps.Done;
					status = EOperationStatus.Failed;
				}
			}

			if (m_Steps == ESteps.Checking)
			{
				progress = m_AsyncOp.progress;
				if (m_AsyncOp.isDone == false)
					return;

				m_Steps = ESteps.Done;
				status = EOperationStatus.Succeed;
			}
		}
	}
}