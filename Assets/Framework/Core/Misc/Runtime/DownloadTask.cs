using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Networking;

namespace Framework.Core
{
    public class DownloadTaskInfo
    {
        public Uri                                              srcUri;
        public string                                           dstURL;
        public string                                           verifiedHash;               // 下载文件的hash，用来验证下载文件的正确性；为空表示不需验证
        public int                                              retryCount;                 // 重试次数
        public Action<DownloadTaskInfo, ulong, ulong, float>    onProgress;                 // arg1: downedLength; arg2: totalLength; arg3: downloadSpeed
        public Action<DownloadTaskInfo, bool, int>              onCompleted;                // arg1: self; arg2: success or failure; arg3: tryCount
        public Action<DownloadTaskInfo, string>                 onRequestError;             // arg1: request error
        public Action<DownloadTaskInfo, string>                 onDownloadError;            // arg1: download error
    }

    public class DownloadTask : IDisposable
    {
        private byte[]                  m_CachedBuffer;
        private UnityWebRequest         m_Request;
        private DownloadHandlerFile     m_Downloader;

        private bool                    m_isVerified;
        private int                     m_TryCount;

        private float                   downloadSpeed       { get { return m_Downloader?.downloadSpeed ?? 0; } }
        private float                   downloadProgress    { get { return m_Request?.downloadProgress ?? 0; } }
        private ulong                   downedLength        { get { return m_Downloader?.downedLength ?? 0; } }
        private ulong                   totalLength         { get { return m_Downloader?.totalLength ?? 0; } }
        public bool                     isRunning           { get; private set; }
        public string                   error               { get; private set; }

        protected DownloadTask() { }

        public DownloadTask(byte[] preallocatedBuffer)
        {
            m_CachedBuffer = preallocatedBuffer;       
        }

        public IEnumerator Run(DownloadTaskInfo data)
        {
            //Debug.Log($"ExtractTask: Begin Running       {Time.frameCount}");

            error = null;
            isRunning = true;
            {
                m_isVerified = false;
                m_TryCount = 0;

                yield return RunOnce(data);

                while (!m_isVerified && m_TryCount < data.retryCount + 1)
                {
                    yield return RunOnce(data);
                }

                if(!m_isVerified)
                {
                    error = "Failed to download file, because hash not match";
                }
            }
            isRunning = false;

            //Debug.Log($"ExtractTask: End Running       {Time.frameCount}");
        }

        private IEnumerator RunOnce(DownloadTaskInfo data)
        {
            m_Request = UnityWebRequest.Get(data.srcUri);
            m_Request.disposeDownloadHandlerOnDispose = true;

            m_Downloader = new DownloadHandlerFile(data.dstURL, m_Request, m_CachedBuffer);
            m_Request.downloadHandler = m_Downloader;
            m_Request.SendWebRequest();

            while (!m_Request.isDone)
            {
                if(m_Request.result == UnityWebRequest.Result.InProgress)
                {
                    data.onProgress?.Invoke(data, downedLength, totalLength, downloadSpeed);
                }
                yield return null;
            }
            m_isVerified = string.IsNullOrEmpty(data.verifiedHash) ? true : string.Compare(data.verifiedHash, m_Downloader.hash, true) == 0;
            ++m_TryCount;

            switch (m_Request.result)
            {
                case UnityWebRequest.Result.Success:
                    data.onCompleted?.Invoke(data, m_isVerified, m_TryCount);
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    error = m_Request.error;
                    data.onRequestError?.Invoke(data, m_Request.error);
                    break;
            }
            if (!string.IsNullOrEmpty(m_Downloader.handlerError))
            {
                error = m_Downloader.handlerError;
                data.onDownloadError?.Invoke(data, m_Downloader.handlerError);
            }

            Dispose();
        }

        public void Dispose()
        {
            m_Downloader?.Flush();
            m_Request?.Dispose();
        }
    }
}