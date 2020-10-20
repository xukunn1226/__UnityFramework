using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    public class Heap<T>
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

        public int  Count   { get { return m_Buffer.Count; } }

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

                if(index > 0 && m_Comparer.Compare(m_Buffer[index], m_Buffer[(index - 1) / 2]) < 0)
                { // 小于父节点仅上浮即可
                    SiftUp(index);
                }
                else
                { // 大于父节点则判断其子树
                    Heapify(index, Count - 1);
                }
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
        }
        
        private void SiftUp(int nodeIndex)
        {
            int parent = (nodeIndex - 1) / 2;
            while (m_Comparer.Compare(m_Buffer[nodeIndex], m_Buffer[parent]) < 0)
            {
                Swap(m_Buffer, parent, nodeIndex);
                nodeIndex = parent;
                parent = (nodeIndex - 1) / 2;
            }
        }

        private void Heapify(int nodeIndex, int lastIndex)
        {
            // assume left(i) and right(i) are min-heaps
            int left = (nodeIndex * 2) + 1;
            int right = left + 1;
            int largest = nodeIndex;

            // If m_Buffer[left] < m_Buffer[nodeIndex]
            if (left <= lastIndex && m_Comparer.Compare(m_Buffer[left], m_Buffer[nodeIndex]) < 0)
                largest = left;

            // If m_Buffer[right] < m_Buffer[largest]
            if (right <= lastIndex && m_Comparer.Compare(m_Buffer[right], m_Buffer[largest]) < 0)
                largest = right;

            // Swap and heapify
            if (largest != nodeIndex)
            {
                Swap(m_Buffer, nodeIndex, largest);
                Heapify(largest, lastIndex);
            }
        }

        public void Push(T value)
        {
            m_Buffer.Add(value);
            SiftUp(m_Buffer.Count - 1);
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

            Heapify(0, Count - 1);

            return head;
        }

        public void Remove(T value)
        {
            int index = FindIndex(value);
            if(index != -1)
                RemoveAt(index);
        }

        public int FindIndex(T value)
        {
            if(IsEmpty)
                return -1;

            return InternalFindIndex(value, 0, Count - 1);
        }

        private int InternalFindIndex(T value, int nodeIndex, int lastIndex)
        {
            int compared = m_Comparer.Compare(m_Buffer[nodeIndex], value);

            if (compared > 0)
                return -1;
            if( compared == 0)
                return nodeIndex;

            int left = (nodeIndex * 2) + 1;
            int right = left + 1;

            int index = -1;
            if(left <= lastIndex && index == -1)
                index = InternalFindIndex(value, left, lastIndex);                
            if(right <= lastIndex && index == -1)
                index = InternalFindIndex(value, right, lastIndex);

            return index;
        }

        public void RemoveAt(int index)
        {
            if(index < 0 || index >= Count || Count == 0)
                throw new IndexOutOfRangeException();

            Swap(m_Buffer, index, Count - 1);
            m_Buffer.RemoveAt(Count - 1);

            Heapify(index, Count - 1);
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
    }
}