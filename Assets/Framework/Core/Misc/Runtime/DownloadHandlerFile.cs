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
        
        public ulong                downedLength    { get; private set; }
        public ulong                totalLength     { get; private set; }
        public bool                 isFinished      { get; private set; }
        public bool                 hasError        { get; private set; }
        public string               hash            { get; private set; }

        protected DownloadHandlerFile() {}

        public DownloadHandlerFile(string path, byte[] preallocatedBuffer) : base(preallocatedBuffer)
        {
            Prepare(path);
        }

        private void Prepare(string path)
        {
            CloseFile();
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
                
                CloseFile();
                hasError = true;
            }
            m_Stream.Position = m_Stream.Length;
            downedLength = (ulong)m_Stream.Length;
        }

        private void CloseFile()
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
            isFinished = true;

            m_Stream.Position = 0;
            hash = EasyMD5.Hash(m_Stream);
            
            CloseFile();

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

            Debug.Log($"CompleteContent:    hash:{hash}    {downedLength}/{totalLength}   frameCount:{Time.frameCount}");
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

        // Callback, invoked with a Content-Length header is received.
        protected override void ReceiveContentLengthHeader(ulong contentLength)
        {
            totalLength = contentLength;
            
            Debug.Log($"ReceiveContentLengthHeader：{downedLength}/{totalLength}        frameCount: {Time.frameCount}");
        }

        // Callback, invoked as data is received from the remote server.
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if(totalLength == 0)
            {
                CloseFile();
                hasError = true;
                return false;
            }

            if(data == null || data.Length < 1)
            {
                CloseFile();
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
                CloseFile();
                hasError = true;
                return false;
            }
            downedLength += (ulong)dataLength;
            
            Debug.Log($"ReceiveData: dataLength: {dataLength}     downedLength: {downedLength}      frameCount: {Time.frameCount}");

            return true;
        }
    }
}