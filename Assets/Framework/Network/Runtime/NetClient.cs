using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Framework.NetWork
{
    public class NetClient : MonoBehaviour
    {
        public delegate void        OnConnected(NetClient client);
        public delegate void        OnDisconnected(NetClient client, int ret);

        private TcpClient           m_Client;

        private IPAddress           m_IP;
        private int                 m_Port;

        private OnConnected         m_ConnectedHandler;
        private OnDisconnected      m_DisconnectedHandler;

        private NetStreamBuffer     m_SendBuffer;                                                   // 消息发送缓存池
        private NetStreamBuffer     m_ReceiveBuffer;                                                // 消息接收缓存池

        private SemaphoreSlim       m_SendBufferSema = new SemaphoreSlim(0, 1);                     // 控制消息发送的信号量

        public NetClient(string host, int port, OnConnected connectionHandler, OnDisconnected disconnectedHandler)
        {
            m_ConnectedHandler = connectionHandler;
            m_DisconnectedHandler = disconnectedHandler;

            Connect(host, port);
        }

        async void Connect(string host, int port)
        {
            m_Client = new TcpClient();
            m_Client.NoDelay = true;

            try
            {
                m_IP = IPAddress.Parse(host);
                m_Port = port;
                await m_Client.ConnectAsync(m_IP, m_Port);

                m_ConnectedHandler?.Invoke(this);
            }
            catch(ArgumentNullException e)
            {
                m_ConnectedHandler?.Invoke(null);
                Debug.LogError($"Client::Connect {e.Message}");
            }
            catch(SocketException e)
            {
                m_ConnectedHandler?.Invoke(null);
                Debug.LogError($"Client::Connect {e.Message}");
            }

            m_SendBuffer = new NetStreamBuffer(m_Client.GetStream(), 4 * 1024);
            m_ReceiveBuffer = new NetStreamBuffer(m_Client.GetStream(), 8 * 1024);

            FlushOutputStream();
            ReceiveAsync();
        }

        public void Close()
        {
            try
            {
                m_Client.GetStream().Close();
                m_Client.Close();

                m_DisconnectedHandler?.Invoke(this, 0);
            }
            catch(Exception e)
            {
                Debug.LogError($"Client::Close {e.Message}");
            }
        }

        private void LateUpdate()
        {
            // 一帧触发一次消息发送
            if (m_SendBufferSema.CurrentCount == 0 && !m_SendBuffer.IsEmpty())             // The number of remaining threads that can enter the semaphore
            {
                m_SendBufferSema.Release();                     // Sema.CurrentCount += 1
            }

            // 解析消息
            ParseMsg();
        }

        private async void FlushOutputStream()
        {
            try
            {
                while (m_Client.Connected)
                {
                    await m_SendBufferSema.WaitAsync();         // 等待Sema.CurrentCount > 0，执行完Sema.CurrentCount -= 1
                    await m_SendBuffer.FlushWrite();
                }
            }
            catch(SocketException e)
            {
                Debug.LogError($"FlushOutputStream  {e.Message}");
                m_DisconnectedHandler?.Invoke(this, e.ErrorCode);         // 异常断开
            }
        }

        public void Send(byte[] buf, int offset, int length)
        {
            if (buf == null || offset + length > buf.Length)
            {
                throw new ArgumentException("Send: offset + length > buf.Length");
            }

            m_SendBuffer.Write(buf, offset, length);
        }

        public void Send(byte[] buf)
        {
            if (buf == null) throw new ArgumentNullException("Send...");
            Send(buf, 0, buf.Length);
        }






        private async void ReceiveAsync()
        {
            try
            {
                while(m_Client.Connected)
                {
                    int count = await m_ReceiveBuffer.ReadAsync();
                    if(count == 0)
                    {
                        m_DisconnectedHandler?.Invoke(this, 2);     // 远端主动断开网络
                    }
                }
            }
            catch(SocketException e)
            {
                Debug.LogError($"ReceiveAsync   {e.Message}");
                m_DisconnectedHandler?.Invoke(this, e.ErrorCode);
            }
        }

        private void ParseMsg()
        {

        }
    }
}