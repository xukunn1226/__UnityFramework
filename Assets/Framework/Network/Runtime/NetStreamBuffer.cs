using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Framework.NetWork
{
    /// <summary>
    /// wrapper of NetworkStream, be responsible for sending/receiving of protocol
    /// </summary>
    public class NetStreamBuffer
    {
        private NetworkStream   m_Stream;

        private byte[]          m_Buffer;
        private int             m_Head;
        private int             m_Tail;
        private int             m_IndexMask;

        public byte[]           Buffer  { get { return m_Buffer; } }

        public int              Head    { get { return m_Head; } }

        public int              Tail    { get { return m_Tail; } }

        public NetStreamBuffer(NetworkStream stream, int capacity = 256 * 1024)
        {
            if (stream == null) throw new ArgumentNullException();

            m_Stream = stream;
            m_Buffer = new byte[Mathf.NextPowerOfTwo(capacity)];
            m_Head = 0;
            m_Tail = 0;
            m_IndexMask = m_Buffer.Length - 1;
        }

        public void Clear()
        {
            m_Head = 0;
            m_Tail = 0;
        }

        public bool IsEmpty()
        {
            return m_Head == m_Tail;
        }

        public bool IsFull()
        {
            return ((m_Head + 1) & m_IndexMask) == m_Tail;
        }

        private int GetFreeCapacity()
        {
            return m_Buffer.Length - 1 - GetUsedCapacity();
        }

        private int GetUsedCapacity()
        {
            return m_Head >= m_Tail ? m_Head - m_Tail : m_Buffer.Length - 1 - (m_Tail - m_Head);
        }

        public bool Write(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException("data == null");

            // 传入参数的合法性检查:可写入空间大小的检查
            if (offset + length > data.Length || length > GetFreeCapacity())
                throw new ArgumentOutOfRangeException();

            if(m_Head + length <= m_Buffer.Length - 1)              // buffer最后一个字节留空
            {
                System.Buffer.BlockCopy(data, offset, m_Buffer, m_Head, length);
            }
            else
            {
                int countToEnd = m_Buffer.Length - 1 - m_Head;      // 到buffer末尾的剩余空间，最后一个字节留空
                System.Buffer.BlockCopy(data, offset, m_Buffer, m_Head, countToEnd);
                System.Buffer.BlockCopy(data, countToEnd, m_Buffer, 0, length - countToEnd);
            }
            m_Head = (m_Head + length) & m_IndexMask;

            return true;
        }

        public bool Write(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data == null");
            return Write(data, 0, data.Length);
        }

        /// <summary>
        /// 异步发送Buff所有数据，由上层决定什么时候发送（最佳实践：一帧调用一次）
        /// </summary>
        /// <returns></returns>
        public async Task FlushWrite()
        {
            try
            {
                if (IsEmpty())
                    return;

                int count = GetUsedCapacity();
                if (m_Head > m_Tail)
                {
                    await m_Stream.WriteAsync(m_Buffer, m_Tail, count);
                }
                else
                {
                    await m_Stream.WriteAsync(m_Buffer, m_Tail, m_Buffer.Length -1 - m_Tail);
                    if(m_Head > 0)
                        await m_Stream.WriteAsync(m_Buffer, 0, m_Head);
                }

                m_Tail = (m_Tail + count) & m_IndexMask;        // 数据发送完成，更新Tail
            }
            catch(Exception e)
            {
                Debug.LogError($"NetStreamBuffer.Flush: {e.Message}");
            }
        }

        /// <summary>
        /// 异步接收消息数据，不负责粘包处理
        /// </summary>
        /// <returns>返回接收到的字节数</returns>
        public async Task<int> ReadAsync()
        {
            try
            {
                if (IsFull())
                    return 0;

                int maxCount = m_Buffer.Length - 1 - m_Head;
                if (m_Tail > m_Head)
                    maxCount = Mathf.Min(maxCount, m_Tail - m_Head);
                
                int count = await m_Stream.ReadAsync(m_Buffer, m_Head, maxCount);
                m_Head = (m_Head + count) & m_IndexMask;
                return count;
            }
            catch(Exception e)
            {
                Debug.LogError($"ReadAsync: {e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 上层解析完消息后需要调用此接口
        /// </summary>
        /// <param name="length"></param>
        public void FinishRead(int length)
        {
            m_Tail = (m_Tail + length) & m_IndexMask;
        }

        public void FetchBuffer(byte[] data)
        {

        }
    }
}