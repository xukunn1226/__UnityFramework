using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.MeshParticleSystem
{
    [ExecuteInEditMode]
    public class FX_ScaleXYZ : FX_Component
    {
        public float            Delay;
#if UNITY_2019_1_OR_NEWER
        [Min(0)]
#endif
        public float            Duration;
        
        public bool             Addition;
        public Vector3          Target = Vector3.one;
        public AnimationCurve   CurveX = new AnimationCurve(FX_Const.defaultKeyFrames);
        public AnimationCurve   CurveY = new AnimationCurve(FX_Const.defaultKeyFrames);
        public AnimationCurve   CurveZ = new AnimationCurve(FX_Const.defaultKeyFrames);
        
        private float           m_Delay;
        private Vector3         m_OriginalLocalScale;
        static private Vector3  k_LocalScale = Vector3.one;

        private void Awake()
        {
            // 记录初始localScale
            m_OriginalLocalScale = transform.localScale;            
        }

        protected override void InitEx()
        {
            base.InitEx();

            transform.localScale = m_OriginalLocalScale;         // 恢复初始localScale

            m_Delay = Delay;
        }

        void Update()
        {
            m_Delay -= deltaTime;
            if (m_Delay > 0)
                return;

            float percent = 1;
            if (Duration > 0)
            {
                elapsedTime += deltaTime;
                percent = elapsedTime / Duration;
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
