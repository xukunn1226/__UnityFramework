﻿using System.Collections;
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
        private Stack<T>            m_DeactiveObjects;

        public int                  countAll        { get; private set; }

        public int                  countActive     { get { return countAll - countInactive; } }

        public int                  countInactive   { get { return m_DeactiveObjects.Count; } }
        
        public ObjectPool(int capacity = 0)
        {
            PoolManager.AddObjectPool(typeof(T), this);

            m_DeactiveObjects = new Stack<T>(Mathf.Max(0, capacity));
        }

        public IPooledObject Get()
        {
            T element;
            if (m_DeactiveObjects.Count == 0)
            {
                element = new T();
                element.OnInit();
                ++countAll;
            }
            else
            {
                element = m_DeactiveObjects.Pop();
            }
            element.OnGet();

            return element;
        }

        public void Return(IPooledObject element)
        {
            if (element == null)
                throw new System.ArgumentNullException("element");

            m_DeactiveObjects.Push((T)element);
            element.OnRelease();
        }

        public void Clear()
        {
            m_DeactiveObjects.Clear();
        }

        public void Trim()
        {
            m_DeactiveObjects.TrimExcess();
        }
    }
}