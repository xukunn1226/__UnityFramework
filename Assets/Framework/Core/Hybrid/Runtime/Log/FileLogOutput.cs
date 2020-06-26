using System.IO;
using System;
using System.Text;
using UnityEngine;

namespace Framework.Core
{
    [RequireComponent(typeof(GameDebug))]
    public class FileLogOutput : BaseLogOutput
    {
        private bool            m_Disposed = false;

        private FileStream      m_FileStream;
        private StreamWriter    m_Writer;

        private int             m_BufSize = 1024;
        private int             m_BufWrittenBytes;

        void Awake()
        {
            m_FileStream = new FileStream(Application.persistentDataPath + "/" + Application.productName + "_log.txt", FileMode.Create, FileAccess.ReadWrite);
            m_Writer = new StreamWriter(m_FileStream);
            m_BufWrittenBytes = 0;
            Debug.Log($"File log output: {Application.persistentDataPath + "/" + Application.productName + "_log.txt"}");
        }

        public override void Output(string logString, string stackTrace, LogType type)
        {
            if(type == LogType.Error || type == LogType.Warning || type == LogType.Exception)
            {
                stackTrace = stackTrace.TrimEnd(new char[] {'\n'});
                logString = string.Format("{0}\r\n  {1}", logString, stackTrace.Replace("\n", "\r\n     "));
            }
            logString = string.Format("[{0}] {1}{2}", type.ToString(), logString, "\n");
            Receive(logString);
        }

        public override void Flush()
        {
            m_BufWrittenBytes = 0;
            if (m_Writer != null)
                m_Writer.Flush();
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool shouldDisposeManagedReources)
        {
            if (m_Disposed)
                return;

            if (shouldDisposeManagedReources)
            {
                // TODO:释放那些实现IDisposable接口的托管对象
                m_Writer.Close();
                m_Writer.Dispose();

                m_FileStream.Close();
                m_FileStream.Dispose();
            }

            //TODO:释放非托管资源，设置对象为null

            m_Disposed = true;
        }

        private void Receive(string content)
        {
            byte[] bytes = Encoding.Default.GetBytes(content);
            
            int count = bytes.Length;
            int pos = 0;
            while(m_BufWrittenBytes + count > m_BufSize)
            {
                int c = m_BufSize - m_BufWrittenBytes;
                m_Writer.Write(Encoding.Default.GetString(bytes, pos, c));
                m_Writer.Flush();

                m_BufWrittenBytes = 0;
                pos += c;
                count -= c;
            }
            if(count != 0)
            {
                m_Writer.Write(Encoding.Default.GetString(bytes, pos, count));
                m_BufWrittenBytes += count;
            }
        }
    }
}