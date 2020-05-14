using System.Collections;
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
        [SerializeField][HideInInspector]
        private float m_Speed = 1;

        public float speed
        {
            get { return m_Speed; }
            set
            {
                if(m_Speed != value)
                {
                    m_Speed = value;

                    InitComps();
                    foreach(var fx in m_FXComps)
                    {
                        fx.speed = value;
                    }
                    foreach(var ps in m_ParticleComps)
                    {
                        ParticleSystem.MainModule main = ps.main;
                        main.simulationSpeed = value;
                    }
                }
            }
        }

        private float deltaTime
        {
            get
            {
                if (m_State == PlayState.Play)
                    return Time.deltaTime * speed;

                return 0;
            }
        }

        enum PlayState
        {
            Play    = 1,
            Pause   = 2,
            Stop    = 4,
        }
        private PlayState m_State = PlayState.Play;

        public bool isPlaying
        {
            get
            {
                return (m_State & PlayState.Play) != 0;
            }
        }

        public bool isPaused
        {
            get
            {
                return (m_State & PlayState.Pause) != 0;
            }
        }

        public bool isStoped
        {
            get
            {
                return (m_State & PlayState.Stop) != 0;
            }
        }

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
        private float m_LifeTime = 0;

        public float ElapsedLifeTime { get; private set; }

        private FX_Component[]      m_FXComps;
        private ParticleSystem[]    m_ParticleComps;
        private bool                m_bInit;

        [System.NonSerialized]
        public bool                 m_SimulatedMode;

        private void InitComps()
        {
#if UNITY_EDITOR    // 编辑模式下为了适应组件增删改，不考虑性能总是获取
            m_FXComps = GetComponentsInChildren<FX_Component>(true);
            m_ParticleComps = GetComponentsInChildren<ParticleSystem>(true);

#else
            if (!m_bInit)
            {
                m_bInit = true;

                m_FXComps = GetComponentsInChildren<FX_Component>(true);
                m_ParticleComps = GetComponentsInChildren<ParticleSystem>(true);
            }
#endif
        }

        void Update()
        {
            if (isStoped) return;

            ElapsedLifeTime += deltaTime;            
            if (lifeTime > 0 && ElapsedLifeTime > lifeTime)
            {
                Stop();
            }

#if UNITY_EDITOR
            if (m_SimulatedMode && !Application.isPlaying && m_ParticleComps != null)
            { // 模拟模式且非运行时才执行
                foreach (var ps in m_ParticleComps)
                {
                    ps.Simulate(deltaTime, false, false, false);
                }
            }
#endif
        }

        private void OnEffectEnd()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)     // 方便美术编辑，非运行模式下不处理回收
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
            // 回收时统一放置Pool下
            transform.parent = ((MonoBehaviour)Pool)?.transform ?? null;

            base.OnRelease();
        }

        private void Reset()
        {
            ElapsedLifeTime = 0;
            transform.parent = null;
        }

        public void Play()
        {
            InitComps();

            m_State = PlayState.Play;
            foreach(var fx in m_FXComps)
            {
                fx.Play();
            }
            foreach(var ps in m_ParticleComps)
            {
                ps.Play();
            }
        }

        public void Pause()
        {
            InitComps();

            m_State = PlayState.Pause;
            foreach(var fx in m_FXComps)
            {
                fx.Pause();
            }
            foreach(var ps in m_ParticleComps)
            {
                ps.Pause();
            }
        }

        public void Stop()
        {
            InitComps();

            m_State = PlayState.Stop;
            ElapsedLifeTime = 0;

            foreach (var fx in m_FXComps)
            {
                fx.Stop();
            }
            foreach(var ps in m_ParticleComps)
            {
                ps.Clear();
                ps.Stop();
            }
            OnEffectEnd();
        }

        /// <summary>
        /// 重置特效所有状态(FX_Component, ParticleSystem, TrailRenderer)
        /// </summary>
        public void Restart()
        {
            InitComps();

            m_State = PlayState.Play;
            ElapsedLifeTime = 0;

            foreach(var fx in m_FXComps)
            {
                fx.Restart();
            }
            foreach(var ps in m_ParticleComps)
            {
                ps.Clear();
                ps.Stop();
                ps.Play();
            }
        }
    }
}