﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace MeshParticleSystem.Profiler
{
    public class ParticleProfiler
    {
        private const float                     kMaxSimulatedTime = 10.0f;
        static private GameObject               m_Inst;
        static private ParticleProfilingData    m_ProfilingData = new ParticleProfilingData();
        static public ParticleProfilingData     profilingData => m_ProfilingData;
        static private float                    m_BeginTime;
        static private float                    m_LastTime;
        static private float                    m_IntervalSampleTime;
        static public float                     elapsedTime         { get; private set; }
        static public bool                      profilingFinished   { get; private set; }

        static public void StartProfiler(string assetPath)
        {
            profilingFinished = false;

            GameObject particle = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if(particle == null)
            {
                Debug.LogError($"Can't load gameObject at path {assetPath}");
                profilingFinished = true;
                return;
            }
            
            m_Inst = Object.Instantiate(particle) as GameObject;
            m_Inst.transform.position = Vector3.zero;
            Selection.activeGameObject = m_Inst;        // 选中对象才能正确模拟，奇怪

            m_ProfilingData = new ParticleProfilingData(assetPath);
            m_ProfilingData.allParticles = m_Inst.GetComponentsInChildren<ParticleSystem>(true).ToList();
            m_ProfilingData.allFXComponents = m_Inst.GetComponentsInChildren<FX_Component>(true).ToList();
            m_ProfilingData.allMeshes = GetAllMeshes(m_Inst);
            m_ProfilingData.allMaterials = GetAllMaterials(m_Inst);
            m_ProfilingData.allTextures = GetAllTextures(m_Inst);
            m_ProfilingData.materialCount = m_ProfilingData.allMaterials.Count;
            m_ProfilingData.textureMemory = GetRuntimeMemorySizeLong(m_ProfilingData.allTextures);
            m_ProfilingData.textureMemoryOnAndroid = GetRuntimeMemorySizeLongOnAndroid(m_ProfilingData.allTextures);
            m_ProfilingData.textureMemoryOnIPhone = GetRuntimeMemorySizeLongOnIPhone(m_ProfilingData.allTextures);

            m_BeginTime = (float)EditorApplication.timeSinceStartup;
            m_LastTime = m_BeginTime;
            m_IntervalSampleTime = 0;

            EditorApplication.update += UpdateSimulate;
        }

        static private void UpdateSimulate()
        {
            float deltaTime = (float)EditorApplication.timeSinceStartup - m_LastTime;
            m_LastTime = (float)EditorApplication.timeSinceStartup;

            // simulate particle system
            foreach(var ps in m_ProfilingData.allParticles)
            {
                ps.Simulate(deltaTime, false, false);
            }

            // update stats
            m_ProfilingData.curDrawCall = UnityEditor.UnityStats.batches - 2;       // 搭建的测试环境有2个batch
            m_ProfilingData.curTriangles = UnityEditor.UnityStats.triangles;
            m_ProfilingData.curParticleCount = GetTotalParticleCount(m_ProfilingData.allParticles);

            // sample stats
            m_IntervalSampleTime += deltaTime;
            if(m_IntervalSampleTime > 0.3f)
            {
                m_IntervalSampleTime = 0;

                float time = (float)(EditorApplication.timeSinceStartup - m_BeginTime);
                m_ProfilingData.DrawCallCurve.AddKey(time, m_ProfilingData.curDrawCall);
                m_ProfilingData.TriangleCountCurve.AddKey(time, m_ProfilingData.curTriangles);
                m_ProfilingData.ParticleCountCurve.AddKey(time, m_ProfilingData.curParticleCount);
            }

            elapsedTime = (float)EditorApplication.timeSinceStartup - m_BeginTime;
            if(elapsedTime > kMaxSimulatedTime || !IsAlive(m_ProfilingData.allParticles))
            {
                profilingFinished = true;

                if(m_Inst != null)
                    Object.DestroyImmediate(m_Inst);               

                EditorApplication.update -= UpdateSimulate;
            }
        }

        static private bool IsAlive(List<ParticleSystem> psList)
        {
            foreach(var ps in psList)
            {
                ParticleSystem.MainModule main = ps.main;
                if(main.loop)
                    return true;

                if(elapsedTime < main.duration + main.startLifetime.constant)
                    return true;
            }
            return false;
        }

        static private int GetTotalParticleCount(List<ParticleSystem> psList)
        {
            int count = 0;
            foreach(var ps in psList)
            {
                count += ps.particleCount;
            }
            return count;
        }

        static private long GetRuntimeMemorySizeLong(List<Texture> texList)
        {
            long size = 0;
            foreach(var tex in texList)
            {
                size += Utility.GetRuntimeMemorySizeLong(tex);
            }
            return size;
        }

        static private long GetRuntimeMemorySizeLongOnAndroid(List<Texture> texList)
        {
            long size = 0;
            foreach(var tex in texList)
            {
                size += Utility.GetRuntimeMemorySizeLongOnAndroid(tex);
            }
            return size;
        }

        static private long GetRuntimeMemorySizeLongOnIPhone(List<Texture> texList)
        {
            long size = 0;
            foreach(var tex in texList)
            {
                size += Utility.GetRuntimeMemorySizeLongOnIPhone(tex);
            }
            return size;
        }

        static private List<Mesh> GetAllMeshes(GameObject particle)
        {
            if(particle == null)
                throw new System.ArgumentNullException("particle");

            HashSet<Mesh> meshSet = new HashSet<Mesh>();

            MeshFilter[] mfs = particle.GetComponentsInChildren<MeshFilter>(true);
            foreach(var mf in mfs)
            {
                if(mf == null || mf.sharedMesh == null) continue;
                meshSet.Add(mf.sharedMesh);
            }

            SkinnedMeshRenderer[] smrs = particle.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach(var smr in smrs)
            {
                if(smr == null || smr.sharedMesh == null) continue;
                meshSet.Add(smr.sharedMesh);
            }

            ParticleSystem[] pss = particle.GetComponentsInChildren<ParticleSystem>(true);
            foreach(var ps in pss)
            {
                var psr = ps.GetComponent<ParticleSystemRenderer>();
                if(psr.renderMode == ParticleSystemRenderMode.Mesh && psr.mesh != null)
                {
                    meshSet.Add(psr.mesh);
                }
            }
            return meshSet.ToList();
        }

        static private List<Texture> GetAllTextures(GameObject particle)
        {
            if(particle == null)
                throw new System.ArgumentNullException("particle");

            HashSet<Texture> texSet = new HashSet<Texture>();

            texSet.UnionWith(GetAllTextureSheet(particle));

            List<Material> matList = GetAllMaterials(particle);
            foreach(var mat in matList)
            {
                int count = ShaderUtil.GetPropertyCount(mat.shader);
				for (int i = 0; i < count; i++)
				{
					ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(mat.shader, i);
					if (propertyType == ShaderUtil.ShaderPropertyType.TexEnv)
					{
						string propertyName = ShaderUtil.GetPropertyName(mat.shader, i);
						Texture tex = mat.GetTexture(propertyName);
						if (tex != null)
						{
                            texSet.Add(tex);
						}
					}
				}
            }
            return texSet.ToList();
        }

        static private HashSet<Texture> GetAllTextureSheet(GameObject particle)
        {
            HashSet<Texture> texSet = new HashSet<Texture>();

            ParticleSystem[] pss = particle.GetComponentsInChildren<ParticleSystem>(true);
            foreach(var ps in pss)
            {
                ParticleSystem.TextureSheetAnimationModule tsa = ps.textureSheetAnimation;
                if(tsa.mode == ParticleSystemAnimationMode.Sprites)
                {
                    for(int i = 0; i < tsa.spriteCount; ++i)
                    {
                        Sprite s = tsa.GetSprite(i);
                        if(s != null && s.texture != null)
                        {
                            texSet.Add(s.texture);
                        }
                    }
                }
            }

            return texSet;
        }

        static private List<Material> GetAllMaterials(GameObject particle)
        {
            if(particle == null)
                throw new System.ArgumentNullException("particle");

            HashSet<Material> matSet = new HashSet<Material>();

            ParticleSystem[] pss = particle.GetComponentsInChildren<ParticleSystem>(true);
            foreach(var ps in pss)
            {
                matSet.UnionWith(GetMaterials(ps));
            }

            FX_Component[] fxs = particle.GetComponentsInChildren<FX_Component>(true);
            foreach(var fx in fxs)
            {
                matSet.UnionWith(GetMaterials(fx));
            }
            return matSet.ToList();
        }

        static private HashSet<Material> GetMaterials(ParticleSystem ps)
        {
            HashSet<Material> matSet = new HashSet<Material>();

            ParticleSystemRenderer[] psrs = ps.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach(var psr in psrs)
            {
                if(psr == null) continue;

                foreach(var mat in psr.sharedMaterials)
                {
                    if(mat == null) continue;
                    matSet.Add(mat);
                }

                if(psr.trailMaterial != null)
                    matSet.Add(psr.trailMaterial);
            }
            return matSet;
        }

        static private HashSet<Material> GetMaterials(FX_Component fxComp)
        {
            HashSet<Material> matSet = new HashSet<Material>();

            Renderer[] rdrs = fxComp.GetComponents<Renderer>();
            foreach(var rdr in rdrs)
            {
                if(rdr == null) continue;

                foreach(var mat in rdr.sharedMaterials)
                {
                    if(mat == null) continue;
                    matSet.Add(mat);
                }
            }

            return matSet;
        }
    }
}