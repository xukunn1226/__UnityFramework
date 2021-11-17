using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Framework.Core.Tests
{
    public class TestQuadTree : MonoBehaviour, IQuadTreeDrawable
    {
        private QuadTree<TestQuadNodeObject> m_QuadTree;

        public QuadTree quadTree { get { return m_QuadTree; } }

        // Start is called before the first frame update
        void Start()
        {

        }
        
        void OnGUI()
        {
            
        }
    }

    public class TestQuadNodeObject : INodeObject
    {        
        private Rect m_Rect;
        
        public ref Rect rect => ref m_Rect;

        public LinkedListNode<INodeObject> quadNode    { get; set; }
    }
}