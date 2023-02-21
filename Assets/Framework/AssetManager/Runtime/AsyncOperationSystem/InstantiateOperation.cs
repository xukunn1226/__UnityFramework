using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public sealed class InstantiateOperation : AsyncOperationBase
    {
        private enum ESteps
		{
			None,
			Clone,
			Done,
		}
        private ESteps  m_Steps = ESteps.None;

		private readonly AssetOperationHandle   m_Handle;
		private readonly Vector3                m_Position;
		private readonly Quaternion             m_Rotation;
		private readonly Transform              m_Parent;
		
		public GameObject Result = null;

		internal InstantiateOperation(AssetOperationHandle handle, Vector3 position, Quaternion rotation, Transform parent)
		{
			m_Handle = handle;
			m_Position = position;
			m_Rotation = rotation;
			m_Parent = parent;
		}

		internal override void Start()
		{
			m_Steps = ESteps.Clone;
		}

		internal override void Update()
		{
			if (m_Steps == ESteps.None || m_Steps == ESteps.Done)
				return;

			if (m_Steps == ESteps.Clone)
			{
				if (m_Handle.isValid == false)
				{
					m_Steps = ESteps.Done;
					status = EOperationStatus.Failed;
					lastError = $"{nameof(AssetOperationHandle)} is invalid.";
					return;
				}

				if (m_Handle.isDone == false)
					return;

				if (m_Handle.assetObject == null)
				{
					m_Steps = ESteps.Done;
					status = EOperationStatus.Failed;
					lastError = $"{nameof(AssetOperationHandle.assetInfo.assetPath)} is null.";
					return;
				}

				// 实例化游戏对象
				Result = Object.Instantiate(m_Handle.assetObject as GameObject, m_Position, m_Rotation, m_Parent);

				m_Steps = ESteps.Done;
				status = EOperationStatus.Succeed;
			}
		}

		/// <summary>
		/// 取消实例化对象操作
		/// </summary>
		public void Cancel()
		{
			if (isDone == false)
			{
				m_Steps = ESteps.Done;
				status = EOperationStatus.Failed;
				lastError = $"User cancelled !";
			}
		}

		/// <summary>
		/// 等待异步实例化结束
		/// </summary>
		public void WaitForAsyncComplete()
		{
			if (m_Steps == ESteps.Done)
				return;
			m_Handle.WaitForAsyncComplete();
			Update();
		}
    }
}