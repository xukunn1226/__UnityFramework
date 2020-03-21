using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cache;

public class BetterLinkedList<T> where T : class, IBetterLinkedListNode<T>, IPooledObject, new()
{
    private ObjectPool<T>   m_Pool;

    public T                Last    { get; private set; }

    public T                First   { get; private set; }

    public int              Count   { get; private set; }


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

    public void AddAfter(T node, T newNode)
    {
    }

    public void AddBefore(T node, T newNode)
    {

    }

    public void AddFirst(T node)
    {
        IBetterLinkedListNode<T> obj = (IBetterLinkedListNode<T>)m_Pool.Get();
    }

    public void AddLast(T node)
    { }

    public void Clear()
    { }

    public void CopyTo(T[] array, int index)
    { }

    public void Remove(T node)
    {

    }

    public void RemoveFirst()
    { }

    public void RemoveLast()
    { }
}
