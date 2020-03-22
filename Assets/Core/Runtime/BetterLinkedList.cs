using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cache;

namespace Core
{
    public class BetterLinkedList<T> where T : class, IBetterLinkedListNode<T>, IPooledObject, new()
    {
        private ObjectPool<T> m_Pool;

        public T    Last { get; private set; }

        public T    First { get; private set; }

        public int  Count { get; private set; }


        public BetterLinkedList()
        {
            m_Pool = new ObjectPool<T>();
            Last = null;
            First = null;
            Count = 0;
        }

        public BetterLinkedList(int capacity)
        {
            m_Pool = new ObjectPool<T>(capacity);
            Last = null;
            First = null;
            Count = 0;
        }

        public void AddAfter(T node)
        {
            if(First == null)
            {

            }
        }

        public void AddBefore(T node)
        {

        }

        public T AddFirst()
        {
            T item = (T)m_Pool.Get();

            IBetterLinkedListNode<T> node = item;
            node.List = this;
            node.Prev = null;
            node.Next = First;
            First = item;

            if(Last == null)
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
            Last = item;

            if(First == null)
            {
                First = Last;
            }

            ++Count;

            return item;
        }

        public void Clear()
        { }

        public void CopyTo(T[] array, int index)
        { }

        public void Remove(T node)
        {
            if (node == null)
                throw new System.NullReferenceException("BetterLinkedList.Remove, node == null");

            if (node.List == null || node.List != this)
                throw new System.ArgumentException("node.List == null || node.List != this");

            if (node.Prev != null && node.Prev.List != this)
                throw new System.ArgumentException("node.Prev != null && node.Prev.List != this");

            if (node.Next != null && node.Next.List != this)
                throw new System.ArgumentException("node.Next != null && node.Next.List != this");

            if(node.Prev != null)
            {
                node.Prev.Next = node.Next;
            }

            if(node.Next != null)
            {
                node.Next.Prev = node.Prev;
            }

            m_Pool.Return(node);
            --Count;
        }

        public void RemoveFirst()
        {
            if (First == null)
                return;

            if (First == Last)
            {
                Last = null;
            }

            T tmp = First; 
            First = (T)First.Next;
            m_Pool.Return(tmp);
            --Count;
        }

        public void RemoveLast()
        {
            if (Last == null)
                return;

            if(First == Last)
            {
                First = null;
            }

            T tmp = Last;
            Last = (T)Last.Prev;
            m_Pool.Return(tmp);
            --Count;
        }
    }
}