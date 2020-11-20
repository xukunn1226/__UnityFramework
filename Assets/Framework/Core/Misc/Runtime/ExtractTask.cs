using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

namespace Framework.Core
{
    public struct ExtractTaskInfo
    {
        public int     userData;
        public string  srcURL;
        public string  dstURL;
    }

    public class ExtractTask : IDisposable
    {
        private byte[]                  m_CachedBuffer;
        private UnityWebRequest         m_Request;
        private DownloadHandlerFile     m_Downloader;
        public ExtractTaskInfo          taskInfo { get; private set; }

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
                return m_Request?.isDone ?? false;
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
            taskInfo = data;

            m_Request = UnityWebRequest.Get(data.srcURL);
            m_Request.disposeDownloadHandlerOnDispose = true;

            // Debug.Log($"------------Begin Running       {isRunning}      {Time.frameCount}");
            m_Downloader = new DownloadHandlerFile(data.dstURL, m_CachedBuffer);
            m_Request.SetRequestHeader("Range", "bytes=" + m_Downloader.downedLength + "-");
            m_Request.downloadHandler = m_Downloader;
            yield return m_Request.SendWebRequest();
            // Debug.Log($"End Running-------------------   {isRunning}     {Time.frameCount}");
        }

        public void Dispose()
        {
            m_Request?.Dispose();
        }
    }
}