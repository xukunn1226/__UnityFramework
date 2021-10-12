using System.Collections.Generic;

namespace Framework.Cache
{
    /// <summary>
    /// LRU-K algorithm, K means Hit Count
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class LRUKQueue<K, V> where V : class
    {
        public delegate void DiscardCallback(K key, V value);
        public event DiscardCallback OnDiscard;

        class TNode<T, U>
        {
            public T    Key;
            public U    Value;
            public int  Hit;       // 访问次数

            public TNode(T key, U value)
            {
                Key = key;
                Value = value;
                Hit = 1;
            }
        }

        private LinkedList<TNode<K, V>>                             m_History;          // 历史队列
        private LinkedList<TNode<K, V>>                             m_Buffer;           // 缓存队列
        private Dictionary<K, LinkedListNode<TNode<K, V>>>          m_Dic;
        private int                                                 m_Hit;

        public int Capacity         { get; private set; }

        public int Count            { get { return m_Buffer.Count; } }

        public int countAll         { get { return Capacity; } }

        public int countOfUsed      { get { return Count; } }

        // int IPool.countOfUnused     { get; }

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <returns></returns>
        // IPooledObject IPool.Get()
        // {
        //     throw new System.NotImplementedException();
        // }

        /// <summary>
        /// 返回缓存对象
        /// </summary>
        /// <param name="item"></param>
        // void IPool.Return(IPooledObject item)
        // {
        //     throw new System.NotImplementedException();
        // }

        public void Clear()
        {
            foreach (var item in m_Buffer)
            {
                OnDiscard?.Invoke(item.Key, item.Value);
            }

            m_History.Clear();
            m_Buffer.Clear();
            m_Dic.Clear();
        }

        /// <summary>
        /// trim excess data
        /// </summary>
        // void IPool.Trim()
        // {
        //     throw new System.NotImplementedException();
        // }

        public LRUKQueue(int capacity, int k = 2)
        {
            m_History = new LinkedList<TNode<K, V>>();
            m_Buffer = new LinkedList<TNode<K, V>>();
            m_Dic = new Dictionary<K, LinkedListNode<TNode<K, V>>>(capacity * 2);
            m_Hit = System.Math.Max(1, k);

            Capacity = capacity;
        }

        public void Cache(K key, V value)
        {
            LinkedListNode<TNode<K, V>> node;
            m_Dic.TryGetValue(key, out node);
            if (node != null)
            { // 已在历史队列或缓存队列中
                
                if (InHistory(node))
                {
                    node.Value.Hit += 1;
                    if (!InHistory(node))
                    { // 从历史队列删除，移动缓存队列
                        MoveToBuffer(node);
                    }
                }
                else
                { // 已在缓存队列，重新排列
                    m_Buffer.Remove(node);
                    m_Buffer.AddBefore(m_Buffer.First, node);
                }
            }
            else
            {
                 MoveToHistory(key, value);
            }
        }

        private bool InHistory(LinkedListNode<TNode<K, V>> node)
        {
            if (node == null || node.Value.Hit < m_Hit)
                return true;
            return false;
        }

        // 第一次访问，进入历史队列
        private void MoveToHistory(K key, V value)
        {
            if(m_History.Count >= Capacity)
            {
                OnDiscard?.Invoke(m_History.Last.Value.Key, m_History.Last.Value.Value);

                m_Dic.Remove(m_History.Last.Value.Key);
                m_History.RemoveLast();
            }
            m_History.AddFirst(new TNode<K, V>(key, value));
            m_Dic.Add(key, m_History.First);
        }

        // 访问次数达到后，从历史队列删除，移动到缓存队列
        private void MoveToBuffer(LinkedListNode<TNode<K, V>> node)
        {
            m_History.Remove(node);

            if (m_Buffer.Count >= Capacity)
            {
                OnDiscard?.Invoke(m_Buffer.Last.Value.Key, m_Buffer.Last.Value.Value);

                m_Dic.Remove(m_Buffer.Last.Value.Key);
                m_Buffer.RemoveLast();
            }
            m_Buffer.AddFirst(node);
        }

        public void PrintIt()
        {
            string message = "History list: ";
            LinkedList<TNode<K, V>> .Enumerator e = m_History.GetEnumerator();
            while(e.MoveNext())
            {
                message += string.Format($"{e.Current.Value.ToString()}    ");
            }
            UnityEngine.Debug.Log(message);

            message = "Buffer list: ";
            e = m_Buffer.GetEnumerator();
            while(e.MoveNext())
            {
                message += string.Format($"{e.Current.Value.ToString()}     ");
            }
            UnityEngine.Debug.Log(message);
        }
    }
}