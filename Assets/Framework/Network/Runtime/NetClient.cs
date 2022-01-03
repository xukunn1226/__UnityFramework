using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace Framework.NetWork
{
    public enum ConnectState
    {
        Disconnected,
        Connecting,
        Connected,
    }

    public interface INetListener<TMessage> where TMessage : class
    {
        void OnPeerConnectSuccess();                            // 连接成功回调
        void OnPeerConnectFailed(Exception e);                  // 连接失败回调
        void OnPeerClose();                                     // 主动断开连接
        void OnPeerDisconnected(Exception e);                   // 异常断开连接
        void OnNetworkReceive(List<TMessage> msgs);             // 网络包回调
        int sendBufferSize          { get; }                    // 发送消息包缓存区大小
        int receiveBufferSize       { get; }                    // 接收消息包缓存区大小
        IPacket<TMessage> parser    { get; }                    // 
    }

    public interface IConnector
    {
        ConnectState state { get; }
        Task Connect(string host, int port);
        void Shutdown();
        void RaiseException(Exception e);
    }

    public class NetClient<TMessage> : IConnector where TMessage : class
    {
        public ConnectState             state { get; private set; } = ConnectState.Disconnected;

        private TcpClient               m_Client;
        
        private string                  m_Host;
        private int                     m_Port;

        private NetStreamWriter         m_StreamWriter;
        private NetStreamReader         m_StreamReader;

        private bool                    m_HandleException;
        private Exception               m_Exception;
        private IPacket<TMessage>       m_Parser;
        private List<TMessage>          m_MessageList   = new List<TMessage>();
        private INetListener<TMessage>  m_Listener;
        private bool                    m_isShutdowning;

        public NetClient(INetListener<TMessage> listener)
        {
            m_Listener = listener;
            m_Parser = listener.parser;

            m_StreamWriter = new NetStreamWriter(this, listener.sendBufferSize);
            m_StreamReader = new NetStreamReader(this, listener.receiveBufferSize);
        }

        async public Task Connect(string host, int port)
        {
            m_Client = new TcpClient();
            m_Client.NoDelay = true;
            m_Client.ReceiveBufferSize = 256 * 1024;
            m_Client.SendBufferSize = 256 * 1024;
            m_HandleException = false;
            m_Exception = null;

            m_Host = host;
            m_Port = port;

            try
            {
                IPAddress ip = IPAddress.Parse(m_Host);
                state = ConnectState.Connecting;
                await m_Client.ConnectAsync(ip, m_Port);

                m_StreamWriter.Start(m_Client.GetStream());
                m_StreamReader.Start(m_Client.GetStream());

                OnConnectSuccess();
            }
            catch (ArgumentNullException e)
            {
                OnConnectFailed(e);
            }
            catch (ArgumentOutOfRangeException e)
            {
                OnConnectFailed(e);
            }
            catch (ObjectDisposedException e)
            {
                OnConnectFailed(e);
            }
            catch (SocketException e)
            {
                OnConnectFailed(e);
            }
        }

        async public Task Reconnect()
        {
            if (state == ConnectState.Connected || state == ConnectState.Connecting)
                throw new InvalidOperationException("连接中，不能执行重连操作");
            await Connect(m_Host, m_Port);
        }

        /// <summary>
        /// 主动断开连接
        /// </summary>
        public void Shutdown()
        {
            UnityEngine.Debug.LogWarning("Shutdown");
            
            m_isShutdowning = true;

            m_StreamReader.Shutdown();
            m_StreamWriter.Shutdown();

            if (m_Client != null)
            {
                if(m_Client.Connected)
                    m_Client.GetStream().Close();
                m_Client.Close();
            }
            m_Listener.OnPeerClose();               // 主动断开连接
        }

        private void OnConnectSuccess()
        {
            state = ConnectState.Connected;
            m_Listener.OnPeerConnectSuccess();
        }

        private void OnConnectFailed(Exception e)
        {
            state = ConnectState.Disconnected;
            m_Listener.OnPeerConnectFailed(e);
        }

        private void OnDisconnected(Exception e)
        {
            state = ConnectState.Disconnected;
            if(!m_isShutdowning)    // 因主动断开连接而抛出的异常不处理
                m_Listener.OnPeerDisconnected(e);       // 异常断开连接
        }
        
        /// <summary>
        /// 异常断开连接
        /// </summary>
        /// <param name="e"></param>
        void IConnector.RaiseException(Exception e)
        {
            m_HandleException = true;
            m_Exception = e;
        }

        private void HandleException()
        {
            if (m_HandleException && m_Client != null)
            {
                m_HandleException = false;

                if (m_Client != null)
                {
                    if (m_Client.Connected)                          // 当远端主动断开网络时，NetworkStream呈已关闭状态
                        m_Client.GetStream().Close();
                    m_Client.Close();
                    m_Client = null;
                }

                OnDisconnected(m_Exception);
            }
        }

        public void Tick()
        {
            // 接收消息
            ReceiveData();

            HandleException();
        }
        
        private void ReceiveData()
        {
            int offset;
            int length;
            byte[] data = m_StreamReader.FetchBufferToRead(out offset, out length);            // 获取已接收的消息
            if (length == 0)
                return;

            int totalRealLength = 0;            // 实际解析的总长度(byte)
            int startOffset = offset;
            int totalLength = length;
            m_MessageList.Clear();
            while (true)
            {
                int realLength;                 // 单次解析的长度(byte)
                TMessage msg;
                bool success = m_Parser.Deserialize(data, startOffset, totalLength, out realLength, out msg);
                if (success)
                    m_MessageList.Add(msg);

                totalRealLength += realLength;
                startOffset += realLength;
                totalLength -= realLength;

                if (!success || totalRealLength == length)
                    break;                      // 解析失败或者已接收的消息长度解析完了
            }
            m_StreamReader.FinishRead(totalRealLength);     // 实际读取的消息长度

            // dispatch
            m_Listener.OnNetworkReceive(m_MessageList);
        }

        public void SendData(int msgid, TMessage data)
        {
            // 序列化到stream，因buff已预先分配、循环利用，无GC
            int length = m_Parser.GetTotalPacketLen(data);
            MemoryStream stream;
            m_StreamWriter.BeginWrite(length, out stream);
            m_Parser.Serialize(msgid, data, stream);
            m_StreamWriter.EndWrite(length);
        }

        public void SendData(byte[] data)
        {
            m_StreamWriter.Write(data);
        }

        public void SendData(byte[] data, int offset, int length)
        {
            m_StreamWriter.Write(data, offset, length);
        }


        // https://docs.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socket.connected?redirectedfrom=MSDN&view=netcore-3.1#System_Net_Sockets_Socket_Connected
        public bool IsConnected()
        {
            if (m_Client == null || m_Client.Client == null)
                return false;

            if (!m_Client.Client.Connected)      // Connected记录的是最近一次Send或Receive时的状态
                return false;

            bool isConnected = true;
            bool blockingState = m_Client.Client.Blocking;
            try
            {
                byte[] tmp = new byte[1];

                m_Client.Client.Blocking = false;
                m_Client.Client.Send(tmp, 0, 0);
                // Trace.Debug("Connected!");
                isConnected = true;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    UnityEngine.Debug.Log("Still Connected, but the Send would block");
                    isConnected = true;
                }
                else
                {
                    UnityEngine.Debug.Log($"Disconnected: error code {e.NativeErrorCode}!");
                    isConnected = false;
                }
            }
            finally
            {
                m_Client.Client.Blocking = blockingState;
            }
            return isConnected;
        }

        // 适用于对端正常关闭socket下的本地socket状态检测，在非正常关闭如断电、拔网线的情况下不起作用
        public bool IsConnected2()
        {
            if (m_Client == null || m_Client.Client == null)
                return false;

            if (m_Client.Client.Poll(10, SelectMode.SelectRead) && (m_Client.Client.Available == 0) || !m_Client.Client.Connected)
                return false;
            else
                return true;
        }
    }
}