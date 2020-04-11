using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core
{
    /// <summary>
    /// 圆形缓冲区，无锁FIFO(first-in first-out queue)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircularQueue<T>
    {
        private T[]                 m_Buffer;
        private int                 m_IndexMask;
        private int                 m_Head;
        private int                 m_Tail;

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

                if (!IsFull())
                    throw new System.InvalidOperationException("buffer has space, no expansion required");

                T[] newBuffer = new T[value];
                if(m_Head > m_Tail)
                {
                    Array.Copy(m_Buffer, 0, newBuffer, 0, m_Buffer.Length);
                }
                else
                {
                    // copy [0, m_Head] to newBuffer
                    Array.Copy(m_Buffer, 0, newBuffer, 0, m_Head + 1);

                    // copy [m_Tail, end] to newBuffer
                    int length = m_Buffer.Length - m_Tail;
                    Array.Copy(m_Buffer, m_Tail, newBuffer, value - length, length);
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

        public T this[int index]
        {
            get
            {
                if(index < 0 || index >= Count)
                    throw new System.IndexOutOfRangeException($"CircularQueue[{index}] out of range");
                return m_Buffer[(m_Tail + index) & m_IndexMask];
            }
        }

        public CircularQueue(int capacity)
        {
            m_Buffer = new T[MathUtility.NextPowerOfTwo(capacity)];
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

        public void Push(T data)
        {
            if(IsFull())
            {
                // expand capacity
                Capacity = MathUtility.NextPowerOfTwo(2 * Capacity);

                Push(data);
            }
            else
            {
                m_Buffer[m_Head] = data;
                m_Head = GetNextIndex(m_Head);                
            }
        }

        public T Pop()
        {
            if (IsEmpty())
                throw new System.InvalidOperationException("empty buffer");

            T data = m_Buffer[m_Tail];
            m_Tail = GetNextIndex(m_Tail);
            return data;
        }

        public T Peek()
        {
            if (IsEmpty())
                throw new System.InvalidOperationException("empty buffer");

            return m_Buffer[m_Tail];
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