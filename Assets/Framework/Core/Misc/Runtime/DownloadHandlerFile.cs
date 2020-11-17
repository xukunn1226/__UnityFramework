using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

namespace Framework.Core
{
    public class DownloadHandlerFile : DownloadHandlerScript
    {
        private string              m_Path;
        private string              m_TempPath;
        private FileStream          m_Stream;
        private long                m_DownedLength;             // 已下载字节长度
        private long                m_ContentLength;            // 剩余字节长度
        private long                m_TotalLength;              // m_DownedLength + m_ContentLength

        public long downedLength    { get { return m_DownedLength; } }
        public bool isFinished      { get; private set; }
        public bool hasError        { get; private set; }

        protected DownloadHandlerFile() {}

        public DownloadHandlerFile(string path, byte[] preallocatedBuffer) : base(preallocatedBuffer)
        {
            Prepare(path);
        }

        private void Prepare(string path)
        {
            Close();
            isFinished = false;
            hasError = false;

            m_Path = path.Replace("\\", "/");
            string directoryPath = m_Path.Substring(0, m_Path.LastIndexOf("/"));
            if(!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            m_TempPath = m_Path + ".tmp";
            try
            {
                m_Stream = new FileStream(m_TempPath, FileMode.OpenOrCreate);
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                
                Close();
                hasError = true;
            }
            m_DownedLength = m_Stream.Length;
            m_Stream.Position = m_DownedLength;
        }

        private void Close()
        {
            if(m_Stream != null)
            {
                m_Stream.Close();
                m_Stream.Dispose();
                m_Stream = null;
            }
        }

        // Callback, invoked when all data has been received from the remote server.
        protected override void CompleteContent()
        {
            Debug.Log($"CompleteContent: {Time.frameCount}");

            Close();

            if(!File.Exists(m_TempPath))
            {
                Debug.LogError($"Tmp file {m_TempPath} is missing");
                hasError = true;
                return;
            }

            if(File.Exists(m_Path))
            {
                File.Delete(m_Path);
            }
            File.Move(m_TempPath, m_Path);

            isFinished = true;
        }

        // Callback, invoked when UnityWebRequest.downloadProgress is accessed.
        protected override float GetProgress()
        {
            return m_DownedLength * 1.0f / m_TotalLength;
        }

        protected override string GetText()
        {
            throw new System.NotImplementedException();
        }

        // Callback, invoked with a Content-Length header is received.
        protected override void ReceiveContentLengthHeader(ulong contentLength)
        {
            m_ContentLength = (long)contentLength;
            m_TotalLength = m_ContentLength + m_DownedLength;
            
            Debug.Log($"已下载：{m_DownedLength}/{m_TotalLength}        {Time.frameCount}");
        }

        // Callback, invoked as data is received from the remote server.
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            Debug.Log($"ReceiveData: {dataLength}       {Time.frameCount}");

            if(m_ContentLength == 0)
            {
                Close();
                hasError = true;
                return false;
            }

            if(data == null || data.Length < 1)
            {
                Close();
                hasError = true;
                return false;
            }

            try
            {
                m_Stream.Write(data, 0, dataLength);
            }
            catch(System.Exception e)
            {
                Debug.LogError(e.Message);
                Close();
                hasError = true;
                return false;
            }
            m_DownedLength += dataLength;
            return true;
        }
    }
}