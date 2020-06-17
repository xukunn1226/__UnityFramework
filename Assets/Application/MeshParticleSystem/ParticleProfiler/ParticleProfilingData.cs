using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MeshParticleSystem.Profiler
{
    [System.Serializable]
    public class ParticleProfilingData
    {
        private List<ParticleSystem> m_Particles = new List<ParticleSystem>();
        private List<FX_Component> m_FXComponents = new List<FX_Component>();

        private HashSet<Texture> m_Textures = new HashSet<Texture>();

        public ParticleProfilingData(GameObject particle)
        {
            if(particle == null)
                throw new System.ArgumentNullException("");

            particle.GetComponentsInChildren<ParticleSystem>(true, m_Particles);
        }

        static private HashSet<Texture> s_TexturesSet = new HashSet<Texture>();
        static private HashSet<Texture> GetTextures(ParticleSystem ps)
        {
            s_TexturesSet.Clear();

            ParticleSystem.TextureSheetAnimationModule tsa = ps.textureSheetAnimation;
            if(tsa.mode == ParticleSystemAnimationMode.Sprites)
            {
                for(int i = 0; i < tsa.spriteCount; ++i)
                {
                    Sprite s = tsa.GetSprite(i);
                    if( s != null && s.texture != null )
                    {
                        s_TexturesSet.Add(s.texture);
                    }
                }
            }

            // ParticleSystemRenderer[] psrs = ps.GetComponentsInChildren<ParticleSystemRenderer>(true);
            // List<Material> mats = new List<Material>();
            // foreach(var psr in psrs)
            // {
            //     psr.sharedMaterials.CopyTo(mats.ToArray(), 0);

            //     if(psr.trailMaterial != null)
            //         mats.Append(psr.trailMaterial);
            // }

            return s_TexturesSet;
        }

        static private HashSet<Material> s_MaterialsSet = new HashSet<Material>();
        static private HashSet<Material> GetMaterials(ParticleSystem ps)
        {
            s_MaterialsSet.Clear();

            ParticleSystemRenderer[] psrs = ps.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach(var psr in psrs)
            {
                if(psr == null) continue;

                foreach(var mat in psr.sharedMaterials)
                {
                    if(mat == null) continue;
                    s_MaterialsSet.Add(mat);
                }
            }
            return s_MaterialsSet;
        }
    }
}