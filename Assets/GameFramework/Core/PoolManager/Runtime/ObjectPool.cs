using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cache
{
    /// <summary>
    /// 对象池（非Mono对象），仅记录未使用的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ObjectPool<T> : IPool where T : IPooledObject, new()
    {
        private Stack<T>            m_UnusedObjects;

        public Stack<T>             unusedObjects   { get { return m_UnusedObjects; } }

        public int                  countAll        { get; private set; }

        public int                  countOfUsed     { get { return countAll - countOfUnused; } }

        public int                  countOfUnused   { get { return m_UnusedObjects.Count; } }
        
        public ObjectPool(int capacity = 0)
        {
            PoolManager.AddObjectPool(typeof(T), this);

            m_UnusedObjects = new Stack<T>(Mathf.Max(0, capacity));
        }

        public IPooledObject Get()
        {
            T element;
            if (m_UnusedObjects.Count == 0)
            {
                element = new T();
                element.OnInit();
                ++countAll;
            }
            else
            {
                element = m_UnusedObjects.Pop();
            }
            element.OnGet();

            return element;
        }

        public void Return(IPooledObject element)
        {
            if (element == null)
                throw new System.ArgumentNullException("element");

            m_UnusedObjects.Push((T)element);
            element.OnRelease();
        }

        public void Clear()
        {
            m_UnusedObjects.Clear();
        }

        public void Trim()
        {
            m_UnusedObjects.TrimExcess();
        }
    }
}