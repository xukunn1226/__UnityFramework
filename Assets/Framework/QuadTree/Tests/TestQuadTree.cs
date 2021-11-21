using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Framework.Core.Tests
{
    public class TestQuadTree : MonoBehaviour, IQuadTreeDrawable
    {
        public int width;
        public int height;
        public int maxObject;
        public int maxDepth;

        private QuadTree<TestQuadNodeObject> m_QuadTree;

        public QuadTree quadTree { get { return m_QuadTree; } }

        // Start is called before the first frame update
        void Start()
        {
            m_QuadTree = new QuadTree<TestQuadNodeObject>(new Rect(new Vector2(transform.position.x, transform.position.z), new Vector2(width, height)), maxObject, maxDepth);
        }
        
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                bool ret = m_QuadTree.Insert(new TestQuadNodeObject(RandomRect()));
                Debug.Log($"Insert: {ret}");
            }            
        }

        Rect RandomRect()
        {
            float x = Random.Range(transform.position.x - 0.6f * width, transform.position.x + 0.6f * width);
            float y = Random.Range(transform.position.y - 0.6f * height, transform.position.y + 0.6f * height);
            float w = Random.Range(0.01f, 0.1f) * width;
            float h = Random.Range(0.01f, 0.1f) * height;
            return new Rect(x, y, w, h);
        }
    }

    public class TestQuadNodeObject : INodeObject
    {        
        private Rect m_Rect;
        
        public ref Rect rect => ref m_Rect;

        public LinkedListNode<INodeObject> quadNode    { get; set; }

        public TestQuadNodeObject() {}

        public TestQuadNodeObject(Rect rect)
        {
            m_Rect = rect;
        }
    }
}