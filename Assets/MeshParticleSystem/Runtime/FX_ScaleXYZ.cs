using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_ScaleXYZ : FX_Component, IReplay
    {
        public float            Delay;
        [Min(0)]
        public float            Duration;
        
        public bool             Addition;
        public Vector3          Target = Vector3.one;
        public AnimationCurve   CurveX = new AnimationCurve(FX_Const.defaultKeyFrames);
        public AnimationCurve   CurveY = new AnimationCurve(FX_Const.defaultKeyFrames);
        public AnimationCurve   CurveZ = new AnimationCurve(FX_Const.defaultKeyFrames);
        
        private float           m_ElapsedTime;
        private float           m_Delay;
        private Vector3         m_OriginalLocalScale;
        static private Vector3  k_LocalScale = Vector3.one;

        private void Awake()
        {
            // 记录初始localScale
            m_OriginalLocalScale = transform.localScale;            
        }

        private void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            transform.localScale = m_OriginalLocalScale;         // 恢复初始localScale

            m_ElapsedTime = 0;
            m_Delay = Delay;
        }

        public void Replay()
        {
            enabled = !enabled;
        }

        void Update()
        {
            m_Delay -= Time.deltaTime;
            if (m_Delay > 0)
                return;

            float percent = 1;
            if (Duration > 0)
            {
                m_ElapsedTime += Time.deltaTime;
                percent = m_ElapsedTime / Duration;
            }
            float valueX = CurveX.Evaluate(percent);
            float valueY = CurveY.Evaluate(percent);
            float valueZ = CurveZ.Evaluate(percent);

            if (Addition)
            {
                k_LocalScale.x = Mathf.Lerp(m_OriginalLocalScale.x, m_OriginalLocalScale.x + Target.x, valueX);
                k_LocalScale.y = Mathf.Lerp(m_OriginalLocalScale.y, m_OriginalLocalScale.y + Target.y, valueY);
                k_LocalScale.z = Mathf.Lerp(m_OriginalLocalScale.z, m_OriginalLocalScale.z + Target.z, valueZ);
            }
            else
            {
                k_LocalScale.x = Mathf.Lerp(m_OriginalLocalScale.x, Target.x, valueX);
                k_LocalScale.y = Mathf.Lerp(m_OriginalLocalScale.y, Target.y, valueY);
                k_LocalScale.z = Mathf.Lerp(m_OriginalLocalScale.z, Target.z, valueZ);
            }

            transform.localScale = k_LocalScale;
        }
    }
}
