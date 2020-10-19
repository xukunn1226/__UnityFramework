using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    public class PriorityQueue<TKey> where TKey : IComparable<TKey>
    {
        private Heap<TKey>       m_Buffer;

        public PriorityQueue() : this(0, null) {}

        public PriorityQueue(int capacity) : this(capacity, null) {}

        public PriorityQueue(int capacity, Comparer<TKey> comparer = null)
        {
            m_Buffer = new Heap<TKey>(capacity, comparer);
        }

        public int Count { get { return m_Buffer.Count; } }
        public bool IsEmpty { get { return m_Buffer.IsEmpty; } }

        public void Push(TKey value)
        {
            m_Buffer.Push(value);
        }

        public TKey Peek()
        {
            return m_Buffer.Peek();
        }

        public TKey Pop()
        {
            return m_Buffer.Pop();
        }

        public void RemoveAt(int index)
        {
            m_Buffer.RemoveAt(index);
        }

        public void RemoveAt(TKey value)
        {
            m_Buffer.RemoveAt(value);
        }

        public void Clear()
        {
            m_Buffer.Clear();
        }

        public void UpdatePriority(TKey value)
        {
            for(int i = 0; i < Count; ++i)
            {
                if(m_Buffer[i].Equals(value))
                {
                    m_Buffer[i] = value;
                }
            }
        }
    }
}