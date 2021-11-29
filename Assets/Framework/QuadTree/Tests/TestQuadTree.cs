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
        public int capacity;
        public Vector2 smallObjectSize;
        public Vector2 bigObjectSize;

#if UNITY_EDITOR        
        private SceneManagement<TestQuadNodeObject> m_SceneManagement;

        public QuadTree<TestQuadNodeObject>         quadTree { get { return m_SceneManagement?.quadTree; } }

        public QuadTree<TestQuadNodeObject>.Node    rootNode { get { return quadTree?.rootNode; } }

        public void CreateQuadTree()
        {
            float x = transform.position.x - width * 0.5f;
            float y = transform.position.z - height * 0.5f;
            Rect rect = new Rect(x, y, width, height);
            
            m_SceneManagement = new SceneManagement<TestQuadNodeObject>();
            m_SceneManagement.Init(capacity, rect, maxObjects, maxDepth, largeObjectSize);
        }

        public Rect RandomQueryRect()
        {
            if(rootNode == null)
                return new Rect();

            QuadTree<TestQuadNodeObject>.Node root = rootNode;
            float x = Random.Range(root.rect.xMin - root.rect.width * 0.2f, root.rect.xMax + root.rect.width * 0.2f);
            float y = Random.Range(root.rect.yMin - root.rect.height * 0.2f, root.rect.yMax + root.rect.height * 0.2f);
            float w = Random.Range(root.rect.width * 0.2f, root.rect.width * 1.5f);
            float h = Random.Range(root.rect.height * 0.2f, root.rect.height * 1.5f);
            return new Rect(x, y, w, h);
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
            TestQuadNodeObject obj = new TestQuadNodeObject(RandomObjectRect(min, max));
            // quadTree.Insert(obj);
            m_SceneManagement.Insert(obj);
        }

        Rect RandomObjectRect(float min, float max)
        {
            float x = transform.position.x + Random.Range(-0.5f * width, 0.5f * width);
            float y = transform.position.z + Random.Range(-0.5f * height, 0.5f * height);
            // float x = transform.position.x + Random.Range(0, 0.5f * width);
            // float y = transform.position.z + Random.Range(0, 0.5f * height);
            float w = width * Random.Range(min, max);
            float h = height * Random.Range(min, max);
            return new Rect(x, y, w, h);
        }

        private void InsertSpecial(float min, float max)
        {
            TestQuadNodeObject obj = new TestQuadNodeObject(RandomSpecialRect(min, max));
            // quadTree.Insert(obj);
            m_SceneManagement.Insert(obj);
        }

        Rect RandomSpecialRect(float min, float max)
        {
            // float x = transform.position.x + Random.Range(-0.5f * width, 0.5f * width);
            // float y = transform.position.z + Random.Range(-0.5f * height, 0.5f * height);
            float x = transform.position.x + 0.2f * width;
            float y = transform.position.z + 0.2f * height;
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
        public System.Object userData { get; set; }

        public TestQuadNodeObject(Rect rect)
        {
            m_Rect = rect;
        }
    }
}