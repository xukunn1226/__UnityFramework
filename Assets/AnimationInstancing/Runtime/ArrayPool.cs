using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AnimationInstancingModule.Runtime
{
    internal static class ArrayPool<T>
    {
        static private Dictionary<int, Stack<T[]>> m_ArrayPool = new Dictionary<int, Stack<T[]>>();

        static public T[] Get(int n)
        {
            n = Mathf.NextPowerOfTwo(n);
            
            Stack<T[]> pool;
            if(!m_ArrayPool.TryGetValue(n, out pool))
            {
                pool = new Stack<T[]>();
                m_ArrayPool.Add(n, pool);
            }

            T[] element;
            if (pool.Count == 0)
            {
                element = new T[n];
            }
            else
            {
                element = pool.Pop();
            }
            return element;
        }

        static public void Release(T[] element)
        {
            Stack<T[]> pool;
            if(!m_ArrayPool.TryGetValue(element.Length, out pool))
            {
                Debug.LogError("Internal error. Trying to destroy object that is not from pool");
                return;
            }

            pool.Push(element);
        }
    }
}