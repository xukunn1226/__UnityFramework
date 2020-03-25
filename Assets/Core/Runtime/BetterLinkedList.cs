using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cache;
using System;

namespace Core
{
    public class BetterLinkedList<T> : IEnumerable<T> where T : class, IBetterLinkedListNode<T>, IPooledObject, new()
    {
        private ObjectPool<T> m_Pool;
        
        public T    Last { get; private set; }

        public T    First { get; private set; }

        public int  Count { get; private set; }


        public BetterLinkedList()
        {
            m_Pool = new ObjectPool<T>();
            First = null;
            Last = null;
            Count = 0;
        }

        public BetterLinkedList(int capacity)
        {
            m_Pool = new ObjectPool<T>(capacity);
            First = null;
            Last = null;
            Count = 0;
        }

        public T AddFirst()
        {
            T item = (T)m_Pool.Get();

            IBetterLinkedListNode<T> node = item;
            node.List = this;
            node.Prev = null;
            node.Next = First;

            if(First != null)
            {
                First.Prev = node;
            }
            First = item;

            if (Last == null)
            {
                Last = First;
            }

            ++Count;
            return item;
        }

        public T AddLast()
        {
            T item = (T)m_Pool.Get();

            IBetterLinkedListNode<T> node = item;
            node.List = this;
            node.Prev = Last;
            node.Next = null;

            if (Last != null)
            {
                Last.Next = node;
            }
            Last = item;

            if (First == null)
            {
                First = Last;
            }

            ++Count;
            return item;
        }

        /// <summary>
        /// add new node after "node"
        /// </summary>
        /// <param name="node"></param>
        /// <returns>T: new node</returns>
        public T AddAfter(T node)
        {
            if (node == null)
                throw new System.NullReferenceException("BetterLinkedList.AddAfter, node == null");

            if (node.List == null || node.List != this)
                throw new System.ArgumentException("BetterLinkedList.AddAfter, node.List == null || node.List != this");

            if (node.Prev != null && node.Prev.List != this)
                throw new System.ArgumentException("BetterLinkedList.AddAfter, node.Prev != null && node.Prev.List != this");

            if (node.Next != null && node.Next.List != this)
                throw new System.ArgumentException("BetterLinkedList.AddAfter, node.Next != null && node.Next.List != this");

            if (node.Prev == null && node.Next == null)
                throw new System.ArgumentException("BetterLinkedList.AddAfter, node.Prev == null && node.Next == null");

            // new Node
            T item = (T)m_Pool.Get();

            // insert new node
            IBetterLinkedListNode<T> newNode = item;
            newNode.List = this;
            newNode.Prev = node;
            newNode.Next = node.Next;

            if(node.Next != null)
            {
                node.Next.Prev = newNode;
            }
            node.Next = newNode;

            // update "Last"
            if(node == Last)
            {
                Last = (T)newNode;
            }
            ++Count;
            return item;
        }

        /// <summary>
        /// add new node before "node"
        /// </summary>
        /// <param name="node"></param>
        /// <returns>T: new node</returns>
        public T AddBefore(T node)
        {
            if (node == null)
                throw new System.NullReferenceException("BetterLinkedList.AddBefore, node == null");

            if (node.List == null || node.List != this)
                throw new System.ArgumentException("BetterLinkedList.AddBefore, node.List == null || node.List != this");

            if (node.Prev != null && node.Prev.List != this)
                throw new System.ArgumentException("BetterLinkedList.AddBefore, node.Prev != null && node.Prev.List != this");

            if (node.Next != null && node.Next.List != this)
                throw new System.ArgumentException("BetterLinkedList.AddBefore, node.Next != null && node.Next.List != this");

            if (node.Prev == null && node.Next == null)
                throw new System.ArgumentException("BetterLinkedList.AddBefore, node.Prev == null && node.Next == null");

            // new Node
            T item = (T)m_Pool.Get();

            // insert new node
            IBetterLinkedListNode<T> newNode = item;
            newNode.List = this;
            newNode.Prev = node.Prev;
            newNode.Next = node;

            if (node.Prev != null)
            {
                node.Prev.Next = newNode;
            }
            node.Prev = newNode;

            // update "First"
            if(node == First)
            {
                First = (T)newNode;
            }
            ++Count;
            return item;
        }

        public void Remove(T node)
        {
            if (node == null)
                throw new System.NullReferenceException("BetterLinkedList.Remove, node == null");

            if (node.List == null || node.List != this)
                throw new System.ArgumentException("BetterLinkedList.Remove, node.List == null || node.List != this");

            if (node.Prev != null && node.Prev.List != this)
                throw new System.ArgumentException("BetterLinkedList.Remove, node.Prev != null && node.Prev.List != this");

            if (node.Next != null && node.Next.List != this)
                throw new System.ArgumentException("BetterLinkedList.Remove, node.Next != null && node.Next.List != this");

            if (node.Prev == null && node.Next == null)
                throw new System.ArgumentException("BetterLinkedList.Remove, node.Prev == null && node.Next == null");

            // update "First" & "Last"
            if(node == First)
            {
                First = (T)node.Next;
            }
            if(node == Last)
            {
                Last = (T)node.Prev;
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
            node.List = null;
            node.Prev = null;
            node.Next = null;
            m_Pool.Return(node);
            --Count;
        }

        public void RemoveFirst()
        {
            if (First == null)
                return;
            Remove(First);
        }

        public void RemoveLast()
        {
            if (Last == null)
                return;
            Remove(Last);
        }


        //public T[] ToArray()
        //{
        //    T[] arr = new T[Count];
        //}

        public void Clear()
        {
            while(First != null)
            {
                RemoveFirst();
            }
        }

        public void Trim()
        {
            if (m_Pool == null)
                throw new System.ArgumentNullException("BetterLinedList.Trim: m_Pool == null");

            m_Pool.Trim();
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

        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            private BetterLinkedList<T> m_Buffer;
            private T m_Current;
            private T m_Cursor;

            internal Enumerator(BetterLinkedList<T> buffer)
            {
                m_Buffer = buffer;
                m_Current = default(T);
                m_Cursor = buffer.First;
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
                if (m_Cursor == null)
                    return false;

                m_Current = m_Cursor;
                m_Cursor = (T)m_Cursor.Next;

                return true;
            }

            void IEnumerator.Reset()
            {
                m_Current = default(T);
                m_Cursor = m_Buffer.First;
            }
        }
    }
}