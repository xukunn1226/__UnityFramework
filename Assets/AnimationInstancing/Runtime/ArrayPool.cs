using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AnimationInstancingModule.Runtime
{
    internal class ArrayPool<T>
    {
        // private readonly Stack<T[]> m_Stack = new Stack<T[]>();
        private Dictionary<int, Stack<T[]>> m_ArrayPool = new Dictionary<int, Stack<T[]>>();
        
        // public int countAll { get; private set; }
        // public int countActive { get { return countAll - countInactive; } }
        // public int countInactive { get { return m_Stack.Count; } }

        public T[] Get(int n)
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

        public void Release(T[] element)
        {
            // if (m_Stack.Count > 0 && element.Length != m_Capacity)
            //     Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            // m_Stack.Push(element);
        }
    }

    // internal static class ArrayPoolList
}