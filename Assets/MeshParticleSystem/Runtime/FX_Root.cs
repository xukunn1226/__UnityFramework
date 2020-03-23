using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_Root : MonoBehaviour
    {
        public float    LifeTime;
        private float   m_LifeTime;

        private void OnEnable()
        {
            m_LifeTime = LifeTime;
        }

        void Update()
        {
            if (LifeTime <= 0) { return; }

            m_LifeTime -= Time.deltaTime;

            if (m_LifeTime <= 0)
            {
                GameObject.Destroy(gameObject);
            }
        }

        // 重置特效所有状态(FX_Rotation, FX_Transition, FX_Animation, FX_CustomPropertiesTransfer, ParticleSystem, TrailRenderer)
        public void Replay()
        {

        }
    }
}