using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Threading;

namespace Framework.NetWork
{
    public class UserToken
    {
        public Socket m_Socket;
        public byte[] m_Buffer = new byte[1024];

        public void ReceiveData(byte[] data)
        { }
    }

    public class Server
    {
        private Socket              m_Socket;           // 负责监听IP和端口

        private Stack<UserToken>    m_Pools;

        private int                 m_MaxClient;

        //private static ManualResetEvent connectDone =
        //new ManualResetEvent(false);

        public void Start(int port, int maxClient = 1)
        {
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            m_Socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
         
            m_Socket.Listen(10);

            m_MaxClient = Math.Max(1, maxClient);
            m_Pools = new Stack<UserToken>(m_MaxClient);
            for(int i = 0; i < m_MaxClient; ++i)
            {
                m_Pools.Push(new UserToken());
            }

            m_Socket.BeginAccept(ConnectCallback, null);
        }

        // 思考：既然是异步，当一个客户端已连接ConnectCallback尚未执行结束时，其他客户端是否能连接上？
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = m_Socket.EndAccept(ar);
                Debug.Log($"client {client.RemoteEndPoint} connected...");

                UserToken token = m_Pools.Pop();
                token.m_Socket = client;

                BeginReceive(token);

                // 继续监听其他客户端
                if(m_Pools.Count != 0)
                    m_Socket.BeginAccept(ConnectCallback, null);
            }
            catch(Exception e)
            {
                Debug.Log(e);
            }
        }

        private void BeginReceive(UserToken token)
        {
            try
            {
                token.m_Socket.BeginReceive(token.m_Buffer, 0, token.m_Buffer.Length, SocketFlags.None, EndReceive, token);
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        private void EndReceive(IAsyncResult ar)
        {
            try
            {
                UserToken token = ar.AsyncState as UserToken;

                int len = token.m_Socket.EndReceive(ar);
                if(len > 0)
                {
                    byte[] data = new byte[len];
                    Buffer.BlockCopy(token.m_Buffer, 0, data, 0, len);

                    token.ReceiveData(data);

                    BeginReceive(token);
                }
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}