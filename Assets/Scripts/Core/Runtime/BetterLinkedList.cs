using System.Collections;
using System.Collections.Generic;
using Cache;

namespace Core
{
    public class BetterLinkedList<T> : ICollection<T>// where T : IPooledObject
    {
        public class BetterLinkedListNode : IPooledObject
        {
            public BetterLinkedListNode next { get; internal set; }

            public BetterLinkedListNode prev { get; internal set; }

            public T                    item { get; internal set; }

            public void OnInit() { }

            public void OnGet() { }

            public void OnRelease() { }

            public void ReturnToPool() { }

            public IPool Pool { get; set; }
        }

        static private ObjectPool<BetterLinkedListNode> m_Pool;

        public BetterLinkedListNode First { get; private set; }

        public BetterLinkedListNode Last { get; private set; }

        public int Count { get; private set; }

        public bool IsReadOnly { get { return false; } }

        public BetterLinkedList()
        {
            First.next = null;
        }


        public void Add(T item)
        { }

        public void Clear()
        { }

        public bool Contains(T item)
        {
            return true;
        }

        public void CopyTo(T[] array, int arrayIndex)
        { }

        public bool Remove(T item)
        {
            return true;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private T m_Current;

            internal Enumerator(BetterLinkedList<T> list)
            {
                m_Current = default(T);
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get
                {
                    return m_Current;
                }
            }

            public T Current
            {
                get
                {
                    return m_Current;
                }
            }

            public bool MoveNext()
            {
                return true;
            }

            public void Reset()
            {
            }
        }
    }
}