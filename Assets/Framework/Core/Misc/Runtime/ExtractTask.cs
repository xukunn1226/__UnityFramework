using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Networking;

namespace Framework.Core
{
    public class ExtractTaskInfo
    {
        public Uri                                          srcUri;
        public string                                       dstURL;
        public string                                       verifiedHash;               // 下载文件的hash，用来验证下载文件的正确性；为空表示不需验证
        public int                                          retryCount;                 // 重试次数
        public Action<ExtractTaskInfo, ulong, ulong, float> onProgress;                 // arg1: downedLength; arg2: totalLength; arg3: downloadSpeed
        public Action<ExtractTaskInfo, bool, int>           onCompleted;                // arg1: self; arg2: success or failure; arg3: tryCount
        public Action<ExtractTaskInfo, string>              onRequestError;             // arg1: request error
        public Action<ExtractTaskInfo, string>              onDownloadError;            // arg1: download error
    }

    public class ExtractTask : IDisposable
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

        protected ExtractTask() { }

        public ExtractTask(byte[] preallocatedBuffer)
        {
            m_CachedBuffer = preallocatedBuffer;       
        }

        public IEnumerator Run(ExtractTaskInfo data)
        {
            //Debug.Log($"ExtractTask: Begin Running       {Time.frameCount}");

            isRunning = true;
            {
                m_isVerified = false;
                m_TryCount = 0;

                yield return RunOnceEx(data);

                while (!m_isVerified && m_TryCount < data.retryCount + 1)
                {
                    yield return RunOnceEx(data);
                }
            }
            isRunning = false;

            //Debug.Log($"ExtractTask: End Running       {Time.frameCount}");
        }

        private IEnumerator RunOnce(ExtractTaskInfo data)
        {
            Debug.LogError($"url: {data.srcUri.ToString()}");
            m_Request = UnityWebRequest.Get(data.srcUri);
            m_Request.disposeDownloadHandlerOnDispose = true;

            m_Downloader = new DownloadHandlerFile(data.dstURL, m_Request, m_CachedBuffer);
            m_Request.downloadHandler = m_Downloader;
            m_Request.SendWebRequest();

            while (!m_Request.isDone)
            {
                Debug.LogError($"----{m_Request.result}");
                if(m_Request.result == UnityWebRequest.Result.InProgress)
                {
                    data.onProgress?.Invoke(data, downedLength, totalLength, downloadSpeed);
                }
                yield return null;
            }
            Debug.LogError($"===={m_Request.result}");
            m_isVerified = string.IsNullOrEmpty(data.verifiedHash) ? true : string.Compare(data.verifiedHash, m_Downloader.hash) == 0;
            ++m_TryCount;

            switch (m_Request.result)
            {
                case UnityWebRequest.Result.Success:
                    data.onCompleted?.Invoke(data, m_isVerified, m_TryCount);
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    data.onRequestError?.Invoke(data, m_Request.error);
                    break;
            }
            if (!string.IsNullOrEmpty(m_Downloader.handlerError))
            {
                data.onDownloadError?.Invoke(data, m_Downloader.handlerError);
            }

            //Debug.Log($"[RunOnce]     Hash: {m_Downloader.hash}  name: {data.dstURL}  isRunning: {isRunning}   tryCount: {m_TryCount}     frameCount: {Time.frameCount}");
        }

        private IEnumerator RunOnceEx(ExtractTaskInfo data)
        {
            Debug.LogError($"url: {data.srcUri.ToString()}");
            m_Request = UnityWebRequest.Get(data.srcUri);
            m_Request.disposeDownloadHandlerOnDispose = true;
            m_Request.SendWebRequest();

            while (!m_Request.isDone)
            {
                Debug.LogError($"----{m_Request.result}     {Path.GetFileName(data.dstURL)}");
                if (m_Request.result == UnityWebRequest.Result.InProgress)
                {
                    data.onProgress?.Invoke(data, downedLength, totalLength, downloadSpeed);
                }
                yield return null;
            }
            Debug.LogError($"===={m_Request.result}     {Path.GetFileName(data.dstURL)}");

            // create directory
            string dstPath = data.dstURL.Replace("\\", "/");
            string directoryPath = dstPath.Substring(0, dstPath.LastIndexOf("/"));
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            byte[] buf = m_Request.downloadHandler.data;
            FileStream fs = new FileStream(dstPath, FileMode.Create);
            fs.Write(buf, 0, buf.Length);
            fs.Flush();
            fs.Close();
            fs.Dispose();

            m_isVerified = string.IsNullOrEmpty(data.verifiedHash) ? true : EasyMD5.Verify(buf, data.verifiedHash);
            ++m_TryCount;

            switch (m_Request.result)
            {
                case UnityWebRequest.Result.Success:
                    data.onCompleted?.Invoke(data, m_isVerified, m_TryCount);
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    data.onRequestError?.Invoke(data, m_Request.error);
                    break;
            }
            //Debug.Log($"[RunOnce]     Hash: {m_Downloader.hash}  name: {data.dstURL}  isRunning: {isRunning}   tryCount: {m_TryCount}     frameCount: {Time.frameCount}");
        }

        public void Dispose()
        {
            m_Request?.Dispose();
        }
    }
}