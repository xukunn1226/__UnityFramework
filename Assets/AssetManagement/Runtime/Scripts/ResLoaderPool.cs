using System.Collections.Generic;
using UnityEngine;


namespace AssetManagement.Runtime
{
    /// <summary>
    /// 负责对AssetBundleLoader、AssetBundleRef、AssetLoader、AssetLoaderAsync的对象池管理
    /// 记录使用中与未使用中的对象信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResLoaderPool<T> where T : new()
    {
        /// <summary>
        /// inactived object pool, cache LinkedListNode
        /// </summary>
        private Stack<LinkedListNode<T>>    m_InactivedPool;

        /// <summary>
        /// actived object pool
        /// </summary>
        private LinkedList<T>               m_ActivedPool;

        /// <summary>
        /// 获取缓存池对象的Filter
        /// </summary>
        public delegate void onGetCallback(T t);
        private onGetCallback OnGetCallback;

        /// <summary>
        /// 返回缓存对象的Filter
        /// </summary>
        public delegate void onReleaseCallback(T t);
        private onReleaseCallback OnReleaseCallback;

        /// <summary>
        /// 缓存池当前生成对象的总数量
        /// </summary>
        /// <value>The count all.</value>
        public int                          countAll        { get { return countActive + countInactive; } }

        /// <summary>
        /// 缓存池释放出去未回收对象
        /// </summary>
        /// <value>The count active.</value>
        public int                          countActive     { get { return m_ActivedPool?.Count ?? 0; } }

        /// <summary>
        /// 缓存池未使用的对象
        /// </summary>
        /// <value>The count inactive.</value>
        public int                          countInactive   { get { return m_InactivedPool?.Count ?? 0; } }

        public LinkedList<T>                ActivedPool     { get { return m_ActivedPool; } }

        public Stack<LinkedListNode<T>>     InactivedPool   { get { return m_InactivedPool; } }

        public ResLoaderPool(int initSize, onGetCallback actionOnGet = null, onReleaseCallback actionOnRelease = null)
        {
            OnGetCallback = actionOnGet;
            OnReleaseCallback = actionOnRelease;

            m_InactivedPool = new Stack<LinkedListNode<T>>(Mathf.Max(0, initSize));
            m_ActivedPool = new LinkedList<T>();
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <returns>T.</returns>
        public LinkedListNode<T> Get()
        { 
            LinkedListNode<T> node = null;
            if(m_InactivedPool.Count == 0)
            {
                T value = new T();
                node = m_ActivedPool.AddLast(value);
            }
            else
            {
                node = m_InactivedPool.Pop();
                m_ActivedPool.AddLast(node);
            }
            OnGetCallback?.Invoke(node.Value);
            return node;
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="node"></param>
        public void Release(LinkedListNode<T> node)
        {
            if (node == null)
                throw new System.ArgumentNullException();

            m_ActivedPool.Remove(node);
            m_InactivedPool.Push(node);

            OnReleaseCallback?.Invoke(node.Value);
        }
    }
}