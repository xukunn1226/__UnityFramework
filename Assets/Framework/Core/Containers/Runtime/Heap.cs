using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    public class Heap<T> where T : IComparable<T>
    {
        private T[]             m_Buffer;
        private Comparer<T>     m_Comparer;
        
        public Heap() : this(0, null)
        {}

        public Heap(Comparer<T> comparer) : this(0, comparer)
        {}

        public Heap(int capacity, Comparer<T> comparer)
        {
            m_Buffer = new T[capacity];
            m_Comparer = comparer ?? Comparer<T>.Default;
        }

        public int Count { get { return m_Buffer.Length; } }

        public bool IsEmpty { get { return m_Buffer.Length == 0; } }
    }
}