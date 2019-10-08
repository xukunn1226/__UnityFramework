using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class ObjectPool<T> : IPool where T : IPooledObject, new()
    {
        private Stack<T>            m_Stack;

        // 缓存池最大缓存数量，<= 0, 表示没有最大限制
        private readonly int        m_MaxCount;
        
        public ObjectPool(int initSize, int MaxCount = 0)
        {
            initSize = Mathf.Max(0, initSize);
            m_MaxCount = MaxCount <= 0 ? int.MaxValue : Mathf.Max(initSize, MaxCount);

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
            }
        }

        public IPooledObject Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                element = new T();
                element.OnInit();
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

            if (m_Stack.Count < m_MaxCount)
            {
                m_Stack.Push((T)element);
                element.OnRelease();
            }
        }
    }
}