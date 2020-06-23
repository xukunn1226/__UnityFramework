using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MeshParticleSystem.Profiler
{
    public class ParticleProfilerEx : MonoBehaviour
    {
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
        }

        void Update()
        {
            
        }
    }
}