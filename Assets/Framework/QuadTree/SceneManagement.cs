using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public struct SceneStateEvent
    {
        public INodeObject  sceneObject         { get; internal set; }
        public bool         hasBecomeVisible    { get; internal set; }
        public bool         hasBecomeInvisible  { get; internal set; }
    }

    public class SceneManagement<T> where T : INodeObject
    {
        public delegate void StateChanged(SceneStateEvent evt);
        public StateChanged onStateChanged { get; set; }

        public int              width;
        public int              height;
        public int              maxObjects;
        public int              maxDepth;
        public float            largeObjectSize;
        public int              capacity;

        #if UNITY_EDITOR
        public 
        #endif
        class SceneObject
        {
            public int          Index;
            public T            Object;
            public bool         isVisible;
            public bool         wasVisible;
        }
        private SceneObject[]   m_SceneObjects;             // 所有管理的场景物体列表，保证容量足够大
        private int             m_UsedSceneObjectCount;     // 有效的场景物体数量
        private QuadTree<T>     m_QuadTree;
        private BitSet          m_CurVisibleData;           // 当前帧可见物体列表
        private BitSet          m_PrevVisibleData;          // 上一帧可见物体列表
        private BitSet          m_ChangedVisibleData;       // 发生变化的数据
        private bool            m_isInit;
        private List<T>         m_QueryObjects  = new List<T>(128);

        #if UNITY_EDITOR
        public QuadTree<T>      quadTree        { get { return m_QuadTree; } }
        public List<T>          queryObjects    { get { return m_QueryObjects; } }
        #endif

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

            m_SceneObjects          = new SceneObject[capacity];
            m_CurVisibleData        = new BitSet(capacity);
            m_PrevVisibleData       = new BitSet(capacity);
            m_ChangedVisibleData    = new BitSet(capacity);
            m_QuadTree              = new QuadTree<T>(totalRect, maxObjects, maxDepth, largeObjectSize);
        }

        public void Insert(T obj)
        {
            if(!m_isInit)
                throw new System.InvalidOperationException($"Insert: m_isInit is {m_isInit}");

            if(m_UsedSceneObjectCount >= m_SceneObjects.Length)
                throw new System.ArgumentOutOfRangeException($"m_UsedSceneObjectCount[{m_UsedSceneObjectCount}] >= m_SceneObjects.Length[{m_SceneObjects.Length}]");
            Debug.Assert(m_SceneObjects[m_UsedSceneObjectCount] == null);

            m_QuadTree.Insert(obj);

            SceneObject so = new SceneObject() { Index = m_UsedSceneObjectCount, Object = obj };
            obj.userData = so;
            m_SceneObjects[m_UsedSceneObjectCount++] = so;
        }

        public void Query(ref Rect queryRect)
        {
            // Query current objects by query rect
            m_QueryObjects.Clear();
            m_QuadTree.Query(ref queryRect, ref m_QueryObjects);

            m_CurVisibleData.SetAll(false);
            for(int i = 0; i < m_QueryObjects.Count; ++i)
            {
                m_CurVisibleData.Set(((SceneObject)m_QueryObjects[i].userData).Index, true);
            }

            m_CurVisibleData.CopyTo(m_ChangedVisibleData);
            m_ChangedVisibleData.Xor(m_PrevVisibleData);

            IEnumerator e = m_ChangedVisibleData.GetFastEnumerator(m_UsedSceneObjectCount);
            while(e.MoveNext())
            {
                int index = (int)e.Current;
                m_SceneObjects[index].wasVisible = m_SceneObjects[index].isVisible;
                m_SceneObjects[index].isVisible = !m_SceneObjects[index].isVisible;

                onStateChanged?.Invoke(new SceneStateEvent() {sceneObject = m_SceneObjects[index].Object, 
                                                              hasBecomeVisible = !m_SceneObjects[index].wasVisible && m_SceneObjects[index].isVisible, 
                                                              hasBecomeInvisible = m_SceneObjects[index].wasVisible && !m_SceneObjects[index].isVisible});
            }

            m_CurVisibleData.CopyTo(m_PrevVisibleData);
        }
    }
}