using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_Scale : MonoBehaviour
    {
        public bool             Addition;

        public float            Delay;
        public float            Duration;
        public bool             Loop;

        public Vector3          Target = Vector3.one;
        public AnimationCurve   Curve = new AnimationCurve(FX_Const.defaultKeyFrames);

        private float           m_Duration;
        private float           m_Delay;
        private Vector3         m_OriginalLocalScale;

        private void Awake()
        {
            m_OriginalLocalScale = transform.localScale;         // 记录初始localScale
        }
        
        private void OnEnable()
        {
            transform.localScale = m_OriginalLocalScale;         // 恢复初始localScale
            
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

            float value = Mathf.Clamp01(Curve.Evaluate(percent));
            if (Addition)
                transform.localScale = Vector3.Lerp(m_OriginalLocalScale, m_OriginalLocalScale + Target, value);
            else
                transform.localScale = Vector3.Lerp(m_OriginalLocalScale, Target, value);
        }
    }
}
