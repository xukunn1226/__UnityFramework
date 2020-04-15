using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

namespace Framework.Core
{
    /// <summary>
    /// 基于CircularBuffer实现的无锁FIFO(first-in first-out queue)
    /// 单生产、单消费模式时线程安全
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RingBuffer
    {
        private byte[]          m_Buffer;
        private int             m_IndexMask;
        private int             m_Head;
        private int             m_Tail;

        public int Capacity
        {
            get
            {
                return m_Buffer.Length;
            }
            private set
            {
                if (m_Buffer.Length == value)
                    throw new System.ArgumentOutOfRangeException("Capacity");

                byte[] newBuffer = new byte[value];
                if (m_Head > m_Tail)
                {
                    Buffer.BlockCopy(m_Buffer, 0, newBuffer, 0, m_Buffer.Length);
                }
                else if(m_Head < m_Tail)
                {
                    // copy [0, m_Head] to newBuffer
                    Buffer.BlockCopy(m_Buffer, 0, newBuffer, 0, m_Head + 1);

                    // copy [m_Tail, end] to newBuffer
                    int length = m_Buffer.Length - m_Tail;
                    Buffer.BlockCopy(m_Buffer, m_Tail, newBuffer, value - length, length);
                    m_Tail = value - length;
                }
                m_Buffer = newBuffer;
                m_IndexMask = m_Buffer.Length - 1;
            }
        }

        public int Count
        {
            get
            {
                int size = m_Head - m_Tail;
                if (size < 0)
                    size += m_Buffer.Length;
                return size;
            }
        }

        public RingBuffer(int capacity = 256 * 1024)
        {
            m_Buffer = new byte[MathUtility.NextPowerOfTwo(capacity)];
            m_IndexMask = m_Buffer.Length - 1;
            m_Head = 0;
            m_Tail = 0;
        }

        public void Clear()
        {
            m_Tail = m_Head;
        }

        public bool IsEmpty()
        {
            return m_Head == m_Tail;
        }

        public bool IsFull()
        {
            return GetNextIndex(m_Head) == m_Tail;
        }

        private int GetFreeCapacity()
        {
            return m_Buffer.Length - 1 - Count;     // 最后一个字节留空
        }

        public void Write(byte[] data, int offset, int length)
        {
            // 传入参数的合法性检查
            if (data == null)
                throw new System.ArgumentNullException("data");

            if (offset + length > data.Length)
                throw new System.ArgumentOutOfRangeException("offset + length");

            if (length <= 0)
                throw new System.ArgumentException($"length[{length} is illegal.]");

            if (length > GetFreeCapacity())
            {
                // expand capacity
                int newCapacity = MathUtility.NextPowerOfTwo(Count + length + 1);       // 需要多一个字节留空
                Capacity = newCapacity;
            }

            if(m_Head < m_Tail)
            {
                Buffer.BlockCopy(data, offset, m_Buffer, m_Head, length);
            }
            else
            {
                int countToEnd = m_Buffer.Length - m_Head;
                Buffer.BlockCopy(data, offset, m_Buffer, m_Head, Mathf.Min(length, countToEnd));
                if(length > countToEnd)
                {
                    Buffer.BlockCopy(data, offset + Mathf.Min(length, countToEnd), m_Buffer, 0, length - countToEnd);
                }
            }
            m_Head = (m_Head + length) & m_IndexMask;
        }

        public void Write(byte[] data)
        {
            Write(data, 0, data.Length);
        }

        /// <summary>
        /// read data from ring buffer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns>实际读取数据的字节长度</returns>
        public int Read(byte[] data, int offset, int length)
        {
            // 传入参数的合法性检查
            if (data == null)
                throw new System.ArgumentNullException("data");

            if (offset + length > data.Length)
                throw new System.ArgumentOutOfRangeException("offset + length");

            if (IsEmpty())
                return 0;

            int realLen = Math.Min(length, Count);      // 实际读取数据字节长度
            if(m_Head > m_Tail)
            {
                Buffer.BlockCopy(m_Buffer, m_Tail, data, offset, realLen);
            }
            else
            {
                int countToEnd = m_Buffer.Length - m_Tail;
                Buffer.BlockCopy(m_Buffer, m_Tail, data, offset, Math.Min(countToEnd, realLen));
                if(realLen > countToEnd)
                {
                    Buffer.BlockCopy(m_Buffer, 0, data, Math.Min(countToEnd, realLen), realLen - countToEnd);
                }
            }
            m_Tail = (m_Tail + realLen) & m_IndexMask;

            return realLen;
        }

        public int Read(byte[] data)
        {
            return Read(data, 0, data.Length);
        }

        private int GetNextIndex(int index)
        {
            return (index + 1) & m_IndexMask;
        }

        private int GetPrevIndex(int index)
        {
            return (index - 1) & m_IndexMask;
        }
    }
}