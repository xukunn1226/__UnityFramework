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
        private BetterLinkedList<T>     m_Buffer;

        public int countAll             { get { return countActive + countInactive; } }

        public int countActive          { get { return m_Buffer.Count; } }

        public int countInactive        { get { return m_Buffer.CountOfUnused; } }

        public LinkedObjectPool(int capacity = 0)
        {
            PoolManager.AddObjectPool(typeof(T), this);

            m_Buffer = new BetterLinkedList<T>(capacity);
        }

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <returns></returns>
        public IPooledObject Get() 
        {
            return m_Buffer.AddFirst();
        }

        /// <summary>
        /// 返回缓存对象
        /// </summary>
        /// <param name="item"></param>
        public void Return(IPooledObject element)
        {
            if (element == null)
                throw new System.ArgumentNullException("element");

            m_Buffer.Remove((T)element);
        }

        /// <summary>
        /// 清空缓存池对象
        /// </summary>
        public void Clear()
        {
            m_Buffer?.Clear();
        }

        /// <summary>
        /// trim excess data
        /// </summary>
        public void Trim()
        {
            m_Buffer?.Trim();
        }
    }
}