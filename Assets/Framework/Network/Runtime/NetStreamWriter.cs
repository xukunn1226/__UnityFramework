using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace Framework.NetWork
{
    sealed internal class NetStreamWriter : NetStream
    {
        private IConnector                  m_NetClient;
        private NetworkStream               m_Stream;
        //private SemaphoreSlim               m_SendBufferSema;                       // 控制是否可以消息发送的信号量
                                                                                    // The count is decremented each time a thread enters the semaphore, and incremented each time a thread releases the semaphore
        //private bool                        m_isSendingBuffer;                      // 发送消息IO是否进行中
        private MemoryStream                m_MemoryStream;
        private CancellationTokenSource     m_Cts;
        private Exception                   m_Exception;

        //struct WriteCommand
        //{
        //    public int Head;
        //    public int Fence;
        //}
        //private Queue<WriteCommand>         m_CommandQueue          = new Queue<WriteCommand>(8);

        internal NetStreamWriter(IConnector netClient, int capacity = 8 * 1024)
            : base(capacity)
        {
            if (netClient == null) throw new ArgumentNullException();

            m_NetClient = netClient;
            m_MemoryStream = new MemoryStream(Buffer, true);
        }

        internal void Start(NetworkStream stream)
        {
            Reset();

            m_Stream = stream;
            m_Cts = new CancellationTokenSource();

            Task.Run(WriteAsync, m_Cts.Token);
        }

        internal void Shutdown()
        {
            m_Cts?.Cancel();
        }

        private void WriteAsync()
        {
            try
            {
                while (m_NetClient.state == ConnectState.Connected)
                {
                    if (m_Cts.Token.IsCancellationRequested)
                    {
                        // Clean up here, then...
                        m_Cts.Token.ThrowIfCancellationRequested();
                    }

                    int head = Head;        // Head由主线程维护，记录下来保证子线程作用域中此数值一致性
                    int tail = Tail;
                    int fence = Fence;
                    if (!IsEmpty(head, tail))
                    {                        
                        int length = GetUsedCapacity(head, tail);

                        if (fence > 0)
                        {
                            m_Stream.Write(Buffer, tail, fence - tail);
                            m_Stream.Write(Buffer, 0, head);
                        }
                        else
                        {
                            m_Stream.Write(Buffer, tail, length);
                        }
                        //UnityEngine.Debug.Log($"Write: Head {head}  Tail {tail}");

                        AdvanceTail(length);
                        ResetFence();
                    }
                }
            }
            catch (ObjectDisposedException e)
            {
                // The NetworkStream is closed
                RaiseException(e);
            }
            catch (ArgumentNullException e)
            {
                // The buffer parameter is NULL
                RaiseException(e);
            }
            catch (ArgumentOutOfRangeException e)
            {
                RaiseException(e);
            }
            catch (InvalidOperationException e)
            {
                RaiseException(e);
            }
            catch (IOException e)
            {
                RaiseException(e);
            }
            catch (SocketException e)
            {
                RaiseException(e);
            }
            catch(OperationCanceledException e)
            {
                RaiseException(e);
            }
            catch(Exception e)
            {
                RaiseException(e);
            }
            finally
            {
                m_Cts.Dispose();
                m_Cts = null;
            }
            UnityEngine.Debug.Log($"Exit to net writing thread: {m_Exception?.Message ?? "_______"}");
        }

        private void RaiseException(Exception e)
        {
            m_Exception = e;
            m_NetClient.RaiseException(e);
        }

        /// <summary>
        /// 发送数据，主线程调用
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        internal void Write(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException("data == null");

            // 传入参数的合法性检查:可写入空间大小的检查
            if (offset + length > data.Length)
                throw new ArgumentOutOfRangeException("offset + length > data.Length");

            if (length > GetUnusedCapacity())
                throw new ArgumentOutOfRangeException("NetRingBuffer is FULL, can't write anymore");

            if (Head + length <= Buffer.Length)
            {
                System.Buffer.BlockCopy(data, offset, Buffer, Head, length);
            }
            else
            {
                int countToEnd = Buffer.Length - Head;
                System.Buffer.BlockCopy(data, offset, Buffer, Head, countToEnd);
                System.Buffer.BlockCopy(data, countToEnd, Buffer, 0, length - countToEnd);
            }
            AdvanceHead(length);
        }

        internal void Write(byte[] data)
        {
            Write(data, 0, data.Length);
        }

        /// <summary>
        /// 获取连续地、指定大小(length)的buff，返回给上层写入数据，主线程调用
        /// </summary>
        /// <param name="length">the length of write, expand buffer's capacity internally if necessary</param>
        /// <param name="buf">buffer to write</param>
        /// <param name="offset">the position where can be written</param>
        internal void BeginWrite(int length, out byte[] buf, out int offset)
        {
            int headToEnd = GetConsecutiveUnusedCapacityFromHeadToEnd();        // 优先使用
            if (headToEnd < length)
            { // headToEnd空间不够则跨界从头寻找
                int startToTail = GetConsecutiveUnusedCapacityFromStartToEnd();
                if (startToTail < length)
                {
                    throw new ArgumentOutOfRangeException($"NetRingBuffer: no space to receive data {length}    head: {Head}    tail: {Tail}    headToEnd: {headToEnd}  startToTail: {startToTail}");
                }

                Fence = Head;
                Head = 0;
            }
            offset = Head;
            buf = Buffer;
        }

        /// <summary>
        /// 同上
        /// </summary>
        /// <param name="length"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        internal void BeginWrite(int length, out MemoryStream stream)
        {            
            stream = m_MemoryStream;
            byte[] buf;
            int offset;
            BeginWrite(length, out buf, out offset);
            m_MemoryStream.Seek(offset, SeekOrigin.Begin);
        }

        /// <summary>
        /// 数据写入缓存完成，与FetchBufferToWrite对应，同一帧调用
        /// </summary>
        /// <param name="length"></param>
        internal void EndWrite(int length)
        {
            AdvanceHead(length);
        }
    }
}
