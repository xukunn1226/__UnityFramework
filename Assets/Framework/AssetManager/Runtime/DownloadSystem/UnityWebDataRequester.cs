using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// 下载器：基于UnityWebRequest封装的下载器，支持从streaming assets中读取
    /// </summary>
	internal class UnityWebDataRequester
	{
		protected UnityWebRequest               m_Request;
		protected UnityWebRequestAsyncOperation m_OperationHandle;

		/// <summary>
		/// 请求URL地址
		/// </summary>
		public string URL { private set; get; }


		/// <summary>
		/// 发送GET请求
		/// </summary>
		/// <param name="timeout">超时：从请求开始计时</param>
		public void SendRequest(string url, int timeout = 0)
		{
			if (m_Request == null)
			{
				URL = url;
				m_Request = new UnityWebRequest(URL, UnityWebRequest.kHttpVerbGET);
				DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
				m_Request.downloadHandler = handler;
				m_Request.disposeDownloadHandlerOnDispose = true;
				m_Request.timeout = timeout;
				m_OperationHandle = m_Request.SendWebRequest();
			}
		}

		/// <summary>
		/// 获取下载的字节数据
		/// </summary>
		public byte[] GetData()
		{
			if (m_Request != null && IsDone())
				return m_Request.downloadHandler.data;
			else
				return null;
		}

		/// <summary>
		/// 获取下载的文本数据
		/// </summary>
		public string GetText()
		{
			if (m_Request != null && IsDone())
				return m_Request.downloadHandler.text;
			else
				return null;
		}

		/// <summary>
		/// 释放下载器
		/// </summary>
		public void Dispose()
		{
			if (m_Request != null)
			{
				m_Request.Dispose();
				m_Request = null;
				m_OperationHandle = null;
			}
		}

		/// <summary>
		/// 是否完毕（无论成功失败）
		/// </summary>
		public bool IsDone()
		{
			if (m_OperationHandle == null)
				return false;
			return m_OperationHandle.isDone;
		}

		/// <summary>
		/// 下载进度
		/// </summary>
		public float Progress()
		{
			if (m_OperationHandle == null)
				return 0;
			return m_OperationHandle.progress;
		}

		/// <summary>
		/// 下载是否发生错误
		/// </summary>
		public bool HasError()
		{
			return m_Request.result != UnityWebRequest.Result.Success;
		}

		/// <summary>
		/// 获取错误信息
		/// </summary>
		public string GetError()
		{
			if (m_Request != null)
			{
				return $"URL : {URL} Error : {m_Request.error}";
			}
			return string.Empty;
		}
	}
}