using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Framework.NetWork.Log;
using System.IO;

namespace Framework.NetWork
{
    public enum ConnectState
    {
        Disconnected,
        Connecting,
        Connected,
    }

    public class NetClient
    {
        public delegate void onConnected(int ret);
        public delegate void onDisconnected(int ret);

        public ConnectState         state { get; private set; } = ConnectState.Disconnected;

        private TcpClient           m_Client;
        
        private string              m_Host;
        private int                 m_Port;

        private onConnected         m_ConnectedHandler;
        private onDisconnected      m_DisconnectedHandler;

        private NetStreamWriter     m_StreamWriter;
        private NetStreamReader     m_StreamReader;

        private bool                m_HandleException;

        public NetClient(string host, int port, int sendBufferSize = 4 * 1024, int receiveBufferSize = 8 * 1024, onConnected connectionHandler = null, onDisconnected disconnectedHandler = null)
        {
            m_ConnectedHandler = connectionHandler;
            m_DisconnectedHandler = disconnectedHandler;

            m_StreamWriter = new NetStreamWriter(this, sendBufferSize);
            m_StreamReader = new NetStreamReader(this, receiveBufferSize);

            m_Host = host;
            m_Port = port;
        }

        async public Task Connect()
        {
            m_Client = new TcpClient();
            m_Client.NoDelay = true;
            //m_Client.SendTimeout = 5000;
            m_HandleException = false;

            try
            {
                IPAddress ip = IPAddress.Parse(m_Host);
                state = ConnectState.Connecting;
                await m_Client.ConnectAsync(ip, m_Port);
                OnConnected(0);

                m_StreamWriter.Start(m_Client.GetStream());
                m_StreamReader.Start(m_Client.GetStream());
            }
            catch (ArgumentNullException e)
            {
                OnConnected(-1, e);
            }
            catch (ArgumentOutOfRangeException e)
            {
                OnConnected(-1, e);
            }
            catch (ObjectDisposedException e)
            {
                OnConnected(-1, e);
            }
            catch (SocketException e)
            {
                OnConnected(-1, e);
            }
        }

        async public Task Reconnect()
        {
            if (state == ConnectState.Connected || state == ConnectState.Connecting)
                throw new InvalidOperationException("连接中，不能执行重连操作");
            await Connect();
        }

        private void OnConnected(int ret, Exception e = null)
        {
            state = ret == 0 ? ConnectState.Connected : ConnectState.Disconnected;
            m_ConnectedHandler?.Invoke(ret);

            if(e != null)
            {
                Trace.Debug(e.ToString());
            }
        }

        private void OnDisconnected(int ret)
        {
            state = ConnectState.Disconnected;
            m_DisconnectedHandler?.Invoke(ret);
        }

        public void Tick()
        {
            m_StreamWriter.Flush();

            HandleException();
        }

        public void Close()
        {
            m_HandleException = true;
        }

        internal void RaiseException(Exception e)
        {
            Trace.Debug(e.ToString());
            m_HandleException = true;
        }

        private void HandleException()
        {
            if (m_HandleException)
            {
                m_HandleException = false;
                InternalClose();
            }
        }

        private void InternalClose()
        {
            if (m_Client != null)
            {
                if (m_Client.Connected)                          // 当远端主动断开网络时，NetworkStream呈已关闭状态
                    m_Client.GetStream().Close();
                m_Client.Close();
                m_Client = null;

                OnDisconnected(0);
            }

            if (m_StreamWriter != null)
            {
                m_StreamWriter.Shutdown();
            }
        }

        public void Send(byte[] buf, int offset, int length)
        {
            try
            {
                m_StreamWriter.Send(buf, offset, length);
            }
            catch (ArgumentNullException e)
            {
                RaiseException(e);
            }
            catch (ArgumentOutOfRangeException e)
            {
                RaiseException(e);
            }
        }

        public void Send(byte[] buf)
        {
            Send(buf, 0, buf.Length);
        }

        /// <summary>
        /// 请求指定长度（length）的连续空间，写入完成后务必调用FinishBufferWriting
        /// </summary>
        /// <param name="length"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public void RequestBufferToWrite(int length, out byte[] buf, out int offset)
        {
            m_StreamWriter.RequestBufferToWrite(length, out buf, out offset);
        }

        /// <summary>
        /// 同上
        /// </summary>
        /// <param name="length"></param>
        /// <param name="stream"></param>
        public void RequestBufferToWrite(int length, out MemoryStream stream)
        {
            m_StreamWriter.RequestBufferToWrite(length, out stream);
        }

        public void FinishBufferWriting(int length)
        {
            m_StreamWriter.FinishBufferWriting(length);
        }

        /// <summary>
        /// 获取可读的连续空间
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public ref readonly byte[] FetchBufferToRead(out int offset, out int length)
        {
            return ref m_StreamReader.FetchBufferToRead(out offset, out length);
        }

        /// <summary>
        /// 实际读取了多少数据
        /// </summary>
        /// <param name="length"></param>
        public void FinishRead(int length)
        {
            m_StreamReader.FinishRead(length);
        }


        // https://docs.microsoft.com/zh-cn/dotnet/api/system.net.sockets.socket.connected?redirectedfrom=MSDN&view=netcore-3.1#System_Net_Sockets_Socket_Connected
        public bool IsConnected()
        {
            if (m_Client == null || m_Client.Client == null)
                throw new ArgumentNullException("socket");

            if (!m_Client.Client.Connected)      // Connected记录的是最近一次Send或Receive时的状态
                return false;

            bool isConnected = true;
            bool blockingState = m_Client.Client.Blocking;
            try
            {
                byte[] tmp = new byte[1];

                m_Client.Client.Blocking = false;
                m_Client.Client.Send(tmp, 0, 0);
                Trace.Debug("Connected!");
                isConnected = true;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    Trace.Debug("Still Connected, but the Send would block");
                    isConnected = true;
                }
                else
                {
                    Trace.Debug($"Disconnected: error code {e.NativeErrorCode}!");
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
                throw new ArgumentNullException("socket");

            if (m_Client.Client.Poll(10, SelectMode.SelectRead) && (m_Client.Client.Available == 0) || !m_Client.Client.Connected)
                return false;
            else
                return true;
        }
    }
}