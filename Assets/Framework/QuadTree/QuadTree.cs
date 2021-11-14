using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public interface INodeObject
    {
        Rect rect { get; }
        LinkedListNode<INodeObject> quadNode { get; set; }
    }

    public class QuadTree<T> where T : INodeObject, new()
    {
        /* 一个矩形区域的象限划分：
          
        UL(1)     |    UR(0)
        ----------|-----------
        LL(2)     |    LR(3)
        */
        public enum Quadrant
        {
            UR,
            UL,
            LL,
            LR,
            Max,
        }

        public class Node
        {
            public Rect             rect;                       // 节点所代表的矩形区域
            public int              depth;                      // 节点所在深度，根节点深度为0
            public LinkedList<T>    objects;                    // 归属此节点的对象
            public Node             parent;
            public Node[]           children    = new Node[(int)Quadrant.Max];     // Node[0]: upper right; Node[1]: upper left; Node[2]: lower left; Node[3]: lower right
        }

        private Node        m_Root;
        private int         m_MaxDepth;

        // 指定范围内创建指定层次的四叉树
        public QuadTree(Rect rect, int maxDepth)
        {
            m_MaxDepth = Mathf.Max(0, maxDepth);

            m_Root = new Node();
            m_Root.depth = 0;
            m_Root.rect = rect;
        }

        // 包含区域rect的最小节点
        public Node Search(Node node, Rect queryRect)
        {
            int ret = Contains(node, queryRect);
            if(ret == -1)
                return null;

            if(ret == 0)
                return node;

            Node child = node.children[ret - 1];
            return child != null ? Search(child, queryRect) : node;
        }

        public Node Search(Rect queryRect)
        {
            return Search(m_Root, queryRect);
        }

        public void Traverse(Node node, ref List<T> objs)
        {
            if(node == null)
                return;
            
            if(node.objects != null)
            {
                LinkedList<T>.Enumerator it = node.objects.GetEnumerator();
                while(it.MoveNext())
                {
                    objs.Add(it.Current);
                }
                it.Dispose();
            }

            for(int i = 0; i < 4; ++i)
            {
                Traverse(node.children[i], ref objs);
            }
        }

        public void Traverse(ref List<T> objs)
        {
            Traverse(m_Root, ref objs);
        }

        public void Traverse(Node node, Rect queryRect, ref List<T> objs)
        {
            if(node == null)
                return;

            if(node.objects != null)
            {
                LinkedList<T>.Enumerator it = node.objects.GetEnumerator();
                while(it.MoveNext())
                {
                    if(InRegion(queryRect, it.Current.rect) || queryRect.Overlaps(it.Current.rect))
                        objs.Add(it.Current);
                }
                it.Dispose();
            }

            for(int i = 0; i < 4; ++i)
            {
                if(node.children[i] == null)
                    continue;

                if(InRegion(queryRect, node.children[i].rect) || queryRect.Overlaps(node.children[i].rect))
                {
                    Traverse(node.children[i], queryRect, ref objs);
                }
            }
        }

        public void Traverse(Rect queryRect, ref List<T> objs)
        {
            Traverse(m_Root, queryRect, ref objs);
        }

        // 插入一个对象，返回对象所属节点，若插入失败则返回null
        public Node Insert(T obj)
        {
            return Insert(m_Root, obj);
        }

        public Node Insert(Node node, T obj)
        {
            int ret = Contains(node, obj.rect);
            if(ret == -1)
                return null;        // 超出范围

            if(ret == 0)
            { // 被节点自身包含
                AddObjectToNode(node, obj);
                return node;
            }

            if(node.depth + 1 > m_MaxDepth)
            { // 超出最大深度，不继续分裂，物体放置于当前节点
                AddObjectToNode(node, obj);
                return node;
            }

            Node child = node.children[ret - 1];
            if(child == null)
            { // create child node                
                child = new Node();
                child.parent = node;
                child.depth = node.depth + 1;
                child.rect = GetSubRect(ref node.rect, (Quadrant)(ret - 1));
                child.objects = new LinkedList<T>();
                AddObjectToNode(child, obj);

                node.children[ret - 1] = child;
                return child;
            }
            return Insert(child, obj);
        }
        
        private void AddObjectToNode(Node node, T obj)
        {
            LinkedListNode<T> quadNode = node.objects.AddLast(obj);
            obj.quadNode = quadNode as LinkedListNode<INodeObject>;
        }

        public void Clear()
        {
            Clear(m_Root);
        }

        private void Clear(Node node)
        {
            if(node == null)
                return;

            node.objects.Clear();

            if(node.children[0] != null)
                Clear(node.children[0]);
            if(node.children[1] != null)
                Clear(node.children[1]);
            if(node.children[2] != null)
                Clear(node.children[2]);
            if(node.children[3] != null)
                Clear(node.children[3]);
            node = null;
        }

        public void Remove(T obj)
        {
            UnityEngine.Debug.Assert(obj.quadNode != null);

            Node node = Search(obj.rect);
            if(node != null)
            {
                node.objects.Remove(obj.quadNode as LinkedListNode<T>);
            }
        }
        
        // 区域与节点node的包含关系
        // -1: 对象完全不包含在节点内
        //  0: 对象与节点交叠，与区域交叠和与四个子区域交叠都判定为交叠
        //  1: 被完全包含与UR区域内
        //  2: 被完全包含与UL区域内
        //  3: 被完全包含与LL区域内
        //  4: 被完全包含与LR区域内
        private int Contains(Node node, Rect rect)
        {
            if(node == null)
                throw new System.ArgumentNullException("node");

            if(OutOfRegion(node.rect, rect)) return -1;
            if(InRegion(GetSubRect(ref node.rect, Quadrant.UR), rect)) return 1;
            if(InRegion(GetSubRect(ref node.rect, Quadrant.UL), rect)) return 2;
            if(InRegion(GetSubRect(ref node.rect, Quadrant.LL), rect)) return 3;
            if(InRegion(GetSubRect(ref node.rect, Quadrant.LR), rect)) return 4;
            return 0;
        }

        private Rect GetSubRect(ref Rect rect, Quadrant qr)
        {
            switch(qr)
            {
                case Quadrant.UR:
                    return new Rect(rect.center,                                                        rect.size * 0.5f);
                case Quadrant.UL:
                    return new Rect(rect.center + new Vector2(rect.width * -0.5f,                   0), rect.size * 0.5f);
                case Quadrant.LL:
                    return new Rect(rect.center + new Vector2(rect.width * -0.5f, rect.height * -0.5f), rect.size * 0.5f);
                case Quadrant.LR:
                    return new Rect(rect.center + new Vector2(rect.width *  0.5f, rect.height * -0.5f), rect.size * 0.5f);
            }
            return rect;
        }

        private bool InRegion(Rect nodeRect, Rect objRect)
        {
            return objRect.xMin >= nodeRect.xMin && objRect.xMax <= nodeRect.xMax && objRect.yMin >= nodeRect.yMin && objRect.yMax <= nodeRect.yMax;
        }

        private bool OutOfRegion(Rect nodeRect, Rect objRect)
        {
            return objRect.xMax < nodeRect.xMin || objRect.xMin > nodeRect.xMax || objRect.yMax < nodeRect.yMin || objRect.yMin > nodeRect.yMax;
        }
    }
}