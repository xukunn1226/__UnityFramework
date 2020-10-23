using System.Collections;
using System.Collections.Generic;
using System;

namespace Framework.Core
{
    public class SkipList<T> where T : IComparable<T>
    {
        private const int           SKIPLIST_MAXLEVEL   = 32;
        private const float         SKIPLIST_P          = 0.25f;

        private Random              m_Seed;

        private SkipListNode<T>     m_Header;

        private int                 m_CurMaxLevel;
        private SkipListNode<T>[,]  m_Pathway;

        public int                  Count               { get; private set; }

        public SkipList()
        {
            Init();
        }

        private void Init()
        {
            m_Seed = new Random();

            m_Header = new SkipListNode<T>(default(T), SKIPLIST_MAXLEVEL);
            m_Pathway = new SkipListNode<T>[SKIPLIST_MAXLEVEL, 2];

            for(int i = 0; i < SKIPLIST_MAXLEVEL; ++i)
            {
                m_Header.Forwards[i] = null;
            }

            m_CurMaxLevel = 1;
        }

        public bool Contains(T value)
        {
            Traversal(value);

            SkipListNode<T> closestNode = m_Pathway[0, 0];
            if(closestNode == null)
                throw new System.ArgumentNullException("SkipListNode: closestNode == null");

            if(closestNode.Forwards[0] == null || closestNode.Forwards[0].Value.CompareTo(value) != 0)
                return false;
                
            return true;
        }

        public bool Add(T value)
        {
            if(Contains(value))
                return false;
            
            int level = RandomLevel();
            SkipListNode<T> newNode = new SkipListNode<T>(value, level);
            for(int i = 0; i < level; ++i)
            {
                if(i < m_CurMaxLevel)
                {
                    newNode.Forwards[i] = m_Pathway[i, 0].Forwards[i];
                    m_Pathway[i, 0].Forwards[i] = newNode;
                }
                else
                {
                    newNode.Forwards[i] = null;
                    m_Header.Forwards[i] = newNode;
                }
            }
            m_CurMaxLevel = Math.Max(m_CurMaxLevel, level);
            ++Count;
            return true;
        }

        public bool Remove(T value)
        {
            if(!Contains(value))
                return false;

            SkipListNode<T> closestNode = m_Pathway[0, 0];
            SkipListNode<T> removedNode = closestNode.Forwards[0];
            for(int i = 0; i < removedNode.Level; ++i)
            {
                m_Pathway[i, 0] = m_Pathway[i, 1];
            }
            --Count;
            return true;
        }

        private void Traversal(T value)
        {
            SkipListNode<T> cur = m_Header;
            for (int i = m_CurMaxLevel - 1; i >= 0; --i)
            {
                while (cur.Forwards[i] != null && cur.Forwards[i].Value.CompareTo(value) < 0)
                {
                    cur = cur.Forwards[i];
                }
                m_Pathway[i, 0] = cur;
                m_Pathway[i, 1] = cur?.Forwards[i];       // next node
            }
        }
        
        private int RandomLevel()
        {
            // int level = 1;
            // while(m_Seed.NextDouble() < SKIPLIST_P && level <= m_CurMaxLevel && level < SKIPLIST_MAXLEVEL)
            // {
            //     ++level;
            // }
            // return level < SKIPLIST_MAXLEVEL ? level : SKIPLIST_MAXLEVEL;

            switch(Count)
            {
                case 0:
                    return 1;
                case 1:
                    return 4;
                case 2:
                    return 1;
                case 3:
                    return 2;
                case 4:
                    return 1;
                case 5:
                    return 2;
                case 6:
                    return 1;
                case 7:
                    return 1;
                case 8:
                    return 3;
                case 9:
                    return 1;
            }
            return 1;
        }
    }

    internal class SkipListNode<T> where T : IComparable<T>
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
    }
}