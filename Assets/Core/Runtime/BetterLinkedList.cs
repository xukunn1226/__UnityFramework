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

        public T AddLast()
        {
            return InternalAddAfter(null);
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

            return InternalAddAfter(node);
        }

        private T InternalAddAfter(T node)
        {
            // 允许node为null，此时认为添加至Last
            if(node == null)
            {
                node = Last;
            }

            // node仍可能为null，小心处理
            T item = (T)m_Pool.Get();
            IBetterLinkedListNode<T> newNode = item;
            newNode.List = this;
            newNode.Prev = node;
            newNode.Next = node?.Next ?? null;

            if(node != null && node.Next != null)
            {
                node.Next.Prev = newNode;
            }
            if(node != null)
            {
                node.Next = newNode;
            }

            // update "First" & "Last"
            if(First == null)
            { // add after时newNode不可能为First，除非当前First为空（第一次加入）
                First = item;
            }
            if(newNode.Next == null)
            {
                Last = item;
            }

            ++Count;
            return item;
        }

        public T AddFirst()
        {
            return InternalAddBefore(null);
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

            return InternalAddBefore(node);
        }

        private T InternalAddBefore(T node)
        {
            // 允许node为null，此时认为添加至First
            if (node == null)
            {
                node = First;
            }

            // node仍可能为null，小心处理
            T item = (T)m_Pool.Get();
            IBetterLinkedListNode<T> newNode = item;
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
                First = item;
            }
            if (Last == null)
            { // add before时newNode不可能为Last，除非当前Last为空（第一次加入）
                Last = item;
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

        public bool Contains(T node)
        {
            T item = First;
            while(item != null)
            {
                if (item == node)
                    return true;
                item = (T)item.Next;
            }
            return false;
        }

        public void CopyTo(T[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (index < 0 || index > array.Length)
                throw new ArgumentOutOfRangeException("index");

            if (index + Count > array.Length)
                throw new ArgumentException("Insufficient Space");

            T node = First;
            while(node != null)
            {
                array[index++] = node;
                node = (T)node.Next;
            }
        }

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

            public void Reset()
            {
                m_Current = default(T);
                m_Cursor = m_Buffer.First;
            }
        }
    }
}