using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Core.Tests
{
    [ExecuteInEditMode]
    public class TestQuadTree : MonoBehaviour
    {
        public int width;
        public int height;
        public int maxObjects;
        public int maxDepth;
        public float largeObjectSize;
        public Vector2 smallObjectSize;
        public Vector2 bigObjectSize;

#if UNITY_EDITOR        
        private QuadTree<TestQuadNodeObject> m_QuadTree;

        public QuadTree<TestQuadNodeObject> quadTree { get { return m_QuadTree; } }

        public QuadTree<TestQuadNodeObject>.Node rootNode { get { return m_QuadTree?.rootNode; } }

        public void CreateQuadTree()
        {
            float x = transform.position.x - width * 0.5f;
            float y = transform.position.z - height * 0.5f;
            m_QuadTree = new QuadTree<TestQuadNodeObject>(new Rect(x, y, width, height), maxObjects, maxDepth, largeObjectSize);
        }

        public void InsertSmall()
        {
            Insert(smallObjectSize.x, smallObjectSize.y);
        }

        public void InsertBig()
        {
            Insert(bigObjectSize.x, bigObjectSize.y);
        }

        private void Insert(float min, float max)
        {
            TestQuadNodeObject obj = new TestQuadNodeObject(RandomRect(min, max));
            m_QuadTree.Insert(obj);
        }

        Rect RandomRect(float min, float max)
        {
            float x = transform.position.x + Random.Range(-0.5f * width, 0.5f * width);
            float y = transform.position.z + Random.Range(-0.5f * height, 0.5f * height);
            float w = width * Random.Range(min, max);
            float h = height * Random.Range(min, max);
            return new Rect(x, y, w, h);
        }
#endif        
    }

    public class TestQuadNodeObject : INodeObject
    {        
        private Rect m_Rect;
        
        public ref Rect rect => ref m_Rect;

        public LinkedListNode<INodeObject> quadNode    { get; set; }

        public TestQuadNodeObject(Rect rect)
        {
            m_Rect = rect;
        }
    }
}