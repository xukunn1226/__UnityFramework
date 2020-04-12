using System.Collections.Generic;

namespace Core
{
    public class LRUQueue<K, V>
    {
        public delegate void DiscardCallback(K key, V value);
        public event DiscardCallback OnDiscard;

        class TNode<T, U>
        {
            public T Key;
            public U Value;

            public TNode(T key, U value)
            {
                Key = key;
                Value = value;
            }
        }

        private LinkedList<TNode<K, V>>                             m_Buffer;           // 记录缓存的实例，最近使用的记录在First
        private Dictionary<K, LinkedListNode<TNode<K, V>>>          m_Dic;              // 记录m_Buffer中所有的LinkedListNode

        public int Capacity { get; private set; }


        public LRUQueue(int capacity)
        {
            m_Buffer = new LinkedList<TNode<K, V>>();
            m_Dic = new Dictionary<K, LinkedListNode<TNode<K, V>>>(capacity);

            Capacity = capacity;
        }

        public void GetOrCreate(K key, V value)
        {
            if (value == null)
                throw new System.ArgumentNullException();

            LinkedListNode<TNode<K, V>> node;
            m_Dic.TryGetValue(key, out node);
            if(node != null)
            { // 已缓存对象
                // 更新至First
                m_Buffer.Remove(node);
                m_Buffer.AddFirst(node);
            }
            else
            { // 没缓存的对象

                // 当缓存队列大于最大存储上限时，删除最久不使用的数据
                if (m_Dic.Count >= Capacity)
                {
                    OnDiscard?.Invoke(m_Buffer.Last.Value.Key, m_Buffer.Last.Value.Value);

                    m_Dic.Remove(m_Buffer.Last.Value.Key);
                    m_Buffer.RemoveLast();
                }

                // add buffer
                LinkedListNode<TNode<K, V>> listNode = m_Buffer.AddFirst(new TNode<K, V>(key, value));
                m_Dic.Add(key, listNode);
            }
        }

        public void Clear()
        {
            foreach(var item in m_Buffer)
            {
                OnDiscard?.Invoke(item.Key, item.Value);
            }
            m_Buffer.Clear();
            m_Dic.Clear();
            OnDiscard = null;
        }

        public void PrintIt()
        {
            string message = "Buffer list: ";
            LinkedList<TNode<K, V>>.Enumerator e = m_Buffer.GetEnumerator();
            while (e.MoveNext())
            {
                message += string.Format($"{e.Current.Value.ToString()}     ");
            }
            UnityEngine.Debug.Log(message);
        }
    }
}