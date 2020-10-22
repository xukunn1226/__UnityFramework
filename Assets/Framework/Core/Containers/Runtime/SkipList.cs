using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
//     int zslRandomLevel(void) {
//     int level = 1;
//     while ((random()&0xFFFF) < (ZSKIPLIST_P * 0xFFFF))
//         level += 1;
//     return (level<ZSKIPLIST_MAXLEVEL) ? level : ZSKIPLIST_MAXLEVEL;
// }
    public class SkipList<T> where T : IComparable<T>
    {
        private const int           SKIPLIST_MAXLEVEL   = 32;
        private const float         SKIPLIST_P          = 0.25f;

        private Random              m_Seed;

        private SkipListNode<T>     m_Header;
        private SkipListNode<T>     m_Nil;
        private int[]               m_Sizes;            // 每层链表的大小

        private int                 m_CurMaxLevel;

        public SkipList()
        {
            Init();
        }

        private void Init()
        {
            m_Seed = new Random();

            m_Header = new SkipListNode<T>(default(T), SKIPLIST_MAXLEVEL);
            m_Nil = new SkipListNode<T>(default(T), SKIPLIST_MAXLEVEL);

            for(int i = 0; i < SKIPLIST_MAXLEVEL; ++i)
            {
                m_Header.Forwards[i] = m_Nil;
            }

            m_CurMaxLevel = 1;
            m_Sizes = new int[SKIPLIST_MAXLEVEL];
        }

        public bool Find(T value)
        {
            return true;
        }

        public bool Add(T value)
        {
            return true;
        }

        public bool Remove(T value)
        {
            return true;
        }

        private int RandomLevel()
        {
            int level = 1;
            while(m_Seed.NextDouble() < SKIPLIST_P && level <= m_CurMaxLevel && level < SKIPLIST_MAXLEVEL)
            {
                ++level;
            }
            return level < SKIPLIST_MAXLEVEL ? level : SKIPLIST_MAXLEVEL;
        }
    }

    internal class SkipListNode<T> : IComparable<SkipListNode<T>> where T : IComparable<T>
    {
        public T                    Value       { get; private set; }
        public SkipListNode<T>[]    Forwards    { get; private set; }
        public int                  Level       { get { return Forwards.Length; } }

        protected SkipListNode() {}

        public SkipListNode(T value, int level)
        {
            if(level < 1)
                throw new ArgumentOutOfRangeException($"invalid value for level[{level}]");

            Value = value;
            Forwards = new SkipListNode<T>[level];
        }

        public int CompareTo(SkipListNode<T> other)
        {
            if(other == null)
                return -1;
            return Value.CompareTo(other.Value);
        }
    }
}