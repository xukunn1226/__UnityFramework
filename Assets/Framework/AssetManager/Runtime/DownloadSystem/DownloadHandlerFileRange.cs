using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// TODO:测试文件不存在的用例
    /// </summary>
    internal class DownloadHandlerFileRange : DownloadHandlerScript
    {
        private string              m_SavePath;
        private string              m_TempSavePath;
        private FileStream          m_Stream;
        private UnityWebRequest     m_Request;
        
        public ulong                downedLength    { get; private set; }           // 已下载长度
        public ulong                totalLength     { get; private set; }           // 总长度
        private string              m_InternalError;
        public string               handlerError    { get { return string.IsNullOrEmpty(error) ? m_InternalError : error; } }

        protected DownloadHandlerFileRange() {}
        ~DownloadHandlerFileRange()
        {
            Cleanup();
        }

        public DownloadHandlerFileRange(string savePath, ulong totalSize, UnityWebRequest request, byte[] preallocatedBuffer) : base(preallocatedBuffer)
        {            
            // create directory
            m_SavePath = savePath.Replace("\\", "/");
            string directoryPath = m_SavePath.Substring(0, m_SavePath.LastIndexOf("/"));
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            // create FileStream
            m_TempSavePath = m_SavePath + ".tmp";
            try
            {
                m_Stream = new FileStream(m_TempSavePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);

                Cleanup();
            }
            m_Stream.Position = m_Stream.Length;
            downedLength = (ulong)m_Stream.Length;
            totalLength = totalSize;

            // 设定实际下载的文件长度（ReceiveContentLengthHeader）
            m_Request = request;
            m_Request.SetRequestHeader("Range", "bytes=" + downedLength + "-");
        }

        public void Cleanup()
        {
            if(m_Stream != null)
            {
                m_Stream.Flush();
                m_Stream.Dispose();
                m_Stream = null;
            }
        }

        // Callback, invoked when all data has been received from the remote server.
        protected override void CompleteContent()
        {
            Cleanup();

            if(!File.Exists(m_TempSavePath))
            {
                m_InternalError = string.Format($"Tmp file {m_TempSavePath} is missing");
                Debug.LogError(m_InternalError);
                return;
            }

            if(File.Exists(m_SavePath))
            {
                File.Delete(m_SavePath);
            }
            File.Move(m_TempSavePath, m_SavePath);
        }

        // Callback, invoked as data is received from the remote server.
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if(data == null || dataLength == 0 || m_Request.responseCode >= 400)
            {
                Cleanup();
                m_InternalError = "ReceiveData: data == null || dataLength == 0";
                Debug.LogError(m_InternalError);
                return false;
            }

            try
            {
                m_Stream.Write(data, 0, dataLength);
            }
            catch(Exception e)
            {
                Cleanup();
                m_InternalError = e.Message;
                Debug.LogError(m_InternalError);
                return false;
            }
            downedLength += (ulong)dataLength;
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