using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 圆形缓冲区，无锁FIFO(first-in first-out queue)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircularQueue<T>
    {
        private CircularBuffer<T>   m_Buffer;
        private int                 m_Head;
        private int                 m_Tail;

        public T this[int index]
        {
            get
            {
                if(index < 0 || index >= Size())
                    throw new System.IndexOutOfRangeException($"CircularQueue[{index}] out of range");
                return m_Buffer[m_Tail + index];
            }
        }

        public CircularQueue(int capacity)
        {
            m_Buffer = new CircularBuffer<T>(capacity);
            m_Head = 0;
            m_Tail = 0;
        }

        public int Size()
        {
            int size = m_Head - m_Tail;
            if (size < 0)
                size += m_Buffer.Capacity;
            return size;
        }

        public int Capacity()
        {
            return m_Buffer.Capacity;
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
            return m_Buffer.GetNextIndex(m_Head) == m_Tail;
        }

        public void Push(T data)
        {
            if(IsFull())
            {
                Pop();
                Push(data);
            }
            else
            {
                m_Buffer[m_Head] = data;
                m_Head = m_Buffer.GetNextIndex(m_Head);                
            }
        }

        public T Pop()
        {
            if (IsEmpty())
                return default(T);

            T data = m_Buffer[m_Tail];
            m_Tail = m_Buffer.GetNextIndex(m_Tail);
            return data;
        }

        public T Peek()
        {
            if (IsEmpty())
                return default(T);
            return m_Buffer[m_Tail];
        }
    }
}