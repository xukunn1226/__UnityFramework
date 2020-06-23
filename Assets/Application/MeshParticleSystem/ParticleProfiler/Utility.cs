using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;

namespace MeshParticleSystem.Profiler
{
    static public class Utility
    {
        static public MethodInfo m_GetRuntimeMemorySizeLong = null;                 // 运行时纹理内存大小
        static public MethodInfo m_GetTextureFormat = null;                         // 当前平台纹理格式

        static public long GetRuntimeMemorySizeLong(Texture texture)
        {
            if(m_GetRuntimeMemorySizeLong == null)
            {
                m_GetRuntimeMemorySizeLong = typeof(AssetDatabase).Assembly.GetType("UnityEditor.TextureUtil").GetMethod("GetRuntimeMemorySizeLong", BindingFlags.Public | BindingFlags.Static);
            }
            return (long)(m_GetRuntimeMemorySizeLong?.Invoke(null, new object[] {texture}) ?? 0);
        }

        static public string GetTextureFormatString(Texture texture)
        {
            if(m_GetTextureFormat == null)
            {
                m_GetTextureFormat = typeof(AssetDatabase).Assembly.GetType("UnityEditor.TextureUtil").GetMethod("GetTextureFormat", BindingFlags.Public | BindingFlags.Static);
            }
            return m_GetTextureFormat?.Invoke(null, new object[] {texture}).ToString() ?? null;
        }

        static public long GetRuntimeMemorySizeLongOnAndroid(Texture texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;
            if(ti == null)
                return 0;

            long size = texture.width * texture.height * (ti.DoesSourceTextureHaveAlpha() ? 8 : 4) / 8;
            if(ti.mipmapEnabled)
                size = size * 13 / 10;

            return size;
        }

        static public long GetRuntimeMemorySizeLongOnIPhone(Texture texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;
            if(ti == null)
                return 0;

            float size = texture.width * texture.height * (3.56f) / 8;
            if(texture.mipmapCount > 0)
                size = size * 1.3f;

            return (long)size;
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

        static public long GetRuntimeMemorySizeLong(List<Texture> texList)
        {
            long size = 0;
            foreach(var tex in texList)
            {
                size += GetRuntimeMemorySizeLong(tex);
            }
            return size;
        }

        static public long GetRuntimeMemorySizeLongOnAndroid(List<Texture> texList)
        {
            long size = 0;
            foreach(var tex in texList)
            {
                size += GetRuntimeMemorySizeLongOnAndroid(tex);
            }
            return size;
        }

        static public long GetRuntimeMemorySizeLongOnIPhone(List<Texture> texList)
        {
            long size = 0;
            foreach(var tex in texList)
            {
                size += GetRuntimeMemorySizeLongOnIPhone(tex);
            }
            return size;
        }

        static public List<Mesh> GetAllMeshes(GameObject particle)
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

        static public List<Texture> GetAllTextures(GameObject particle)
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

        static public HashSet<Texture> GetAllTextureSheet(GameObject particle)
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

        static public List<Material> GetAllMaterials(GameObject particle)
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

        static public HashSet<Material> GetMaterials(ParticleSystem ps)
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

        static public HashSet<Material> GetMaterials(FX_Component fxComp)
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