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
        private NetworkStream               m_NetworkStream;
        //private SemaphoreSlim               m_SendBufferSema;                       // 控制是否可以消息发送的信号量
                                                                                    // The count is decremented each time a thread enters the semaphore, and incremented each time a thread releases the semaphore
        //private bool                        m_isSendingBuffer;                      // 发送消息IO是否进行中
        private MemoryStream                m_MemoryStream;
        private CancellationTokenSource     m_Cts;

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

        internal void Start(NetworkStream stream, CancellationTokenSource cts)
        {
            m_NetworkStream = stream;
            m_Cts = cts;

            Reset();
            m_MemoryStream.Seek(0, SeekOrigin.Begin);
            //m_SendBufferSema?.Dispose();
            //m_SendBufferSema = new SemaphoreSlim(0, 1);
            //m_isSendingBuffer = false;

            Task.Run(WriteAsync, m_Cts.Token);
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
            m_MemoryStream?.Dispose();
            m_NetworkStream?.Dispose();
            //m_SendBufferSema?.Dispose();

            m_Disposed = true;
        }

        /// <summary>
        /// 中止数据发送(WriteAsync)
        /// </summary>
        internal void Shutdown()
        {
            // release semaphore, make WriteAsync jump out from the while loop
            //if (m_SendBufferSema?.CurrentCount == 0)
            //{
            //    m_SendBufferSema.Release();
            //}
            //m_SendBufferSema?.Dispose();
            //m_SendBufferSema = null;
        }

        internal void Flush()
        {
            //if (m_NetClient?.state == ConnectState.Connected &&
            //    m_SendBufferSema != null &&
            //    m_SendBufferSema.CurrentCount == 0 &&           // The number of remaining threads that can enter the semaphore
            //    !m_isSendingBuffer &&                           // 上次消息已发送完成
            //    !IsEmpty())                                     // 已缓存一定的待发送消息
            //{
            //    // cache the pending sending data
            //    m_CommandQueue.Enqueue(new WriteCommand() { Head = this.Head, Fence = this.Fence });

            //    // 每次push command完重置Fence
            //    ResetFence();

            //    m_SendBufferSema.Release();                     // Sema.CurrentCount += 1
            //}
        }

        private async void WriteAsync()
        {
            try
            {
                while (m_NetClient.state == ConnectState.Connected)
                {
                    //await m_SendBufferSema.WaitAsync();         // CurrentCount==0将等待，直到Sema.CurrentCount > 0，执行完Sema.CurrentCount -= 1

                    //// todo：这里还能做优化，把command queue合并，减少WriteAsync调用次数
                    //m_isSendingBuffer = true;
                    //if(m_CommandQueue.Count > 0)
                    //{
                    //    WriteCommand cmd = m_CommandQueue.Peek();

                    //    int length = GetUsedCapacity(cmd.Head);
                    //    if (cmd.Head > Tail)
                    //    {
                    //        await m_NetworkStream.WriteAsync(Buffer, Tail, length);
                    //    }
                    //    else
                    //    {
                    //        if (cmd.Fence > 0)
                    //            await m_NetworkStream.WriteAsync(Buffer, Tail, cmd.Fence - Tail);
                    //        else
                    //            await m_NetworkStream.WriteAsync(Buffer, Tail, Buffer.Length - Tail);

                    //        if (cmd.Head > 0)
                    //            await m_NetworkStream.WriteAsync(Buffer, 0, cmd.Head);
                    //    }

                    //    FinishBufferSending(length);        // 数据发送完成，更新Tail
                    //    m_CommandQueue.Dequeue();
                    //}
                    //m_isSendingBuffer = false;

                    if(IsEmpty())
                    {
                        await Task.Yield();
                    }
                    else
                    {
                        int head = Head;        // Head由主线程维护，记录下来保证子线程作用域中此数值一致性
                        int length = GetUsedCapacity(head);

                        if (Fence > 0)
                        {
                            await m_NetworkStream.WriteAsync(Buffer, Tail, Fence - Tail, m_Cts.Token);
                            await m_NetworkStream.WriteAsync(Buffer, 0, head, m_Cts.Token);
                        }
                        else
                        {
                            await m_NetworkStream.WriteAsync(Buffer, Tail, head - Tail, m_Cts.Token);
                        }

                        FinishBufferSending(length);
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
            catch (TaskCanceledException e)
            {
                if (e.CancellationToken == m_Cts.Token && m_Cts.IsCancellationRequested)
                {
                    UnityEngine.Debug.Log("==================== NetStreamWriter is cancel normally.");
                }
                else
                {
                    RaiseException(e);
                }
            }
            UnityEngine.Debug.Log($"Exit to net writing thread");
        }

        private void RaiseException(Exception e)
        {
            m_NetClient.RaiseException(e);
        }

        /// <summary>
        /// 发送数据，主线程调用
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        internal void Send(byte[] data, int offset, int length)
        {
            Write(data, offset, length);
        }

        internal void Send(byte[] data)
        {
            Write(data);
        }

        /// <summary>
        /// 请求指定长度（length）的连续空间由上层逻辑填充数据，填充完毕调用FinishBufferWriting，主线程调用
        /// </summary>
        /// <param name="length"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        internal bool RequestBufferToWrite(int length, out byte[] buf, out int offset)
        {
            try
            {
                BeginWrite(length, out buf, out offset);
                return true;
            }
            catch (ArgumentOutOfRangeException e)
            {
                UnityEngine.Debug.LogError(e.Message);
                buf = null;
                offset = 0;
                return false;
            }
        }

        /// <summary>
        /// 同上
        /// </summary>
        /// <param name="length"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        internal bool RequestBufferToWrite(int length, out MemoryStream stream)
        {            
            stream = m_MemoryStream;
            try
            {
                byte[] buf;
                int offset;
                BeginWrite(length, out buf, out offset);
                m_MemoryStream.Seek(offset, SeekOrigin.Begin);
                return true;
            }
            catch(ArgumentOutOfRangeException e)
            {
                UnityEngine.Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 见RequestBufferToWrite
        /// </summary>
        /// <param name="length"></param>
        internal void FinishBufferWriting(int length)
        {
            EndWrite(length);
        }
    }
}
