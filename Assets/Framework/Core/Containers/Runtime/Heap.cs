using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    public class Heap<T> : IEnumerable<T>
    {
        private ArrayList<T>    m_Buffer;
        private Comparer<T>     m_Comparer;
        
        public Heap() : this(0, null)
        {}

        public Heap(Comparer<T> comparer) : this(0, comparer)
        {}

        public Heap(IList<T> arr, Comparer<T> comparer = null) : this(arr.Count, comparer)
        {
            m_Buffer.AddRange(arr);
            BuildHeap();
        }

        public Heap(int capacity, Comparer<T> comparer = null)
        {
            m_Buffer = new ArrayList<T>(capacity);
            m_Comparer = comparer ?? Comparer<T>.Default;
        }

        public int Count { get { return m_Buffer.Count; } }

        public bool IsEmpty { get { return m_Buffer.Count == 0; } }

        public T this[int index]
        {
            get
            {
                if(index < 0 || index >= Count || Count == 0)
                    throw new IndexOutOfRangeException();
                return m_Buffer[index];
            }
            set
            {
                if(index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();
                m_Buffer[index] = value;
                BuildHeap();
            }
        }

        private void BuildHeap()
        {
            int lastIndex = m_Buffer.Count - 1;
            int lastNodeWithChildren = lastIndex / 2;

            for (int node = lastNodeWithChildren; node >= 0; --node)
            {
                Heapify(node, lastIndex);
            }

            while(lastIndex >= 0)
            {
                Swap(m_Buffer, 0, lastIndex);
                --lastIndex;
                Heapify(0, lastIndex);
            }
        }

        private void Heapify(int nodeIndex, int lastIndex)
        {
            // assume left(i) and right(i) are max-heaps
            int left = (nodeIndex * 2) + 1;
            int right = left + 1;
            int largest = nodeIndex;

            // If collection[left] > collection[nodeIndex]
            if (left <= lastIndex && m_Comparer.Compare(m_Buffer[left], m_Buffer[nodeIndex]) > 0)
                largest = left;

            // If collection[right] > collection[largest]
            if (right <= lastIndex && m_Comparer.Compare(m_Buffer[right], m_Buffer[largest]) > 0)
                largest = right;

            // Swap and heapify
            if (largest != nodeIndex)
            {
                Swap(m_Buffer, nodeIndex, largest);
                Heapify(largest, lastIndex);
            }
        }

        public void Add(T value)
        {
            m_Buffer.Add(value);
            BuildHeap();
        }

        public T Peek()
        {
            if(IsEmpty)
                throw new Exception("Heap is empty");
            return m_Buffer.First;
        }

        public T Pop()
        {
            if(IsEmpty)
                throw new Exception("Heap is empty");
            
            T head = Peek();

            // 首尾交换，删除尾部数据
            Swap(m_Buffer, 0, Count - 1);
            m_Buffer.RemoveAt(Count - 1);

            BuildHeap();

            return head;
        }

        public void Clear()
        {
            if (IsEmpty)
                throw new Exception("Heap is empty");
            m_Buffer.Clear();
        }

        public T[] ToArray()
        {
            return m_Buffer.ToArray();
        }

        public List<T> ToList()
        {
            return m_Buffer.ToList();
        }
 
        private void Swap(ArrayList<T> arr, int n, int m)
        {
            T tmp = arr[n];
            arr[n] = arr[m];
            arr[m] = tmp;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for(int i = 0; i < Count; ++i)
            {
                yield return m_Buffer[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}