using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_ScaleXYZ : MonoBehaviour
    {
        public bool             Addition;

        public float            Delay;
        public float            Duration;
        public bool             Loop;
        
        public Vector3          Target = Vector3.one;
        public AnimationCurve   CurveX = new AnimationCurve(FX_Const.defaultKeyFrames);
        public AnimationCurve   CurveY = new AnimationCurve(FX_Const.defaultKeyFrames);
        public AnimationCurve   CurveZ = new AnimationCurve(FX_Const.defaultKeyFrames);
        
        private float           m_Duration;
        private float           m_Delay;
        private Vector3         m_OrignalLocalScale;
        static private Vector3  k_LocalScale = Vector3.one;

        private void Awake()
        {
            m_OrignalLocalScale = transform.localScale;              // 记录初始localScale
        }

        private void OnEnable()
        {
            transform.localScale = m_OrignalLocalScale;              // 恢复初始localScale

            m_Duration = Duration;
            m_Delay = Delay;
        }

        void Update()
        {
            m_Delay -= Time.deltaTime;
            if (m_Delay > 0)
                return;

            m_Duration -= Time.deltaTime;
            float percent = 1;
            if (m_Duration >= 0)
            {
                percent = 1 - (m_Duration / Duration);
            }
            else
            {
                if (Loop)
                {
                    m_Duration += Duration;
                    percent = 1 - (m_Duration / Duration);
                }
            }

            float valueX = Mathf.Clamp01(CurveX.Evaluate(percent));
            float valueY = Mathf.Clamp01(CurveY.Evaluate(percent));
            float valueZ = Mathf.Clamp01(CurveZ.Evaluate(percent));

            if (Addition)
            {
                k_LocalScale.x = Mathf.Lerp(m_OrignalLocalScale.x, m_OrignalLocalScale.x + Target.x, valueX);
                k_LocalScale.y = Mathf.Lerp(m_OrignalLocalScale.y, m_OrignalLocalScale.y + Target.y, valueY);
                k_LocalScale.z = Mathf.Lerp(m_OrignalLocalScale.z, m_OrignalLocalScale.z + Target.z, valueZ);
            }
            else
            {
                k_LocalScale.x = Mathf.Lerp(m_OrignalLocalScale.x, Target.x, valueX);
                k_LocalScale.y = Mathf.Lerp(m_OrignalLocalScale.y, Target.y, valueY);
                k_LocalScale.z = Mathf.Lerp(m_OrignalLocalScale.z, Target.z, valueZ);
            }

            transform.localScale = k_LocalScale;
        }
    }
}
