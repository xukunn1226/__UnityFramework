using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

namespace Framework.Core
{
    public class ExtractTaskInfo
    {
        public Uri                              srcUri;
        public string                           dstURL;
        public string                           verifiedHash;               // 下载文件的hash，用来验证下载文件的正确性
        public int                              retryCount;                 // 重试次数
        public Action<ExtractTaskInfo, bool>    onCompleted;
    }

    public class ExtractTask : IDisposable
    {
        private byte[]                  m_CachedBuffer;
        private UnityWebRequest         m_Request;
        private DownloadHandlerFile     m_Downloader;

        private bool                    m_isVerified;
        private int                     m_TryCount;

        public float                    downloadProgress    { get { return m_Request?.downloadProgress ?? 0; } }
        public ulong                    downedLength        { get { return m_Downloader?.downedLength ?? 0; } }
        public ulong                    totalLength         { get { return m_Downloader?.totalLength ?? 0; } }
        public bool                     isRunning           { get; private set; }
        public bool                     hasError            { get; private set; }

        protected ExtractTask() { }

        public ExtractTask(byte[] preallocatedBuffer)
        {
            m_CachedBuffer = preallocatedBuffer;       
        }

        public IEnumerator Run(ExtractTaskInfo data)
        {
            Debug.Log($"ExtractTask: Begin Running       {Time.frameCount}");

            m_isVerified = false;
            m_TryCount = 0;
            isRunning = true;

            yield return RunOnce(data);
            
            while(!m_isVerified && m_TryCount < data.retryCount + 1)
            {
                yield return RunOnce(data);
            }

            isRunning = false;
            hasError = m_Downloader.hasError;
            data.onCompleted?.Invoke(data, m_isVerified);

            Debug.Log($"ExtractTask: End Running       {Time.frameCount}");
        }

        private IEnumerator RunOnce(ExtractTaskInfo data)
        {
            //UnityWebRequest request = UnityWebRequest.Head(data.srcURL);
            //yield return request.SendWebRequest();
            //Dictionary<string, string> d = request.GetResponseHeaders();
            //string l = request.GetResponseHeader("Content-Length");
            //var totalLength = long.Parse(l);
            //Debug.Log($"===== {totalLength}");

            //m_Request = UnityWebRequest.Get(data.srcURL);
            m_Request = UnityWebRequest.Get(data.srcUri);
            m_Request.disposeDownloadHandlerOnDispose = true;

            m_Downloader = new DownloadHandlerFile(data.dstURL, m_CachedBuffer);
            m_Request.SetRequestHeader("Range", "bytes=" + m_Downloader.downedLength + "-");
            m_Request.downloadHandler = m_Downloader;
            yield return m_Request.SendWebRequest();
            
            m_isVerified = string.IsNullOrEmpty(data.verifiedHash) ? true : string.Compare(data.verifiedHash, m_Downloader.hash) == 0;
            ++m_TryCount;

            Debug.Log($"Hash: {m_Downloader.hash}  name: {data.dstURL}  isRunning: {isRunning}   tryCount: {m_TryCount}     frameCount: {Time.frameCount}");
        }

        public void Dispose()
        {
            m_Request?.Dispose();
        }
    }
}