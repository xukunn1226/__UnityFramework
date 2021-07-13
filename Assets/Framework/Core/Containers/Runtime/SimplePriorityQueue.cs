using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    public class SimplePriorityQueue<TKey> where TKey : IComparable<TKey>
    {
        private BinaryHeap<TKey>       m_Heap;

        public SimplePriorityQueue() : this(0, null) {}

        public SimplePriorityQueue(int capacity) : this(capacity, null) {}

        public SimplePriorityQueue(int capacity, Comparer<TKey> comparer = null)
        {
            m_Heap = new BinaryHeap<TKey>(capacity, comparer);
        }

        public SimplePriorityQueue(IList<TKey> arr, Comparer<TKey> comparer = null)
        {
            m_Heap = new BinaryHeap<TKey>(arr, comparer);
        }

        public int  Count   { get { return m_Heap.Count; } }
        public bool IsEmpty { get { return m_Heap.IsEmpty; } }

        public void Push(TKey value)
        {
            m_Heap.Push(value);
        }

        public TKey Peek()
        {
            return m_Heap.Peek();
        }

        public TKey Pop()
        {
            return m_Heap.Pop();
        }

        public void RemoveAt(int index)
        {
            m_Heap.RemoveAt(index);
        }

        public void Remove(TKey value)
        {
            m_Heap.Remove(value);
        }

        public int FindIndex(TKey value)
        {
            return m_Heap.FindIndex(value);
        }

        public void Clear()
        {
            m_Heap.Clear();
        }

        public void UpdatePriority(TKey value)
        {
            int index = m_Heap.FindIndex(value);
            if(index != -1)
                m_Heap[index] = value;
        }
    }
}