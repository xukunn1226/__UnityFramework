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
            m_MemoryStream.Seek(0, SeekOrigin.Begin);

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

            m_Disposed = true;
        }

        public void Cancel()
        {
            m_Cts?.Cancel();
        }

        private void WriteAsync()
        {
            try
            {
                while (m_NetClient.state == ConnectState.Connected)
                {
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
                        UnityEngine.Debug.Log($"Write: Head {head}  Tail {tail}");

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
            UnityEngine.Debug.Log($"Exit to net writing thread: {m_Exception?.Message ?? ""}");
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
        internal void Send(byte[] data, int offset, int length)
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

        internal void Send(byte[] data)
        {
            Send(data, 0, data.Length);
        }

        /// <summary>
        /// 获取连续地、指定大小(length)的buff，返回给上层写入数据，主线程调用
        /// </summary>
        /// <param name="length">the length of write, expand buffer's capacity internally if necessary</param>
        /// <param name="buf">buffer to write</param>
        /// <param name="offset">the position where can be written</param>
        private void BeginWrite(int length, out byte[] buf, out int offset)
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
        /// 数据写入缓存完成，与FetchBufferToWrite对应，同一帧调用
        /// </summary>
        /// <param name="length"></param>
        internal void FinishBufferWriting(int length)
        {
            AdvanceHead(length);
        }

        /// <summary>
        /// reset Fence
        /// </summary>
        private void ResetFence()
        {
            Fence = 0;
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

        // private async void WriteAsync()
        // {
        //     try
        //     {
        //         while (m_NetClient.state == ConnectState.Connected)
        //         {
        //             //await m_SendBufferSema.WaitAsync();         // CurrentCount==0将等待，直到Sema.CurrentCount > 0，执行完Sema.CurrentCount -= 1

        //             //// todo：这里还能做优化，把command queue合并，减少WriteAsync调用次数
        //             //m_isSendingBuffer = true;
        //             //if(m_CommandQueue.Count > 0)
        //             //{
        //             //    WriteCommand cmd = m_CommandQueue.Peek();

        //             //    int length = GetUsedCapacity(cmd.Head);
        //             //    if (cmd.Head > Tail)
        //             //    {
        //             //        await m_NetworkStream.WriteAsync(Buffer, Tail, length);
        //             //    }
        //             //    else
        //             //    {
        //             //        if (cmd.Fence > 0)
        //             //            await m_NetworkStream.WriteAsync(Buffer, Tail, cmd.Fence - Tail);
        //             //        else
        //             //            await m_NetworkStream.WriteAsync(Buffer, Tail, Buffer.Length - Tail);

        //             //        if (cmd.Head > 0)
        //             //            await m_NetworkStream.WriteAsync(Buffer, 0, cmd.Head);
        //             //    }

        //             //    FinishBufferSending(length);        // 数据发送完成，更新Tail
        //             //    m_CommandQueue.Dequeue();
        //             //}
        //             //m_isSendingBuffer = false;

        //             if(IsEmpty())
        //             {
        //                 await Task.Yield();
        //             }
        //             else
        //             {
        //                 int head = Head;        // Head由主线程维护，记录下来保证子线程作用域中此数值一致性
        //                 int length = GetUsedCapacity(head);

        //                 if (Fence > 0)
        //                 {
        //                     await m_Stream.WriteAsync(Buffer, Tail, Fence - Tail, m_Cts.Token);
        //                     await m_Stream.WriteAsync(Buffer, 0, head, m_Cts.Token);
        //                 }
        //                 else
        //                 {
        //                     await m_Stream.WriteAsync(Buffer, Tail, head - Tail, m_Cts.Token);
        //                 }

        //                 FinishBufferSending(length);
        //                 ResetFence();
        //             }
        //         }
        //     }
        //     catch (ObjectDisposedException e)
        //     {
        //         // The NetworkStream is closed
        //         RaiseException(e);
        //     }
        //     catch (ArgumentNullException e)
        //     {
        //         // The buffer parameter is NULL
        //         RaiseException(e);
        //     }
        //     catch (ArgumentOutOfRangeException e)
        //     {
        //         RaiseException(e);
        //     }
        //     catch (InvalidOperationException e)
        //     {
        //         RaiseException(e);
        //     }
        //     catch (IOException e)
        //     {
        //         RaiseException(e);
        //     }
        //     catch (SocketException e)
        //     {
        //         RaiseException(e);
        //     }
        //     catch(OperationCanceledException e)
        //     {
        //         RaiseException(e);
        //     }
        //     catch(Exception e)
        //     {
        //         RaiseException(e);
        //     }
        //     finally
        //     {
        //         m_Cts.Dispose();
        //         m_Cts = null;
        //     }
        //     // catch (TaskCanceledException e)
        //     // {
        //     //     if (e.CancellationToken == m_Cts.Token && m_Cts.IsCancellationRequested)
        //     //     {
        //     //         UnityEngine.Debug.Log("==================== NetStreamWriter is cancel normally.");
        //     //     }
        //     //     else
        //     //     {
        //     //         RaiseException(e);
        //     //     }
        //     // }
        //     UnityEngine.Debug.Log($"Exit to net writing thread: {m_Exception?.Message ?? ""}");
        // }
    }
}
