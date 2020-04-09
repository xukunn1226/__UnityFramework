using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 圆形缓冲区
    /// 缓冲区大小取整至2的幂，为了加速索引操作（使用位与替代耗时的取余）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircularBuffer<T>
    {
        private T[]         m_Buffer;

        private int         m_IndexMask;
        
        public int          Capacity        { get; private set; }

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
            Capacity = Mathf.NextPowerOfTwo(capacity);
            m_Buffer = new T[Capacity];
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