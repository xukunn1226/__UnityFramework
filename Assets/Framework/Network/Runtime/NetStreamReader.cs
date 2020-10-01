using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Framework.NetWork
{
    sealed internal class NetStreamReader : NetStream
    {
        private NetClient           m_NetClient;
        private NetworkStream       m_Stream;
        private byte[]              m_SpanBuffer = new byte[1024];

        internal NetStreamReader(NetClient netClient, int capacity = 8 * 1024)
            : base(capacity)
        {
            if (netClient == null) throw new ArgumentNullException();

            m_NetClient = netClient;
        }

        internal void Start(NetworkStream stream)
        {
            m_Stream = stream;

            // setup environment
            Reset();
            
            Task.Run(ReceiveAsync);
        }

        protected override void Dispose(bool disposing)
        {
            if (m_Disposed)
                return;

            if (disposing)
            {
                // free managed resources
            }

            // free unmanaged resources

            m_Disposed = true;
        }

        private async void ReceiveAsync()
        {
            try
            {
                while (m_NetClient.state == ConnectState.Connected)
                {
                    int freeCount = GetConsecutiveUnusedCapacity();             // 填充连续的空闲空间                
                    if (freeCount == 0)
                        throw new ArgumentOutOfRangeException("ReadAsync: buff is full");

                    int receiveByte = await m_Stream.ReadAsync(Buffer, Head, freeCount);
                    AdvanceHead(receiveByte);

                    if (receiveByte <= 0)              // 连接中断
                    {
                        RaiseException(new Exception("socket disconnected"));
                    }
                }
            }
            catch (SocketException e)
            {
                RaiseException(e);
            }
            catch (ObjectDisposedException e)
            {
                // The NetworkStream is closed
                RaiseException(e);
            }
            catch (InvalidOperationException e)
            {
                // The NetworkStream does not support reading
                RaiseException(e);
            }
            catch (IOException e)
            {
                RaiseException(e);
            }
            catch (ArgumentOutOfRangeException e)
            {
                RaiseException(e);
            }
        }

        private void RaiseException(Exception e)
        {
            m_NetClient.RaiseException(e);
        }

        internal ref readonly byte[] FetchBufferToRead(out int offset, out int length)
        {
            ref readonly byte[] buf = ref BeginRead(out offset, out length);
            if(Head > 0 && Head < Tail)
            { // 当数据跨界时复制至Span Buffer以使空间连续
                if(length > m_SpanBuffer.Length)
                {
                    m_SpanBuffer = new byte[length];
                }
                int countToEnd = Buffer.Length - Tail;
                System.Buffer.BlockCopy(buf, offset, m_SpanBuffer, 0, countToEnd);
                System.Buffer.BlockCopy(buf, 0, m_SpanBuffer, countToEnd, length - countToEnd);
                offset = 0;
                return ref m_SpanBuffer;
            }
            return ref buf;
        }

        internal void FinishRead(int length)
        {
            EndRead(length);
        }
    }
}
