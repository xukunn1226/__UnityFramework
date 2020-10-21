using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    public interface IDelayed<T> : IComparable<T>
    {
        long GetDelay();
    }
    public class DelayQueue<T> where T : IDelayed<T>
    {
        private SimplePriorityQueue<T>    m_PriorityQueue;

        public DelayQueue() : this(0) {}
        public DelayQueue(int capacity)
        {
            m_PriorityQueue = new SimplePriorityQueue<T>(capacity);
        }

        public int Count { get { return m_PriorityQueue.Count; } }

        public void Push(T value)
        {
            m_PriorityQueue.Push(value);
        }

        public void Remove(T value)
        {
            m_PriorityQueue.Remove(value);
        }

        public void Clear()
        {
            m_PriorityQueue.Clear();
        }

        public T Poll()
        {
            if(m_PriorityQueue.IsEmpty)
                return default(T);

            T item = m_PriorityQueue.Peek();
            if(item.GetDelay() <= 0)
            {
                return m_PriorityQueue.Pop();
            }
            return default(T);
        }
    }
}