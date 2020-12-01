using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

namespace Framework.Core
{
    public class DownloadFile : DownloadHandlerScript
    {
        private string m_Path;
        private string m_TempPath;
        private FileStream m_Stream;
        private UnityWebRequest m_Request;
        private float m_LastTime;
        private ulong m_LastDownedLength;

        public float downloadSpeed { get; private set; }           // 下载速度，byte/s
        public ulong downedLength { get; private set; }           // 已下载长度
        public ulong totalLength { get; private set; }           // 总长度
        public bool hasError { get; private set; }
        private string m_InternalError;
        public string handlerError { get { return string.IsNullOrEmpty(error) ? m_InternalError : error; } }
        public string hash { get; private set; }

        protected DownloadFile() { }
        ~DownloadFile()
        {
            Clean();
        }

        public DownloadFile(string path, UnityWebRequest request, byte[] preallocatedBuffer) : base(preallocatedBuffer)
        {
            // create directory
            m_Path = path.Replace("\\", "/");
            string directoryPath = m_Path.Substring(0, m_Path.LastIndexOf("/"));
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            // create FileStream
            m_TempPath = m_Path + ".tmp";
            try
            {
                m_Stream = new FileStream(m_TempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);

                Clean();
            }
            m_Stream.Position = m_Stream.Length;
            downedLength = (ulong)m_Stream.Length;

            // 设定实际下载的文件长度（ReceiveContentLengthHeader）
            m_Request = request;
            m_Request.SetRequestHeader("Range", "bytes=" + downedLength + "-");
        }

        private void Clean()
        {
            if (m_Stream != null)
            {
                m_Stream.Close();
                m_Stream.Dispose();
                m_Stream = null;
            }
        }

        // Callback, invoked when all data has been received from the remote server.
        protected override void CompleteContent()
        {
            m_Stream.Position = 0;
            hash = EasyMD5.Hash(m_Stream);

            Clean();

            if (!File.Exists(m_TempPath))
            {
                m_InternalError = string.Format($"Tmp file {m_TempPath} is missing");
                Debug.LogError(m_InternalError);
                return;
            }

            if (File.Exists(m_Path))
            {
                File.Delete(m_Path);
            }
            File.Move(m_TempPath, m_Path);

            Debug.LogError($"CompleteContent:    hash:{hash}    {downedLength}/{totalLength}   frameCount:{Time.frameCount}");
        }

        /// <summary>
        /// Callback, invoked with a Content-Length header is received.
        /// 如果是续传，则是剩余文件大小；若是本地文件则是总长度
        /// </summary>
        /// <param name="contentLength"></param>
        protected override void ReceiveContentLengthHeader(ulong contentLength)
        {
            string contentLengthHeader = m_Request.GetResponseHeader("Content-Length");
            if (string.IsNullOrEmpty(contentLengthHeader))
            { // 本地文件无法续传，总是重新获取
                downedLength = 0;
            }
            totalLength = downedLength + contentLength;
            m_Stream.Position = (int)downedLength;      // 重新定位stream

            m_LastTime = Time.time;
            m_LastDownedLength = downedLength;

            Debug.LogError($"ReceiveContentLengthHeader：{downedLength}/{totalLength}        frameCount: {Time.frameCount}");
        }

        // Callback, invoked as data is received from the remote server.
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength == 0)
            {
                Clean();
                m_InternalError = "ReceiveData: data == null || dataLength == 0";
                Debug.LogError(m_InternalError);
                return false;
            }

            try
            {
                m_Stream.Write(data, 0, dataLength);
            }
            catch (Exception e)
            {
                Clean();
                m_InternalError = e.Message;
                Debug.LogError(m_InternalError);
                return false;
            }
            downedLength += (ulong)dataLength;

            if (Time.time - m_LastTime > 1.0f)
            {
                downloadSpeed = (downedLength - m_LastDownedLength) / (Time.time - m_LastTime);
                m_LastTime = Time.time;
                m_LastDownedLength = downedLength;
            }

            Debug.LogError($"ReceiveData: dataLength: {dataLength}     downedLength: {downedLength}      frameCount: {Time.frameCount}");

            return true;
        }

        // Callback, invoked when UnityWebRequest.downloadProgress is accessed.
        protected override float GetProgress()
        {
            return downedLength * 1.0f / totalLength;
        }

        protected override string GetText()
        {
            throw new System.NotImplementedException();
        }

        protected override byte[] GetData()
        {
            throw new System.NotImplementedException();
        }
    }
}