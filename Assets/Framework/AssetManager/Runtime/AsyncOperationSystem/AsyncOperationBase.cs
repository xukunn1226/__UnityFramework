using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Framework.AssetManagement.Runtime
{
	public abstract class AsyncOperationBase : IEnumerator
	{		
		public string			lastError	{ get; protected set; }
		public float			progress	{ get; protected set; }
		public EOperationStatus status		{ get; protected set; } = EOperationStatus.None;
		public bool				isDone		{ get { return status == EOperationStatus.Failed || status == EOperationStatus.Succeed; } }

		private Action<AsyncOperationBase> m_Callback; 
		public event Action<AsyncOperationBase> Completed
		{
			add
			{
				if (isDone)
					value.Invoke(this);
				else
					m_Callback += value;
			}
			remove
			{
				m_Callback -= value;
			}
		}

		/// <summary>
		/// 异步操作任务
		/// </summary>
		public Task Task
		{
			get
			{
				if (m_TaskCompletionSource == null)
				{
					m_TaskCompletionSource = new TaskCompletionSource<object>();
					if (isDone)
						m_TaskCompletionSource.SetResult(null);
				}
				return m_TaskCompletionSource.Task;
			}
		}

		internal abstract void Start();
		internal abstract void Update();
		internal void Finish()
		{
			progress = 1f;
			m_Callback?.Invoke(this);
			if (m_TaskCompletionSource != null)
				m_TaskCompletionSource.TrySetResult(null);
		}

		protected void ClearCompletedCallback()
		{
			m_Callback = null;
		}

		#region 异步编程相关
		bool IEnumerator.MoveNext()
		{
			return !isDone;
		}
		void IEnumerator.Reset()
		{
		}
		object IEnumerator.Current => null;

		private TaskCompletionSource<object> m_TaskCompletionSource;
		#endregion
	}
}