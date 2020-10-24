using System.Text;
using System;

namespace Framework.Core
{
    public class SkipList<T> where T : IComparable<T>
    {
        static private readonly int     SKIPLIST_MAXLEVEL   = 32;
        static private readonly float   SKIPLIST_P          = 0.25f;

        private Random                  m_Seed;
        private SkipListNode<T>         m_Header;
        private int                     m_CurMaxLevel;
        private SkipListNode<T>[,]      m_Pathway;
        public int                      Count               { get; private set; }

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
                m_Pathway[i, 0].Forwards[i] = removedNode.Forwards[i];
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
            int level = 1;
            while(m_Seed.NextDouble() < SKIPLIST_P && level <= m_CurMaxLevel && level < SKIPLIST_MAXLEVEL)
            {
                ++level;
            }
            return level < SKIPLIST_MAXLEVEL ? level : SKIPLIST_MAXLEVEL;
        }

        public void PrintIt()
        {
            UnityEngine.Debug.Log($"Print SkipList: Count[{Count}]  MaxLevel[{m_CurMaxLevel}]");
            SkipListNode<T> cur = m_Header;
            while(cur != null)
            {
                cur.PrintIt();
                cur = cur.Forwards[0];
            }            
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

        public void PrintIt()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>Value:</b> {Value}");
            sb.AppendLine($"<b>Level:</b> {Level}");
            for(int i = 0; i < Level; ++i)
            {
                string info = Forwards[i] != null ? Forwards[i].Value.ToString() : "NIL";
                sb.AppendLine($"    <b>Forward[{i}]</b>  {info}");
            }
            UnityEngine.Debug.Log(sb.ToString());
        }
    }
}