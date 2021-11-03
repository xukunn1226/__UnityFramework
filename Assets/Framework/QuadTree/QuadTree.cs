using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public interface INodeObject
    {
        Rect rect { get; }
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
        private int         m_Depth;

        // 指定范围内创建指定层次的四叉树
        public QuadTree(Rect rect, int depth)
        {
            m_Depth = depth;

            m_Root = new Node();
            m_Root.depth = 0;
            m_Root.rect = rect;
        }

        // 指定区域包含在的节点
        public Node Find(T obj)
        {
            return null;
        }

        // 插入一个对象，对象区域超出则返回null
        public Node Insert(T obj)
        {
            if(m_Root.rect.Overlaps(obj.rect))
                return null;        // 对象与根节点区域交叠，判定超出范围
            return InternalInsert(m_Root, obj);
        }

        private Node InternalInsert(Node node, T obj)
        {
            Rect lr = new Rect(node.rect.position, new Vector2(node.rect.width, 0.1f));
            Rect tb = new Rect(node.rect.position, new Vector2(node.rect.height, 0.1f));
            if(obj.rect.Overlaps(lr) || obj.rect.Overlaps(tb))
            { // 对象与此节点交叠，归属此节点
                if(node.objects == null)
                    node.objects = new LinkedList<T>();
                node.objects.AddLast(obj);
                return node;
            }

            Rect subRect_UR = new Rect(node.rect.position + node.rect.size * 0.5f, node.rect.size * 0.5f);
            if(!subRect_UR.Overlaps(obj.rect))
            {

            }
            return null;
        }

        private void QuadCreateBranch(Node node, int depth, Rect rect)
        {
            if(depth > 0)
            {
                node = new Node();
                node.rect = rect;

                Rect subRect_UR = new Rect(node.rect.position + new Vector2(node.rect.width *  0.25f, node.rect.height *  0.25f), node.rect.size * 0.25f);
                Rect subRect_UL = new Rect(node.rect.position + new Vector2(node.rect.width * -0.25f, node.rect.height *  0.25f), node.rect.size * 0.25f);
                Rect subRect_LL = new Rect(node.rect.position + new Vector2(node.rect.width * -0.25f, node.rect.height * -0.25f), node.rect.size * 0.25f);
                Rect subRect_LR = new Rect(node.rect.position + new Vector2(node.rect.width *  0.25f, node.rect.height * -0.25f), node.rect.size * 0.25f);

                QuadCreateBranch(node.children[(int)Quadrant.UR], depth - 1, subRect_UR);
                QuadCreateBranch(node.children[(int)Quadrant.UL], depth - 1, subRect_UL);
                QuadCreateBranch(node.children[(int)Quadrant.LL], depth - 1, subRect_LL);
                QuadCreateBranch(node.children[(int)Quadrant.LR], depth - 1, subRect_LR);
            }
        }

        public bool Remove(T obj)
        {
            return true;
        }
    }
}