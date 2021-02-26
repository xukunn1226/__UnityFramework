using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MeshParticleSystem.Profiler
{
    [ExecuteInEditMode]
    public class ParticleProfiler : MonoBehaviour
    {
        static private float        kMaxSimulatedTime               = 5.0f;
        static public float         kRecommendedTextureMemorySize   = 1;            // 建议的贴图内存占用大小
        static public int           kRecommendTextureCount          = 5;            // 建议的贴图数量
        static public int           kRecommendMaterialCount         = 3;            // 建议的材质数量
        static public int           kRecommendParticleCompCount     = 8;            // 建议的粒子组件数量
        static public int           kRecommendDrawCallCount         = 10;           // 建议的DC数量
        static public int           kRecommendParticleCount         = 50;           // 建议的同时存在的粒子数量
        static public float         kRecommendFillrate              = 4;            // 建议的填充率
        static public int           kRecommendMeshCount             = 3;            // 建议的网格数量        

        [System.Serializable]
        public class ProfilerData
        {
            [System.NonSerialized]
            public List<ParticleSystem> allParticles            = new List<ParticleSystem>();
            [System.NonSerialized]
            public List<FX_Component>   allFXComponents         = new List<FX_Component>();
            [System.NonSerialized]
            public List<Texture>        allTextures             = new List<Texture>();
            [System.NonSerialized]
            public List<Material>       allMaterials            = new List<Material>();
            [System.NonSerialized]
            public List<Mesh>           allMeshes               = new List<Mesh>();
        
            public int                  componentCount;
            public int                  materialCount;
            public long                 textureMemory;
            public long                 textureMemoryOnAndroid;
            public long                 textureMemoryOnIPhone;
            public int                  maxDrawCall;
            private int                 m_CurDrawCall;
            public int                  curDrawCall             { get { return m_CurDrawCall; } set { if(value > maxDrawCall) maxDrawCall = value; m_CurDrawCall = value; } }

            public int                  maxTriangles;
            private int                 m_CurTriangles;
            public int                  curTriangles            { get { return m_CurTriangles; } set { if(value > maxTriangles) maxTriangles = value; m_CurTriangles = value; } }

            public int                  maxParticleCount;
            private int                 m_CurParticleCount;
            public int                  curParticleCount        { get { return m_CurParticleCount; } set { if(value > maxParticleCount) maxParticleCount = value; m_CurParticleCount = value; } }

            public AnimationCurve       DrawCallCurve           = new AnimationCurve();
            public AnimationCurve       TriangleCountCurve      = new AnimationCurve();
            public AnimationCurve       ParticleCountCurve      = new AnimationCurve();
        }

        public ProfilerData m_Data = new ProfilerData();

        private float               m_IntervalSampleTime;
        public float                elapsedTime             { get; private set; }
        public bool                 isSimulatedDone         { get; private set; }

        void OnEnable()
        {
            m_Data.allParticles             = GetComponentsInChildren<ParticleSystem>(true).ToList();
            m_Data.allFXComponents          = GetComponentsInChildren<FX_Component>(true).ToList();
            m_Data.allMeshes                = Utility.GetAllMeshes(gameObject);
            m_Data.allMaterials             = Utility.GetAllMaterials(gameObject);
            m_Data.allTextures              = Utility.GetAllTextures(gameObject);
            m_Data.componentCount           = m_Data.allParticles.Count + m_Data.allFXComponents.Count;
            m_Data.materialCount            = m_Data.allMaterials.Count;
            m_Data.textureMemory            = Utility.GetRuntimeMemorySizeLong(m_Data.allTextures);
            m_Data.textureMemoryOnAndroid   = Utility.GetRuntimeMemorySizeLongOnAndroid(m_Data.allTextures);
            m_Data.textureMemoryOnIPhone    = Utility.GetRuntimeMemorySizeLongOnIPhone(m_Data.allTextures);
            m_Data.curDrawCall              = 0;
            m_Data.curTriangles             = 0;
            m_Data.curParticleCount         = 0;
            m_Data.DrawCallCurve            = new AnimationCurve();
            m_Data.TriangleCountCurve       = new AnimationCurve();
            m_Data.ParticleCountCurve       = new AnimationCurve();

            m_IntervalSampleTime            = 0;
            elapsedTime                     = 0;            
        }

        void Update()
        {
            if(!UnityEngine.Application.isPlaying)
                return;
#if UNITY_EDITOR
            // update stats
            m_Data.curDrawCall = (UnityEditor.UnityStats.batches - 2) >> 1;       // exclude "Clear" & "ImageEffects"
            m_Data.curTriangles = UnityEditor.UnityStats.triangles;
            m_Data.curParticleCount = Utility.GetTotalParticleCount(m_Data.allParticles);
#endif

            elapsedTime = Mathf.Min(kMaxSimulatedTime, elapsedTime + Time.deltaTime);
            isSimulatedDone = elapsedTime > kMaxSimulatedTime - 0.01f || !Utility.IsAlive(m_Data.allParticles, elapsedTime);

            // sample stats
            m_IntervalSampleTime += Time.deltaTime;
            if(!isSimulatedDone && m_IntervalSampleTime > 0.3f)
            {
                m_IntervalSampleTime = 0;

                m_Data.DrawCallCurve.AddKey(elapsedTime, m_Data.curDrawCall);
                m_Data.TriangleCountCurve.AddKey(elapsedTime, m_Data.curTriangles);
                m_Data.ParticleCountCurve.AddKey(elapsedTime, m_Data.curParticleCount);
            }
        }
    }
}