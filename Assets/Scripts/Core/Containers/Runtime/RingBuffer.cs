using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 基于CircularBuffer实现的无锁FIFO(first-in first-out queue)
    /// 单生产、单消费模式时线程安全
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RingBuffer
    {
        private byte[]          m_Buffer;
        private int             m_Head;
        private int             m_Tail;
        private int             m_IndexMask;

        public RingBuffer(int capacity = 256 * 1024)
        {
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
            return m_Head >= m_Tail ? m_Head - m_Tail : m_Buffer.Length - (m_Tail - m_Head);
        }

        public bool Write(byte[] data, int offset, int length)
        {
            // 传入参数的合法性检查
            if (data == null || offset + length > data.Length)
                return false;

            // 可写入空间大小的检查
            if (length > GetFreeCapacity())
                return false;

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
            return Write(data, 0, data.Length);
        }

        public bool Read(byte[] data, int offset, int length)
        {
            // 传入参数的合法性检查
            if (data == null || offset + length > data.Length)
                return false;

            // 可读取空间大小的检查
            if (length > GetUsedCapacity())
                return false;
            
            if(m_Tail + length <= m_Buffer.Length - 1)              // buffer最后一个字节为空
            {
                System.Buffer.BlockCopy(m_Buffer, m_Tail, data, offset, length);
            }
            else
            {
                int countToEnd = m_Buffer.Length - 1 - m_Tail;      // 到buffer末尾的剩余空间，最后一个字节留空
                System.Buffer.BlockCopy(m_Buffer, m_Tail, data, offset, countToEnd);
                System.Buffer.BlockCopy(m_Buffer, 0, data, countToEnd, length - countToEnd);
            }
            m_Tail = (m_Tail + length) & m_IndexMask;

            return true;
        }

        public bool Read(byte[] data)
        {
            return Read(data, 0, data.Length);
        }
    }
}