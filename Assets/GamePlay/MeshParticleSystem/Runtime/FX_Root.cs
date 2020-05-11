﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

//[MenuItem("Tools/Apply PrefabInstance")]
//static private void TestPrefab()
//{
//    GameObject parent = PrefabUtility.GetOutermostPrefabInstanceRoot(Selection.activeObject);
//    if (parent != null)
//        Selection.activeObject = parent;

//    //PrefabUtility.ApplyPrefabInstance(Selection.activeGameObject, InteractionMode.UserAction);
//}
namespace MeshParticleSystem
{
    [ExecuteInEditMode]
    public class FX_Root : MonoPooledObjectBase, IFX_Root
    {
        public enum RecyclingType
        {
            Destroy,                // 直接销毁
            ObjectPool,             // 放置到对象池
            LRUPool,                // 放置到LRU池
            DontHandle,             // 不处理，由owner控制
        }
        [SerializeField]
        private RecyclingType m_RecyclingType = RecyclingType.Destroy;

        public float lifeTime { get { return m_LifeTime; } }

        [SerializeField]
        private float m_LifeTime;

        private float m_CachedLifeTime;

        public float ElapsedLifeTime { get; private set; }

        private FX_Component[] m_Components;
        private bool m_bInit;

        private FX_Component[] Components
        {
            get
            {
                if (!m_bInit)
                {
                    m_Components = GetComponentsInChildren<FX_Component>(true);
                    m_bInit = true;
                }
                return m_Components;
            }
        }

        void Awake()
        {
            m_CachedLifeTime = m_LifeTime;
            ElapsedLifeTime = 0;
        }

        void Update()
        {
            if (lifeTime <= 0) { return; }

            ElapsedLifeTime += Time.deltaTime;

            if (ElapsedLifeTime > lifeTime)
            {
                OnEffectEnd();
            }
        }

        private void OnEffectEnd()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)     // 方便美术编辑，编辑器非运行模式下不处理回收
                return;
#endif
            switch (m_RecyclingType)
            {
                case RecyclingType.Destroy:
#if UNITY_EDITOR
                    GameObject.DestroyImmediate(gameObject);
#else
                    GameObject.Destroy(gameObject);
#endif
                    break;
                case RecyclingType.ObjectPool:
                    ReturnToPool();
                    break;
                case RecyclingType.LRUPool:
                    ReturnToPool();
                    break;
                case RecyclingType.DontHandle:
                    //gameObject.SetActive(false);
                    break;
            }
        }

        public override void OnGet()
        {
            Reset();
            base.OnGet();
        }

        public override void OnRelease()
        {
            transform.parent = ((MonoBehaviour)Pool)?.transform ?? null;

            base.OnRelease();
        }

        private void Reset()
        {
            ElapsedLifeTime = 0;

            transform.parent = null;
        }

        public void Play()
        { }

        public void Pause()
        { }

        public void Stop()
        {
            OnEffectEnd();
        }

        /// <summary>
        /// 重置特效所有状态(FX_Component, ParticleSystem, TrailRenderer)
        /// </summary>
        public void Restart()
        {
            m_LifeTime = m_CachedLifeTime;
            ElapsedLifeTime = 0;

            FX_Component[] comps = Components;
            if (comps == null)
                return;

            //foreach (var comp in comps)
            //{
            //    IReplay rp = comp as IReplay;
            //    if (rp != null)
            //    {
            //        rp.Replay();
            //    }
            //}
        }
    }
}