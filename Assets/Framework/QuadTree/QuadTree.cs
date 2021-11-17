using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Framework.Core
{
    public interface INodeObject
    {
        ref Rect                    rect        { get; }
        LinkedListNode<INodeObject> quadNode    { get; set; }
    }

    public class QuadTree
    {}

    public class QuadTree<T> : QuadTree where T : INodeObject, new()
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
            public LinkedList<T>    objects;                    // 归属此节点的物体
            public Node             parent;
            public Node[]           children;                   // Node[0]: upper right; Node[1]: upper left; Node[2]: lower left; Node[3]: lower right
        }

        private Node                m_Root;                     // 根节点
        private int                 m_MaxDepth;                 // 最大深度
        private int                 m_MaxObjects;               // 每个区域最多容纳的物体数量
        private const float         LARGE_OBJECT_SIZE = 5;      // 判定大物件的尺寸阈值(只有大物体可以包含于上一层节点，能有更大概率被查询到)
                                                                // 为了减少物体与区域边界相交时不好判定归属的问题，设计规则是小物件仅拿坐标与区域bound
                                                                // 做测试，大物件才拿bound与区域bound做相交性测试

        // 指定范围内创建指定层次的四叉树
        public QuadTree(Rect rect, int maxObject, int maxDepth)
        {
            m_MaxDepth = Mathf.Max(0, maxDepth);
            m_MaxObjects = Mathf.Max(4, maxObject);

            m_Root = new Node();
            m_Root.depth = 0;
            m_Root.rect = rect;
        }

        // 包含区域queryRect的最小节点
        public Node Search(Node node, Rect queryRect)
        {
            if(node == null)
                return null;

            int ret = GetQuadrant(ref node.rect, ref queryRect);
            if(ret == -1)
                return null;

            if(ret == 0)
                return node;

            return Search(node.children[ret - 1], queryRect);
        }

        public Node Search(Rect queryRect)
        {
            return Search(m_Root, queryRect);
        }

        public void Query(ref List<T> objs)
        {
            Query(m_Root, ref objs);
        }

        private void Query(Node node, ref List<T> objs)
        {
            if(node == null)
                return;

            objs.AddRange(node.objects);

            if(node.children == null)
                return;

            for(int i = 0; i < 4; ++i)
            {
                Query(node.children[i], ref objs);
            }
        }

        public void Query(Rect queryRect, ref List<T> objs)
        {
            Query(m_Root, queryRect, ref objs);
        }

        private void Query(Node node, Rect queryRect, ref List<T> objs)
        {
            if(node == null)
                return;

            if(node.objects != null)
            {
                LinkedList<T>.Enumerator it = node.objects.GetEnumerator();
                while(it.MoveNext())
                {
                    if(queryRect.Overlaps(it.Current.rect))
                        objs.Add(it.Current);
                }
                it.Dispose();
            }

            if(node.children == null)
                return;

            for(int i = 0; i < 4; ++i)
            {
                // 子区域包含查询区域，则跳过其他子区域
                if(InRegion(node.children[i].rect, ref queryRect))
                {
                    Query(node.children[i], queryRect, ref objs);
                    break;
                }

                // 查询区域包含子区域，则计入子区域内所有对象
                if(InRegion(queryRect, ref node.children[i].rect))
                {
                    GetAllObjects(node.children[i], ref objs);
                    continue;
                }

                if(queryRect.Overlaps(node.children[i].rect))
                {
                    Query(node.children[i], queryRect, ref objs);
                }
            }
        }

        // 获取此节点及所有子节点的对象
        private void GetAllObjects(Node node, ref List<T> objs)
        {
            if(node == null)
                return;

            objs.AddRange(node.objects);
            if(node.children != null)
            {
                for (int i = 0; i < 4; ++i)
                {
                    GetAllObjects(node.children[i], ref objs);
                }
            }
        }

        public void GetAllObjects(ref List<T> objs)
        {
            GetAllObjects(m_Root, ref objs);
        }

        public bool Insert(T obj)
        {
            return Insert(m_Root, obj);
        }

        private bool Insert(Node node, T obj)
        {
            int ret = GetQuadrant(ref node.rect, ref obj.rect);
            if(ret == -1)
                return false;        // 超出范围
            
            if(ret == 0)
            { // 被节点自身包含，只有大物体可能执行这里
                AddObjectToNode(node, obj);
                return true;
            }

            // 有子区域，优先放入
            if(node.children != null)
            {
                Insert(node.children[ret - 1], obj);
                return true;
            }

            AddObjectToNode(node, obj);

            if(node.children == null && node.objects.Count > m_MaxObjects && node.depth < m_MaxDepth)
            {
                Split(node);

                // 把当前节点包含的物体插入子节点
                List<T> ghost = node.objects.ToList();
                node.objects.Clear();
                foreach(var o in ghost)
                {
                    Insert(node, o);
                }
            }
            return true;
        }

        private void Split(Node node)
        {
            Debug.Assert(node.children == null);
            node.children = new Node[(int)Quadrant.Max];
            for(int i = 0; i < (int)Quadrant.Max; ++i)
            {
                Node child = new Node();
                child.parent = node;
                child.depth = node.depth + 1;
                child.rect = GetSubRect(ref node.rect, (Quadrant)i);
                child.objects = new LinkedList<T>();
                node.children[i] = child;
            }
        }
        
        private void AddObjectToNode(Node node, T obj)
        {
            LinkedListNode<T> quadNode = node.objects.AddLast(obj);
            obj.quadNode = quadNode as LinkedListNode<INodeObject>;
        }

        // public void Clear()
        // {
        //     Clear(m_Root);
        // }

        // private void Clear(Node node)
        // {
        //     if(node == null)
        //         return;

        //     node.objects.Clear();

        //     if(node.children[0] != null)
        //         Clear(node.children[0]);
        //     if(node.children[1] != null)
        //         Clear(node.children[1]);
        //     if(node.children[2] != null)
        //         Clear(node.children[2]);
        //     if(node.children[3] != null)
        //         Clear(node.children[3]);
        //     node = null;
        // }

        // public void Remove(T obj)
        // {
        //     UnityEngine.Debug.Assert(obj.quadNode != null);

        //     Node node = Search(obj.rect);
        //     if(node != null)
        //     {
        //         node.objects.Remove(obj.quadNode as LinkedListNode<T>);
        //     }
        // }

        public int Count(Node node)
        {
            if(node == null)
                return 0;

            int count = node.objects.Count;
            if(node.children != null)
            {
                for (int i = 0; i < 4; ++i)
                {
                    count += Count(node.children[i]);
                }
            }
            return count;
        }

        public int Count()
        {
            return Count(m_Root);
        }
        
        // 物体与节点node的包含关系
        // -1: 物体完全不包含在节点内
        //  0: 对象与节点交叠，与区域交叠和与四个子区域交叠都判定为交叠
        //  1: 被完全包含与UR区域内
        //  2: 被完全包含与UL区域内
        //  3: 被完全包含与LL区域内
        //  4: 被完全包含与LR区域内
        private int GetQuadrant(ref Rect nodeRect, ref Rect objRect)
        {
            if(!IsBigObject(ref objRect))
            { // 小物体仅判定点与区域的相关性
                if(!nodeRect.Contains(objRect.center)) return -1;
                if(GetSubRect(ref nodeRect, Quadrant.UR).Contains(objRect.center)) return 1;
                if(GetSubRect(ref nodeRect, Quadrant.UL).Contains(objRect.center)) return 2;
                if(GetSubRect(ref nodeRect, Quadrant.LL).Contains(objRect.center)) return 3;
                if(GetSubRect(ref nodeRect, Quadrant.LR).Contains(objRect.center)) return 4;

                Debug.Assert(false);        // 小物体不判定与本节点的相交性，理论上不会执行到这里
                return 0;
            }

            if(OutOfRegion(ref nodeRect, ref objRect)) return -1;
            if(InRegion(GetSubRect(ref nodeRect, Quadrant.UR), ref objRect)) return 1;
            if(InRegion(GetSubRect(ref nodeRect, Quadrant.UL), ref objRect)) return 2;
            if(InRegion(GetSubRect(ref nodeRect, Quadrant.LL), ref objRect)) return 3;
            if(InRegion(GetSubRect(ref nodeRect, Quadrant.LR), ref objRect)) return 4;
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

        // 区域left是否包含区域right
        private bool InRegion(Rect left, ref Rect right)
        {
            return right.xMin >= left.xMin && right.xMax <= left.xMax && right.yMin >= left.yMin && right.yMax <= left.yMax;
        }

        // 区域left是否在区域right之外
        private bool OutOfRegion(ref Rect left, ref Rect right)
        {
            return right.xMax < left.xMin || right.xMin > left.xMax || right.yMax < left.yMin || right.yMin > left.yMax;
        }

        private bool IsBigObject(ref Rect rect)
        {
            return Mathf.Max(rect.width, rect.height) > LARGE_OBJECT_SIZE;
        }
    }
}