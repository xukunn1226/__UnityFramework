using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem.Profiler
{
    [System.Serializable]
    public class ParticleProfilingData
    {
        public string               assetPath;
        public List<ParticleSystem> allParticles    = new List<ParticleSystem>();
        public List<Texture>        allTextures     = new List<Texture>();
        public List<Material>       allMaterials    = new List<Material>();
        public List<Mesh>           allMeshes       = new List<Mesh>();

        public int                  materialCount   { get; set; }
        public long                 textureMemory   { get; set; }
        public int                  maxDrawCall     { get; set; }
        private int                 m_CurDrawCall;
        public int                  curDrawCall
        {
            get { return m_CurDrawCall; }
            set
            {
                if(value > maxDrawCall)
                {
                    maxDrawCall = value;
                }
                m_CurDrawCall = value;
            }
        }

        public int                  maxTriangles    { get; set; }
        private int                 m_CurTriangles;
        public int                  curTriangles
        {
            get { return m_CurTriangles; }
            set
            {
                if(value > maxTriangles)
                {
                    maxTriangles = value;
                }
                m_CurTriangles = value;
            }
        }

        public int                  maxParticleCount    { get; set; }
        private int                 m_CurParticleCount;
        public int                  curParticleCount
        {
            get { return m_CurParticleCount; }
            set
            {
                if(value > maxParticleCount)
                {
                    maxParticleCount = value;
                }
                m_CurParticleCount = value;
            }
        }

        public AnimationCurve DrawCallCurve         = new AnimationCurve();
        public AnimationCurve TriangleCountCurve    = new AnimationCurve();
        public AnimationCurve ParticleCountCurve    = new AnimationCurve();

        public ParticleProfilingData() {}
        public ParticleProfilingData(string assetPath)
        {
            this.assetPath = assetPath;
        }

        public override string ToString()
        {
            return null;
        }

        public string ToJson()
        {
            return null;
        }
    }
}