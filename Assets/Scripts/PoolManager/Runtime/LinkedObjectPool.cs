using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;

namespace Cache
{
    /// <summary>
    /// 对象池（非Mono对象），记录使用中与未使用的对象，方便追踪数据使用情况
    /// WARNING: 对象只能由ObjectPool或LinkedObjectPool之一管理，二者互斥
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class LinkedObjectPool<T> : IPool where T : class, IBetterLinkedListNode<T>, IPooledObject, new()
    {
        private BetterLinkedList<T>     m_ActiveObjects;
        private Stack<T>                m_DeactiveObjects;


        public int countAll             { get { return countActive + countInactive; } }

        public int countActive          { get { return m_ActiveObjects.Count; } }

        public int countInactive        { get { return m_DeactiveObjects.Count; } }

        public LinkedObjectPool(int capacity = 0)
        {
            PoolManager.AddObjectPool(typeof(T), this);

            m_DeactiveObjects = new Stack<T>(capacity >> 1);
            m_ActiveObjects = new BetterLinkedList<T>(capacity >> 1);
        }

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <returns></returns>
        public IPooledObject Get() 
        {
            T element;
            if (m_DeactiveObjects.Count == 0)
            {
                element = m_ActiveObjects.AddFirst();

                // m_ActiveObjects也是对象池管理，内部会调用OnInit
                //element.OnInit();

                // 同OnInit
                //element.OnGet();
            }
            else
            {
                element = m_DeactiveObjects.Pop();
                element.OnGet();
            }

            return element;
        }

        /// <summary>
        /// 返回缓存对象
        /// </summary>
        /// <param name="item"></param>
        public void Return(IPooledObject element)
        {
            if (element == null)
                throw new System.ArgumentNullException("element");

            m_ActiveObjects.Remove((T)element);
            m_DeactiveObjects.Push((T)element);

            // m_ActiveObjects内部回收时会调用OnRelease
            //element.OnRelease();
        }

        /// <summary>
        /// 清空缓存池对象
        /// </summary>
        public void Clear()
        {
            m_DeactiveObjects?.Clear();
            m_ActiveObjects?.Clear();
        }

        /// <summary>
        /// trim excess data
        /// </summary>
        public void Trim()
        {
            m_DeactiveObjects?.TrimExcess();
            m_ActiveObjects?.Trim();
        }
    }
}