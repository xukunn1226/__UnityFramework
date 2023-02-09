using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace Framework.AssetManagement.Runtime
{
    internal sealed class FileDownloader : DownloaderBase
    {
        private readonly bool       m_BreakResume;
        private UnityWebRequest     m_Request;

        private bool                m_isAbort;
        private ulong               m_FileOriginLength;
        private ulong               m_LatestDownloadBytes;
        private float               m_LatestDownloadRealtime;
        private float               m_TryAgainTimer;

        public FileDownloader(BundleInfo bundleInfo, bool breakResume) : base(bundleInfo)
        {
            m_BreakResume = breakResume;
        }

        public override void Update()
        {
            if (m_Steps == ESteps.None)
                return;
            if (IsDone())
                return;
                        
            // 创建下载器
            if (m_Steps == ESteps.CreateDownload)
            {
                string fileSavePath = m_BundleInfo.descriptor.cachedFilePath;

                // 重置变量
                m_DownloadProgress = 0f;
                m_DownloadedBytes = 0;
                m_isAbort = false;
                m_FileOriginLength = 0;
                m_LatestDownloadBytes = 0;
                m_LatestDownloadRealtime = Time.realtimeSinceStartup;
                m_TryAgainTimer = 0f;

                // 是否开启断点续传下载	
                if (m_BreakResume)
                {
                    long fileLength = -1;
                    if (File.Exists(fileSavePath))
                    {
                        FileInfo fileInfo = new FileInfo(fileSavePath);
                        fileLength = fileInfo.Length;
                        m_FileOriginLength = (ulong)fileLength;
                        m_DownloadedBytes = m_FileOriginLength;
                    }

                    m_RequestURL = GetRequestURL();
                    m_Request = UnityWebRequest.Get(m_RequestURL);
                    var handler = new DownloadHandlerFile(fileSavePath, true);
                    handler.removeFileOnAbort = false;
                    m_Request.downloadHandler = handler;
                    m_Request.disposeDownloadHandlerOnDispose = true;
                    if (fileLength > 0)
                        m_Request.SetRequestHeader("Range", $"bytes={fileLength}-");
                    m_Request.SendWebRequest();
                    m_Steps = ESteps.CheckDownload;
                }
                else
                {
                    m_RequestURL = GetRequestURL();
                    m_Request = new UnityWebRequest(m_RequestURL, UnityWebRequest.kHttpVerbGET);
                    DownloadHandlerFile handler = new DownloadHandlerFile(fileSavePath);
                    handler.removeFileOnAbort = true;
                    m_Request.downloadHandler = handler;
                    m_Request.disposeDownloadHandlerOnDispose = true;
                    m_Request.SendWebRequest();
                    m_Steps = ESteps.CheckDownload;
                }
            }

            // 检测下载结果
            if (m_Steps == ESteps.CheckDownload)
            {
                m_DownloadProgress = m_Request.downloadProgress;
                m_DownloadedBytes = m_FileOriginLength + m_Request.downloadedBytes;
                if (m_Request.isDone == false)
                {
                    CheckTimeout();
                    return;
                }

                bool hasError = false;

                if (m_Request.result != UnityWebRequest.Result.Success)
                {
                    hasError = true;
                    m_LastError = m_Request.error;
                    m_LastCode = m_Request.responseCode;
                }

                // 如果下载失败
                if (hasError)
                {
                    string cacheFilePath = m_BundleInfo.descriptor.cachedFilePath;
                    if (File.Exists(cacheFilePath))
                        File.Delete(cacheFilePath);
                    
                    // 失败后重新尝试
                    if (m_FailedTryAgain > 0)
                    {
                        ReportWarning();
                        m_Steps = ESteps.TryAgain;
                    }
                    else
                    {
                        ReportError();
                        m_Steps = ESteps.Failed;
                    }
                }
                else
                {
                    m_LastError = string.Empty;
                    m_LastCode = 0;
                    m_Steps = ESteps.Succeed;
                }

                // 释放下载器
                DisposeWebRequest();
            }

            // 重新尝试下载
            if (m_Steps == ESteps.TryAgain)
            {
                m_TryAgainTimer += Time.unscaledDeltaTime;
                if (m_TryAgainTimer > 1f)   // 延时一秒，再次下载
                {
                    m_FailedTryAgain--;
                    m_Steps = ESteps.CreateDownload;
                    Debug.LogWarning($"Try again download : {m_RequestURL}");
                }
            }
        }

        public override void Abort()
        {
            if (IsDone() == false)
            {
                m_Steps = ESteps.Failed;
                m_LastError = "user abort";
                m_LastCode = 0;
                DisposeWebRequest();
            }
        }

        private void CheckTimeout()
        {
            // 注意：在连续时间段内无新增下载数据及判定为超时
            if (m_isAbort == false)
            {
                if (m_LatestDownloadBytes != DownloadedBytes)
                {
                    m_LatestDownloadBytes = DownloadedBytes;
                    m_LatestDownloadRealtime = Time.realtimeSinceStartup;
                }

                float offset = Time.realtimeSinceStartup - m_LatestDownloadRealtime;
                if (offset > m_Timeout)
                {
                    Debug.LogWarning($"Web file request timeout : {m_RequestURL}");
                    m_Request.Abort();
                    m_isAbort = true;
                }
            }
        }

        private void DisposeWebRequest()
        {            
            if (m_Request != null)
            {
                m_Request.Dispose();
                m_Request = null;
            }
        }
    }
}