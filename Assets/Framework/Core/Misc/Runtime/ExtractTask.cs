using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

namespace Framework.Core
{
    public class ExtractTaskInfo
    {
        public string                           srcURL;
        public string                           dstURL;
        public object                           userData;
        public string                           verifiedHash;               // 下载文件的hash，用来验证下载文件的正确性
        public int                              retryCount;                 // 重试次数
        public Action<ExtractTaskInfo, bool>    onCompleted;
    }

    public class ExtractTask : IDisposable
    {
        private byte[]                          m_CachedBuffer;
        private UnityWebRequest                 m_Request;
        private DownloadHandlerFile             m_Downloader;

        private bool                            m_isVerified;
        private int                             m_TryCount;

        public float downloadProgress
        {
            get
            {
                return m_Request?.downloadProgress ?? 0;
            }
        }

        public bool isRunning
        {
            get
            {

                return !m_Request?.isDone ?? false;
            }
        }

        public bool hasError
        {
            get
            {
                return m_Downloader?.hasError ?? false;
            }
        }

        public ExtractTask(byte[] preallocatedBuffer)
        {
            m_CachedBuffer = preallocatedBuffer;       
        }

        public IEnumerator Run(ExtractTaskInfo data)
        {
            // Debug.Log($"------------Begin Running       {isRunning}      {Time.frameCount}");

            m_isVerified = false;
            m_TryCount = 0;

            yield return RunOnce(data);
            
            while(!m_isVerified && m_TryCount < data.retryCount + 1)
            {
                yield return RunOnce(data);
            }

            if(data.onCompleted != null) data.onCompleted(data, m_isVerified);

            // Debug.Log($"End Running-------------------   {isRunning}     {Time.frameCount}");
        }

        private IEnumerator RunOnce(ExtractTaskInfo data)
        {
            m_Request = UnityWebRequest.Get(data.srcURL);
            m_Request.disposeDownloadHandlerOnDispose = true;

            m_Downloader = new DownloadHandlerFile(data.dstURL, m_CachedBuffer);
            m_Request.SetRequestHeader("Range", "bytes=" + m_Downloader.downedLength + "-");
            m_Request.downloadHandler = m_Downloader;
            yield return m_Request.SendWebRequest();

            m_isVerified = !string.IsNullOrEmpty(data.verifiedHash) && string.Compare(data.verifiedHash, m_Downloader.hash) == 0;
            ++m_TryCount;
        }

        public void Dispose()
        {
            m_Request?.Dispose();
        }
    }
}