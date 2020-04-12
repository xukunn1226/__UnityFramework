using System.Collections;
using System.Collections.Generic;
using Cache;

namespace Core
{
    /// <summary>
    /// 相比LinkedList优化了LinkedListNode生成的GC
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BetterLinkedList<T> : ICollection<T>
    {
        public class BetterLinkedListNode : IPooledObject
        {
            public BetterLinkedList<T>  List { get; internal set; }

            public BetterLinkedListNode Next { get; internal set; }

            public BetterLinkedListNode Prev { get; internal set; }

            public T                    Item { get; internal set; }

            public void OnInit() { }

            public void OnGet() { }

            public void OnRelease()
            {
                List = null;
                Next = null;
                Prev = null;
                Item = default(T);
                Pool = null;
            }

            public void ReturnToPool()
            {
                if(Pool == null)
                    throw new System.ArgumentNullException("Pool");

                Pool.Return(this);
            }

            public IPool Pool { get; set; }
        }

        static private ObjectPool<BetterLinkedListNode> m_Pool;

        static private int k_DefaultNodePoolCapacity = 8;

        static private BetterLinkedListNode Get()
        {
            if(m_Pool == null)
            {
                m_Pool = new ObjectPool<BetterLinkedListNode>(k_DefaultNodePoolCapacity);
            }
            return (BetterLinkedListNode)m_Pool.Get();
        }

        static private void Release(BetterLinkedListNode node)
        {
            if (m_Pool == null)
                throw new System.ArgumentNullException("node");

            m_Pool.Return(node);
        }

        public BetterLinkedListNode First { get; private set; }

        public BetterLinkedListNode Last { get; private set; }

        public int Count { get; private set; }

        public bool IsReadOnly { get { return false; } }

        public BetterLinkedList()
        {
            First = null;
            Last = null;
            Count = 0;
        }
        
        void ICollection<T>.Add(T item)
        {
            AddLast(item);
        }

        public void AddBefore(BetterLinkedListNode node, BetterLinkedListNode newNode)
        {
            if (newNode == null)
                throw new System.ArgumentNullException("newNode");

            if (newNode.List == null)
                throw new System.ArgumentException("newNode.List == null");

            if (newNode.List != this)
                throw new System.InvalidOperationException("newNode.List != null, newNode is attached other list");

            InternalAddBefore(node, newNode);
        }

        public BetterLinkedListNode AddBefore(BetterLinkedListNode node, T value)
        {
            if (value == null)
                throw new System.ArgumentNullException("value");

            BetterLinkedListNode newNode = Get();
            newNode.List = this;
            newNode.Item = value;
            InternalAddBefore(node, newNode);

            return newNode;
        }

        public void AddFirst(BetterLinkedListNode newNode)
        {
            AddBefore(null, newNode);
        }

        public BetterLinkedListNode AddFirst(T value)
        {
            return AddBefore(null, value);
        }

        private void InternalAddBefore(BetterLinkedListNode node, BetterLinkedListNode newNode)
        {
            // 允许node为null，此时认为添加至First
            if (node == null)
            {
                node = First;
            }

            // node仍可能为null，小心处理
            newNode.Pool = m_Pool;
            newNode.List = this;
            newNode.Prev = node?.Prev ?? null;
            newNode.Next = node;

            if (node != null && node.Prev != null)
            {
                node.Prev.Next = newNode;
            }
            if (node != null)
            {
                node.Prev = newNode;
            }

            // update "First" & "Last"
            if (newNode.Prev == null)
            {
                First = newNode;
            }
            if (Last == null)
            { // add before时newNode不可能为Last，除非当前Last为空（第一次加入）
                Last = newNode;
            }

            ++Count;
        }

        public void AddAfter(BetterLinkedListNode node, BetterLinkedListNode newNode)
        {
            if (newNode == null)
                throw new System.ArgumentNullException("newNode");

            if (newNode.List == null)
                throw new System.ArgumentException("newNode.List == null");

            if (newNode.List != this)
                throw new System.InvalidOperationException("newNode.List != null, newNode is attached other list");

            InternalAddAfter(node, newNode);
        }

        public BetterLinkedListNode AddAfter(BetterLinkedListNode node, T value)
        {
            if (value == null)
                throw new System.ArgumentNullException("value");

            BetterLinkedListNode newNode = Get();
            newNode.List = this;
            newNode.Item = value;
            InternalAddAfter(node, newNode);

            return newNode;
        }

        public void AddLast(BetterLinkedListNode newNode)
        {
            AddAfter(null, newNode);
        }

        public BetterLinkedListNode AddLast(T value)
        {
            return AddAfter(null, value);
        }

        private void InternalAddAfter(BetterLinkedListNode node, BetterLinkedListNode newNode)
        {
            // 允许node为null，此时认为添加至Last
            if (node == null)
            {
                node = Last;
            }

            // node仍可能为null，小心处理
            newNode.Pool = m_Pool;
            newNode.Prev = node;
            newNode.Next = node?.Next ?? null;

            if (node != null && node.Next != null)
            {
                node.Next.Prev = newNode;
            }
            if (node != null)
            {
                node.Next = newNode;
            }

            // update "First" & "Last"
            if (First == null)
            { // add after时newNode不可能为First，除非当前First为空（第一次加入）
                First = newNode;
            }
            if (newNode.Next == null)
            {
                Last = newNode;
            }

            ++Count;
        }
        
        public void Clear()
        {
            while(First != null)
            {
                RemoveFirst();
            }
        }

        public bool Contains(T item)
        {
            return Find(item) != null;
        }

        public void CopyTo(T[] array, int index)
        {
            if (array == null)
                throw new System.ArgumentNullException("array");

            if (index < 0 || index > array.Length)
                throw new System.ArgumentOutOfRangeException("index");

            if (index + Count > array.Length)
                throw new System.ArgumentException("Insufficient Space");

            BetterLinkedListNode node = First;
            while (node != null)
            {
                array[index++] = node.Item;
                node = node.Next;
            }
        }

        public BetterLinkedListNode Find(T value)
        {
            EqualityComparer<T> c = EqualityComparer<T>.Default;
            BetterLinkedListNode node = First;
            while(node != null)
            {
                if(value != null)
                {
                    if(c.Equals(node.Item, value))
                    {
                        return node;
                    }
                }
                else
                {
                    if(node.Item == null)
                    {
                        return node;
                    }
                }

                node = node.Next;
            }
            return null;
        }

        public BetterLinkedListNode FindLast(T value)
        {
            EqualityComparer<T> c = EqualityComparer<T>.Default;
            BetterLinkedListNode node = Last;
            while (node != null)
            {
                if (value != null)
                {
                    if (c.Equals(node.Item, value))
                    {
                        return node;
                    }
                }
                else
                {
                    if (node.Item == null)
                    {
                        return node;
                    }
                }

                node = node.Prev;
            }
            return null;
        }

        public void RemoveFirst()
        {
            if (First == null)
                return;
            InternalRemove(First);
        }

        public void RemoveLast()
        {
            if (Last == null)
                return;
            InternalRemove(Last);
        }

        public bool Remove(T item)
        {
            BetterLinkedListNode node = Find(item);
            if(node != null)
            {
                InternalRemove(node);
                return true;
            }
            return false;
        }

        public void Remove(BetterLinkedListNode node)
        {
            if (node == null)
                throw new System.ArgumentNullException("node");

            if (node.List == null)
                throw new System.ArgumentException("node.List == null");

            if (node.List != this)
                throw new System.InvalidOperationException("node.List != null, node is attached other list");

            InternalRemove(node);
        }

        private void InternalRemove(BetterLinkedListNode node)
        {
            // update "First" & "Last"
            if (node == First)
            {
                First = node.Next;
            }
            if (node == Last)
            {
                Last = node.Prev;
            }

            // relink
            if (node.Prev != null)
            {
                node.Prev.Next = node.Next;
            }
            if (node.Next != null)
            {
                node.Next.Prev = node.Prev;
            }

            // recycle
            Release(node);
            --Count;
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
            private BetterLinkedList<T>     m_List;
            private BetterLinkedListNode    m_Current;
            private BetterLinkedListNode    m_Cursor;

            internal Enumerator(BetterLinkedList<T> list)
            {
                m_List = list;
                m_Current = null;
                m_Cursor = list.First;
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
                    return m_Current != null ? m_Current.Item : default(T);
                }
            }

            public bool MoveNext()
            {
                if (m_Cursor == null)
                    return false;

                m_Current = m_Cursor;
                m_Cursor = m_Cursor.Next;

                return true;
            }

            public void Reset()
            {
                m_Current = null;
                m_Cursor = m_List.First;
            }
        }
    }
}