using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    public interface IPriority
    {
        int GetPriority();
    }

    public class PriorityQueue<TKey> where TKey : class, IPriority
    {
        // static private Comparer<PriorityQueueNode<TKey>> s_MinHeapComparer = Comparer<PriorityQueueNode<TKey>>.Create(MinHeapComparison);
        // static private Comparer<PriorityQueueNode<TKey>> s_MaxHeapComparer = Comparer<PriorityQueueNode<TKey>>.Create(MaxHeapComparison);

        private Heap<PriorityQueueNode<TKey>>   m_Heap;
        private bool                            m_IsMinHeap;

        public PriorityQueue() : this(0, true) {}

        public PriorityQueue(int capacity, bool isMinHeap = true)
        {
            // m_Heap = new Heap<PriorityQueueNode<TKey>>(isMinHeap ? s_MinHeapComparer : s_MaxHeapComparer);
            m_Heap = new Heap<PriorityQueueNode<TKey>>();
            m_IsMinHeap = isMinHeap;
        }

        public PriorityQueue(IList<TKey> arr, bool isMinHeap = true) : this(arr.Count, isMinHeap)
        {
            foreach(var item in arr)
                Push(item);
        }

        // static private int MinHeapComparison(PriorityQueueNode<TKey> left, PriorityQueueNode<TKey> right)
        // {
        //     if(left == null || left.Key == null)
        //         return -1;
        //     if(right == null || right.Key == null)
        //         return 1;
        //     return left.Key.GetPriority().CompareTo(right.Key.GetPriority());
        // }
        
        // static private int MaxHeapComparison(PriorityQueueNode<TKey> left, PriorityQueueNode<TKey> right)
        // {
        //     if(left == null || left.Key == null)
        //         return 1;
        //     if(right == null || right.Key == null)
        //         return -1;                
        //     return right.Key.GetPriority().CompareTo(left.Key.GetPriority());
        // }

        public int  Count   { get { return m_Heap.Count; } }
        public bool IsEmpty { get { return m_Heap.IsEmpty; } }

        public PriorityQueueNode<TKey> Push(TKey value)
        {
            if(value == null)
                throw new ArgumentNullException();

            PriorityQueueNode<TKey> node = new PriorityQueueNode<TKey>(value, m_IsMinHeap);
            m_Heap.Push(node);
            return node;
        }

        public PriorityQueueNode<TKey> Peek()
        {
            return m_Heap.Peek();
        }

        public PriorityQueueNode<TKey> Pop()
        {
            return m_Heap.Pop();
        }

        public void RemoveAt(int index)
        {
            m_Heap.RemoveAt(index);
        }

        public void Remove(PriorityQueueNode<TKey> value)
        {
            int index = FindIndex(value);
            if(index != -1)
                RemoveAt(index);
        }

        public int FindIndex(PriorityQueueNode<TKey> value)
        {
            return m_Heap.FindIndex(value);
        }

        public void Clear()
        {
            m_Heap.Clear();
        }

        public void UpdatePriority(PriorityQueueNode<TKey> value)
        {
            int index = FindIndex(value);
            if(index != -1)
            {
                value.UpdatePriority(value.Key.GetPriority());
                m_Heap[index] = value;
            }
        }
    }

    public class PriorityQueueNode<TKey> : IComparable<PriorityQueueNode<TKey>> where TKey : IPriority
    {
        public TKey     Key         { get; private set; }
        public int      Priority    { get; private set; }

        private bool    m_IsMinHeap;

        protected PriorityQueueNode() : this(default(TKey)) { }

        public PriorityQueueNode(TKey value, bool isMinHeap = true)
        {
            this.Key = value;
            this.Priority = value.GetPriority();
            m_IsMinHeap = isMinHeap;
        }

        public void UpdatePriority(int priority)
        {
            Priority = priority;
        }

        public int CompareTo(PriorityQueueNode<TKey> other)
        {
            if (other == null)
                return -1;

            int compared = this.Priority.CompareTo(other.Priority);
            return m_IsMinHeap ? compared : compared * -1;
        }
    }
}