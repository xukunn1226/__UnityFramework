using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    // public struct SceneStateEvent
    // {
    //     public System.Object userData { get; }
    //     public bool isVisible { get; }
    //     public bool wasVisible { get; }
    // }

    public class SceneManagement : MonoBehaviour
    {
        // public delegate void StateChanged();

        public int      width;
        public int      height;
        public int      maxObjects;
        public int      maxDepth;
        public float    largeObjectSize;
        public int      capacity;
        public Rect     queryRect;

        class SceneObject
        {
            public int          Index;
            public INodeObject  Object;
            public bool         isVisible;
            public bool         wasVisible;
        }
        private SceneObject[]           m_SceneObjects;
        private int                     m_UsedSceneObjectCount;
        private QuadTree<INodeObject>   m_QuadTree;
        private BitSet                  m_CurFrameData;
        private BitSet                  m_PrevFrameData;
        private BitSet                  m_ResultFrameData;      // 发生变化的数据
        private bool                    m_isInit;
        private List<INodeObject>       m_QueryObjects  = new List<INodeObject>(128);

        void Awake()
        {
            float x = transform.position.x - width * 0.5f;
            float y = transform.position.z - height * 0.5f;
            Rect rect = new Rect(x, y, width, height);

            Init(capacity, rect, maxObjects, maxDepth, largeObjectSize);
        }

        public void Init(int capacity, Rect totalRect, int maxObjects, int maxDepth, float largeObjectSize)
        {
            if(capacity < 1)
                throw new System.ArgumentOutOfRangeException($"capacity: {capacity} < 1");
                
            m_isInit                = true;
            this.capacity           = capacity;
            this.width              = (int)totalRect.width;
            this.height             = (int)totalRect.height;
            this.maxObjects         = maxObjects;
            this.maxDepth           = maxDepth;
            this.largeObjectSize    = largeObjectSize;
            m_UsedSceneObjectCount  = 0;

            m_SceneObjects      = new SceneObject[capacity];
            m_CurFrameData      = new BitSet(capacity);
            m_PrevFrameData     = new BitSet(capacity);
            m_ResultFrameData   = new BitSet(capacity);
            m_QuadTree          = new QuadTree<INodeObject>(totalRect, maxObjects, maxDepth, largeObjectSize);
        }

        public void Insert(INodeObject obj)
        {
            if(!m_isInit)
                throw new System.InvalidOperationException($"Insert: m_isInit is {m_isInit}");

            if(m_UsedSceneObjectCount >= m_SceneObjects.Length)
                throw new System.ArgumentOutOfRangeException($"m_UsedSceneObjectCount[{m_UsedSceneObjectCount}] >= m_SceneObjects.Length[{m_SceneObjects.Length}]");
            Debug.Assert(m_SceneObjects[m_UsedSceneObjectCount] == null);

            m_QuadTree.Insert(obj);

            SceneObject so = new SceneObject();
            so.Index = m_UsedSceneObjectCount;
            so.Object = obj;
            obj.userData = so;
            m_SceneObjects[m_UsedSceneObjectCount++] = so;

            // bool isVisible = m_QuadTree.IsVisible(ref so.Object.rect, ref queryRect);
            // m_CurFrameData.Set(so.Index, isVisible);

            // InitViewHandler(so, isVisible);
        }

        // private void InitViewHandler(SceneObject so, bool isVisible)
        // {

        // }

        public void Update()
        {
            // Query current objects by query rect
            m_QueryObjects.Clear();
            m_QuadTree.Query(queryRect, ref m_QueryObjects);

            m_CurFrameData.SetAll(false);
            for(int i = 0; i < m_QueryObjects.Count; ++i)
            {
                m_CurFrameData.Set(((SceneObject)m_QueryObjects[i].userData).Index, true);
            }

            m_CurFrameData.CopyTo(m_ResultFrameData);
            m_ResultFrameData.Xor(m_PrevFrameData);

            IEnumerator e = m_ResultFrameData.GetFastEnumerator(m_UsedSceneObjectCount);
            while(e.MoveNext())
            {
                int index = (int)e.Current;
                m_SceneObjects[index].wasVisible = m_SceneObjects[index].isVisible;
                m_SceneObjects[index].isVisible = !m_SceneObjects[index].isVisible;
            }

            m_CurFrameData.CopyTo(m_PrevFrameData);
        }
    }
}