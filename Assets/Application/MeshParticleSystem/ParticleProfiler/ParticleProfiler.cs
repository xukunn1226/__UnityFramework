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

        public List<ParticleSystem> allParticles            = new List<ParticleSystem>();
        public List<FX_Component>   allFXComponents         = new List<FX_Component>();
        public List<Texture>        allTextures             = new List<Texture>();
        public List<Material>       allMaterials            = new List<Material>();
        public List<Mesh>           allMeshes               = new List<Mesh>();
        
        public int                  materialCount           { get; set; }
        public long                 textureMemory           { get; set; }
        public long                 textureMemoryOnAndroid  { get; set; }
        public long                 textureMemoryOnIPhone   { get; set; }
        public int                  maxDrawCall             { get; set; }
        private int                 m_CurDrawCall;
        public int                  curDrawCall             { get { return m_CurDrawCall; } set { if(value > maxDrawCall) maxDrawCall = value; m_CurDrawCall = value; } }

        public int                  maxTriangles            { get; set; }
        private int                 m_CurTriangles;
        public int                  curTriangles            { get { return m_CurTriangles; } set { if(value > maxTriangles) maxTriangles = value; m_CurTriangles = value; } }

        public int                  maxParticleCount        { get; set; }
        private int                 m_CurParticleCount;
        public int                  curParticleCount        { get { return m_CurParticleCount; } set { if(value > maxParticleCount) maxParticleCount = value; m_CurParticleCount = value; } }

        public AnimationCurve       DrawCallCurve           = new AnimationCurve();
        public AnimationCurve       TriangleCountCurve      = new AnimationCurve();
        public AnimationCurve       ParticleCountCurve      = new AnimationCurve();

        private float               m_IntervalSampleTime;
        public float                elapsedTime             { get; private set; }

        void OnEnable()
        {
            allParticles            = GetComponentsInChildren<ParticleSystem>(true).ToList();
            allFXComponents         = GetComponentsInChildren<FX_Component>(true).ToList();
            allMeshes               = Utility.GetAllMeshes(gameObject);
            allMaterials            = Utility.GetAllMaterials(gameObject);
            allTextures             = Utility.GetAllTextures(gameObject);
            materialCount           = allMaterials.Count;
            textureMemory           = Utility.GetRuntimeMemorySizeLong(allTextures);
            textureMemoryOnAndroid  = Utility.GetRuntimeMemorySizeLongOnAndroid(allTextures);
            textureMemoryOnIPhone   = Utility.GetRuntimeMemorySizeLongOnIPhone(allTextures);
            curDrawCall             = 0;
            curTriangles            = 0;
            curParticleCount        = 0;
            DrawCallCurve           = new AnimationCurve();
            TriangleCountCurve      = new AnimationCurve();
            ParticleCountCurve      = new AnimationCurve();

            m_IntervalSampleTime    = 0;
            elapsedTime             = 0;
        }

        void Update()
        {
            if(!Application.isPlaying)
                return;
#if UNITY_EDITOR
            // update stats
            curDrawCall = (UnityEditor.UnityStats.batches - 2) >> 1;       // exclude "Clear" & "ImageEffects"
            curTriangles = UnityEditor.UnityStats.triangles;
            curParticleCount = Utility.GetTotalParticleCount(allParticles);
#endif

            elapsedTime = Mathf.Min(kMaxSimulatedTime, elapsedTime + Time.deltaTime);
            bool isSimulatedDone = elapsedTime > kMaxSimulatedTime - 0.01f || !Utility.IsAlive(allParticles, elapsedTime);

            // sample stats
            m_IntervalSampleTime += Time.deltaTime;
            if(!isSimulatedDone && m_IntervalSampleTime > 0.3f)
            {
                m_IntervalSampleTime = 0;

                DrawCallCurve.AddKey(elapsedTime, curDrawCall);
                TriangleCountCurve.AddKey(elapsedTime, curTriangles);
                ParticleCountCurve.AddKey(elapsedTime, curParticleCount);
            }          
        }
    }
}