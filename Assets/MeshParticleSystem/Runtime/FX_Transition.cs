using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_Transition : MonoBehaviour
    {
        public bool             Addition;
        public float            Delay;
        public float            Duration;
        public bool             Loop;

        public bool             WorldSpace;        
        public Vector3          Target;
        public AnimationCurve   Curve = new AnimationCurve(FX_Const.defaultKeyFrames);
        
        private float           m_Duration;
        private float           m_Delay;
        private Vector3         m_OriginalLocalPos;
        private Vector3         m_OriginalWorldPos;
        private Vector3         m_StartLocalPos;
        private Vector3         m_StartWorldPos;

        private void Awake()
        {
            m_OriginalLocalPos = transform.localPosition;
            m_OriginalWorldPos = transform.position;
        }

        void OnEnable()
        {
            // 恢复初始位置
            transform.position = m_OriginalWorldPos;
            transform.localPosition = m_OriginalLocalPos;

            // 重复使用时需要先设置到正确位置，再active
            m_StartWorldPos = transform.position;
            m_StartLocalPos = transform.localPosition;

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

            float value = Curve.Evaluate(percent);

            if (WorldSpace)
            {
                if(Addition)
                    transform.position = Vector3.Lerp(m_StartWorldPos, m_StartWorldPos + Target, value);
                else
                    transform.position = Vector3.Lerp(m_StartWorldPos, Target, value);
            }
            else
            {
                if(Addition)
                    transform.localPosition = Vector3.Lerp(m_StartLocalPos, m_StartLocalPos + Target, value);
                else
                    transform.localPosition = Vector3.Lerp(m_StartLocalPos, Target, value);
            }
        }
    }
}
