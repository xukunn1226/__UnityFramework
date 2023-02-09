using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    internal abstract class DownloaderBase
    {
        protected enum ESteps
        {
            None,
            CheckLocalFile,
            CreateDownload,
            CheckDownload,
            TryAgain,
            Succeed,
            Failed,
        }

        protected readonly BundleInfo m_BundleInfo;

        protected ESteps    m_Steps         = ESteps.None;
        protected int       m_Timeout;
        protected int       m_FailedTryAgain;
        protected int       m_RequestCount;
        protected string    m_RequestURL;

        protected string    m_LastError;
        protected long      m_LastCode;
        protected float     m_DownloadProgress;
        protected ulong     m_DownloadedBytes;

        /// <summary>
        /// 下载进度（0f~1f）
        /// </summary>
        public float DownloadProgress
        {
            get { return m_DownloadProgress; }
        }

        /// <summary>
        /// 已经下载的总字节数
        /// </summary>
        public ulong DownloadedBytes
        {
            get { return m_DownloadedBytes; }
        }

        public DownloaderBase(BundleInfo bundleInfo)
        {
            m_BundleInfo = bundleInfo;
        }

        public void SendRequest(int failedTryAgain, int timeout)
        {
            if (m_Steps == ESteps.None)
            {
                m_FailedTryAgain = failedTryAgain;
                m_Timeout = timeout;
                //m_Steps = ESteps.CheckLocalFile;
                m_Steps = ESteps.CreateDownload;
            }
        }
        public abstract void Update();
        public abstract void Abort();

        /// <summary>
        /// 获取网络请求地址
        /// </summary>
        protected string GetRequestURL()
        {
            // 轮流返回请求地址
            m_RequestCount++;
            if (m_RequestCount % 2 == 0)
                return m_BundleInfo.RemoteFallbackURL;
            else
                return m_BundleInfo.RemoteMainURL;
        }

        /// <summary>
        /// 获取资源包信息
        /// </summary>
        public BundleInfo GetBundleInfo()
        {
            return m_BundleInfo;
        }

        /// <summary>
        /// 检测下载器是否已经完成（无论成功或失败）
        /// </summary>
        public bool IsDone()
        {
            return m_Steps == ESteps.Succeed || m_Steps == ESteps.Failed;
        }

        /// <summary>
        /// 下载过程是否发生错误
        /// </summary>
        public bool HasError()
        {
            return m_Steps == ESteps.Failed;
        }

        /// <summary>
        /// 按照错误级别打印错误
        /// </summary>
        public void ReportError()
        {
            Debug.LogError(GetLastError());
        }

        /// <summary>
        /// 按照警告级别打印错误
        /// </summary>
        public void ReportWarning()
        {
            Debug.LogWarning(GetLastError());
        }

        /// <summary>
        /// 获取最近发生的错误信息
        /// </summary>
        public string GetLastError()
        {
            return $"Failed to download : {m_RequestURL} Error : {m_LastError} Code : {m_LastCode}";
        }
    }
}