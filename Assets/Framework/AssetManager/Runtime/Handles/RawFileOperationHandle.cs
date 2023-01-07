using System;
using System.IO;
using System.Text;

namespace Framework.AssetManagement.Runtime
{
    public class RawFileOperationHandle : OperationHandleBase
    {
		private System.Action<RawFileOperationHandle> m_Callback;

		internal RawFileOperationHandle(ProviderBase provider) : base(provider)
		{
		}

		public void Release()
		{
			this.ReleaseInternal();
		}

		internal override void InvokeCallback()
		{
			m_Callback?.Invoke(this);
		}

		public event System.Action<RawFileOperationHandle> Completed
		{
			add
			{
				if (!isValid)
					throw new System.Exception($"{nameof(RawFileOperationHandle)} is invalid");
				if (provider.isDone)
					value.Invoke(this);
				else
					m_Callback += value;
			}
			remove
			{
				if (!isValid)
					throw new System.Exception($"{nameof(RawFileOperationHandle)} is invalid");
				m_Callback -= value;
			}
		}

		public void WaitForAsyncComplete()
		{
			if (!isValid)
				return;
			provider.WaitForAsyncComplete();
		}

		/// <summary>
		/// 获取原生文件的二进制数据
		/// </summary>
		public byte[] GetRawFileData()
		{
			if (!isValid)
				return null;
			string filePath = provider.rawFilePath;
			if (File.Exists(filePath) == false)
				return null;
			return File.ReadAllBytes(filePath);
		}

		/// <summary>
		/// 获取原生文件的文本数据
		/// </summary>
		public string GetRawFileText()
		{
			if (!isValid)
				return null;
			string filePath = provider.rawFilePath;
			if (File.Exists(filePath) == false)
				return null;
			return File.ReadAllText(filePath, Encoding.UTF8);
		}

		/// <summary>
		/// 获取原生文件的路径
		/// </summary>
		public string GetRawFilePath()
		{
			if (!isValid)
				return string.Empty;
			return provider.rawFilePath;
		}
	}
}