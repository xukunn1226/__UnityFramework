﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cache
{
    /// <summary>
    /// 非Mono对象缓存池，不支持扩展，
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ObjectPool<T> : IPool where T : IPooledObject, new()
    {
        private Stack<T>            m_Stack;

        public int                  countAll        { get; private set; }

        public int                  countActive     { get { return countAll - countInactive; } }

        public int                  countInactive   { get { return m_Stack.Count; } }
        
        public ObjectPool(int initSize = 0)
        {
            PoolManager.RegisterObjectPool(typeof(T), this);

            initSize = Mathf.Max(0, initSize);

            InitPool(initSize);
        }

        private void InitPool(int initSize)
        {
            if (m_Stack == null)
            {
                m_Stack = new Stack<T>(initSize);
            }

            for (int i = 0; i < initSize; ++i)
            {
                T element = new T();
                element.OnInit();
                m_Stack.Push(element);
                ++countAll;
            }
        }

        public IPooledObject Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                element = new T();
                element.OnInit();
                ++countAll;
            }
            else
            {
                element = m_Stack.Pop();
            }
            element.OnGet();

            return element;
        }

        public void Return(IPooledObject element)
        {
            if (element == null)
                return;

            m_Stack.Push((T)element);
            element.OnRelease();
        }

        public void Clear()
        {
            m_Stack.Clear();
        }

        public void Trim()
        {
            m_Stack.TrimExcess();
        }
    }
}