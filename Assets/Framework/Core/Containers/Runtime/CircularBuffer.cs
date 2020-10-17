using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.Core
{
    /// <summary>
    /// 圆形缓冲区
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircularBuffer<T>
    {
        private T[]         m_Buffer;

        private int         m_IndexMask;
        
        public int          Capacity
        {
            get
            {
                return m_Buffer.Length;
            }
            set
            {
                if (m_Buffer.Length < value)
                {
                    int newCapacity = MathUtility.NextPowerOfTwo(value);
                    if (newCapacity != m_Buffer.Length)
                    {
                        T[] newItems = new T[newCapacity];
                        if (m_Buffer.Length > 0)
                        {
                            Array.Copy(m_Buffer, 0, newItems, 0, m_Buffer.Length);
                        }
                        m_Buffer = newItems;
                        m_IndexMask = m_Buffer.Length - 1;
                    }
                }
            }
        }

        public T this[int index]
        {
            get
            {
                return m_Buffer[index & m_IndexMask];
            }
            set
            {
                m_Buffer[index & m_IndexMask] = value;
            }
        }

        public CircularBuffer(int capacity)
        {
            m_Buffer = new T[MathUtility.NextPowerOfTwo(capacity)];
            m_IndexMask = Capacity - 1;
        }

        public int GetNextIndex(int index)
        {
            return (index + 1) & m_IndexMask;
        }

        public int GetPrevIndex(int index)
        {
            return (index - 1) & m_IndexMask;
        }
    }
}